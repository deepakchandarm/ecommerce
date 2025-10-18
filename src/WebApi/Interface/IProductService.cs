using WebApi.Dao;
using WebApi.Dto;

namespace WebApi.Interface
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProducts();
        Task<Product> GetProductById(int productId);
        Task<Product> AddProduct(AddProductRequest request);
        Task<Product> UpdateProduct(ProductUpdateRequest request, int productId);
        Task DeleteProductById(int productId);
        Task<List<Product>> GetProductsByBrandAndName(string brand, string productName);
        Task<List<Product>> GetProductsByCategoryAndBrand(string category, string brand);
        Task<List<Product>> GetProductsByName(string name);
        Task<List<Product>> GetProductsByBrand(string brand);
        Task<List<Product>> GetProductsByCategory(string category);
        Task<int> CountProductsByBrandAndName(string brand, string name);
        List<ProductDto> ConvertToDto(List<Product> products);
        ProductDto ConvertToDto(Product product);
    }
}
