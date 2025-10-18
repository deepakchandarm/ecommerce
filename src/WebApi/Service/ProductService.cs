using Microsoft.EntityFrameworkCore;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Service
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.LogInformation($"Fetched {products.Count} products");
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching all products: {ex.Message}");
                throw new InvalidOperationException("Failed to fetch products", ex);
            }
        }

        public async Task<Product> GetProductById(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    throw new ArgumentException("Invalid product ID");
                }

                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    throw new ResourceNotFoundException($"Product with id {productId} not found");
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching product by ID {productId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Product> AddProduct(AddProductRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    throw new ArgumentException("Product name cannot be empty");
                }

                if (request.Price <= 0)
                {
                    throw new ArgumentException("Product price must be greater than 0");
                }

                if (request.Quantity < 0)
                {
                    throw new ArgumentException("Product quantity cannot be negative");
                }

                // Check if product already exists with same name and brand
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == request.Name.ToLower()
                        && p.Brand.ToLower() == request.Brand.ToLower());

                if (existingProduct != null)
                {
                    throw new AlreadyExistException(
                        $"Product '{request.Name}' with brand '{request.Brand}' already exists");
                }

                // Verify category exists
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    throw new ResourceNotFoundException($"Category with id {request.CategoryId} not found");
                }

                var product = new Product
                {
                    Name = request.Name,
                    Brand = request.Brand,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    CategoryId = request.CategoryId
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Reload with category
                await _context.Entry(product).Reference(p => p.Category).LoadAsync();

                _logger.LogInformation($"Product '{product.Name}' added successfully with ID {product.Id}");
                return product;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while adding product: {ex.Message}");
                throw new InvalidOperationException("Error adding product", ex);
            }
        }

        public async Task<Product> UpdateProduct(ProductUpdateRequest request, int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    throw new ArgumentException("Invalid product ID");
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    throw new ArgumentException("Product name cannot be empty");
                }

                if (request.Price <= 0)
                {
                    throw new ArgumentException("Product price must be greater than 0");
                }

                if (request.Quantity < 0)
                {
                    throw new ArgumentException("Product quantity cannot be negative");
                }

                var existingProduct = await _context.Products.FindAsync(productId);

                if (existingProduct == null)
                {
                    throw new ResourceNotFoundException($"Product with id {productId} not found");
                }

                // Verify category exists
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    throw new ResourceNotFoundException($"Category with id {request.CategoryId} not found");
                }

                // Check if name/brand combination is being changed to existing product
                if (!existingProduct.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) ||
                    !existingProduct.Brand.Equals(request.Brand, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicateProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Name.ToLower() == request.Name.ToLower()
                            && p.Brand.ToLower() == request.Brand.ToLower()
                            && p.Id != productId);

                    if (duplicateProduct != null)
                    {
                        throw new AlreadyExistException(
                            $"Product '{request.Name}' with brand '{request.Brand}' already exists");
                    }
                }

                existingProduct.Name = request.Name;
                existingProduct.Brand = request.Brand;
                existingProduct.Price = request.Price;
                existingProduct.Quantity = request.Quantity;
                existingProduct.CategoryId = request.CategoryId;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                // Reload with category
                await _context.Entry(existingProduct).Reference(p => p.Category).LoadAsync();

                _logger.LogInformation($"Product with ID {productId} updated successfully");
                return existingProduct;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while updating product: {ex.Message}");
                throw new InvalidOperationException("Error updating product", ex);
            }
        }

        public async Task DeleteProductById(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    throw new ArgumentException("Invalid product ID");
                }

                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    throw new ResourceNotFoundException($"Product with id {productId} not found");
                }

                // Check if product is in any cart items
                var cartItems = await _context.CartItems
                    .Where(ci => ci.ProductId == productId)
                    .CountAsync();

                if (cartItems > 0)
                {
                    _logger.LogWarning($"Product {productId} has {cartItems} cart items");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product with ID {productId} deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while deleting product: {ex.Message}");
                throw new InvalidOperationException("Error deleting product", ex);
            }
        }

        public async Task<List<Product>> GetProductsByBrandAndName(string brand, string productName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(productName))
                {
                    throw new ArgumentException("Brand and product name cannot be empty");
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Brand.ToLower().Contains(brand.ToLower())
                        && p.Name.ToLower().Contains(productName.ToLower()))
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching products by brand '{brand}' and name '{productName}': {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAndBrand(string category, string brand)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(brand))
                {
                    throw new ArgumentException("Category and brand cannot be empty");
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category.Name.ToLower() == category.ToLower()
                        && p.Brand.ToLower().Contains(brand.ToLower()))
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching products by category '{category}' and brand '{brand}': {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> GetProductsByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Product name cannot be empty");
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching products by name '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> GetProductsByBrand(string brand)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(brand))
                {
                    throw new ArgumentException("Brand cannot be empty");
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Brand.ToLower().Contains(brand.ToLower()))
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching products by brand '{brand}': {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new ArgumentException("Category cannot be empty");
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category.Name.ToLower() == category.ToLower())
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching products by category '{category}': {ex.Message}");
                throw;
            }
        }

        public async Task<int> CountProductsByBrandAndName(string brand, string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Brand and name cannot be empty");
                }

                var count = await _context.Products
                    .Where(p => p.Brand.ToLower().Contains(brand.ToLower())
                        && p.Name.ToLower().Contains(name.ToLower()))
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error counting products by brand '{brand}' and name '{name}': {ex.Message}");
                throw;
            }
        }

        public List<ProductDto> ConvertToDto(List<Product> products)
        {
            return products.Select(ConvertToDto).ToList();
        }

        public ProductDto ConvertToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Unknown Category"
            };
        }
    }
}
