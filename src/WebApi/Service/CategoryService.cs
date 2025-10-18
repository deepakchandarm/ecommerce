using Microsoft.EntityFrameworkCore;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Interface;

namespace WebApi.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ApplicationDbContext context,
            ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                _logger.LogInformation($"Fetched {categories.Count} categories");
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching all categories: {ex.Message}");
                throw new InvalidOperationException("Failed to fetch categories", ex);
            }
        }

        public async Task<Category> AddCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    throw new ArgumentException("Category name cannot be empty");
                }

                // Check if category already exists
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    throw new AlreadyExistException($"Category '{category.Name}' already exists");
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category '{category.Name}' added successfully with ID {category.Id}");
                return category;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while adding category: {ex.Message}");
                throw new InvalidOperationException("Error adding category", ex);
            }
        }

        public async Task<Category> GetCategoryById(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                var category = await _context.Categories.FindAsync(categoryId);

                if (category == null)
                {
                    throw new ResourceNotFoundException($"Category with id {categoryId} not found");
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching category by ID {categoryId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Category> GetCategoryByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Category name cannot be empty");
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (category == null)
                {
                    throw new ResourceNotFoundException($"Category with name '{name}' not found");
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching category by name '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCategoryById(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                var category = await _context.Categories.FindAsync(categoryId);

                if (category == null)
                {
                    throw new ResourceNotFoundException($"Category with id {categoryId} not found");
                }

                // Check if category has associated products
                var productsInCategory = await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .CountAsync();

                if (productsInCategory > 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot delete category '{category.Name}' as it has {productsInCategory} associated products");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category with ID {categoryId} deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while deleting category: {ex.Message}");
                throw new InvalidOperationException("Error deleting category", ex);
            }
        }

        public async Task<Category> UpdateCategory(Category category, int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    throw new ArgumentException("Category name cannot be empty");
                }

                var existingCategory = await _context.Categories.FindAsync(categoryId);

                if (existingCategory == null)
                {
                    throw new ResourceNotFoundException($"Category with id {categoryId} not found");
                }

                // Check if name is being changed to an existing category name
                if (!existingCategory.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicateCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != categoryId);

                    if (duplicateCategory != null)
                    {
                        throw new AlreadyExistException($"Category '{category.Name}' already exists");
                    }
                }

                // Update properties
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category with ID {categoryId} updated successfully");
                return existingCategory;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while updating category: {ex.Message}");
                throw new InvalidOperationException("Error updating category", ex);
            }
        }
    }
}
