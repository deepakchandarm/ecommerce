using WebApi.Dao;

namespace WebApi.Interface
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategories();
        Task<Category> AddCategory(Category category);
        Task<Category> GetCategoryById(int categoryId);
        Task<Category> GetCategoryByName(string name);
        Task DeleteCategoryById(int categoryId);
        Task<Category> UpdateCategory(Category category, int categoryId);
    }
}
