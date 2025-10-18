using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all products.
        /// </summary>
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProducts();
                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching all products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Retrieve a specific product by ID.
        /// </summary>
        [HttpGet("product/{productId}/product")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _productService.GetProductById(productId);
                var productDto = _productService.ConvertToDto(product);
                return Ok(new ApiResponse<ProductDto>("success", productDto));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Product not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid product ID: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Add a new product (Admin only).
        /// </summary>
        [HttpPost("add")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("Product request cannot be null", null));
                }

                var product = await _productService.AddProduct(request);
                var productDto = _productService.ConvertToDto(product);
                return Ok(new ApiResponse<ProductDto>("Add product success!", productDto));
            }
            catch (AlreadyExistException ex)
            {
                _logger.LogWarning($"Product already exists: {ex.Message}");
                return StatusCode(StatusCodes.Status409Conflict,
                    new ApiResponse<string>(ex.Message, null));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Resource not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid product data: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Update an existing product (Admin only).
        /// </summary>
        [HttpPut("product/{productId}/update")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateRequest request, int productId)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("Product request cannot be null", null));
                }

                var product = await _productService.UpdateProduct(request, productId);
                var productDto = _productService.ConvertToDto(product);
                return Ok(new ApiResponse<ProductDto>("Update product success!", productDto));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Product not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (AlreadyExistException ex)
            {
                _logger.LogWarning($"Product already exists: {ex.Message}");
                return StatusCode(StatusCodes.Status409Conflict,
                    new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid product data: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Delete a product (Admin only).
        /// </summary>
        [HttpDelete("product/{productId}/delete")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                await _productService.DeleteProductById(productId);
                return Ok(new ApiResponse<int>("Delete product success!", productId));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"Product not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid product ID: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Search products by brand and name.
        /// </summary>
        [HttpGet("products/by/brand-and-name")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByBrandAndName([FromQuery] string brandName, [FromQuery] string productName)
        {
            try
            {
                var products = await _productService.GetProductsByBrandAndName(brandName, productName);
                if (products.Count == 0)
                {
                    return NotFound(new ApiResponse<string>("No products found", null));
                }

                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameters: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Search products by category and brand.
        /// </summary>
        [HttpGet("products/by/category-and-brand")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByCategoryAndBrand([FromQuery] string category, [FromQuery] string brand)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAndBrand(category, brand);
                if (products.Count == 0)
                {
                    return NotFound(new ApiResponse<string>("No products found", null));
                }

                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameters: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Search products by name.
        /// </summary>
        [HttpGet("products/{name}/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByName(string name)
        {
            try
            {
                var products = await _productService.GetProductsByName(name);
                if (products.Count == 0)
                {
                    return NotFound(new ApiResponse<ProductDto>("No products found", null));
                }

                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameter: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Search products by brand.
        /// </summary>
        [HttpGet("product/by-brand")]
        [AllowAnonymous]
        public async Task<IActionResult> FindProductByBrand([FromQuery] string brand)
        {
            try
            {
                var products = await _productService.GetProductsByBrand(brand);
                if (products.Count == 0)
                {
                    return NotFound(new ApiResponse<string>("No products found", null));
                }

                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameter: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Search products by category.
        /// </summary>
        [HttpGet("product/{category}/all/products")]
        [AllowAnonymous]
        public async Task<IActionResult> FindProductByCategory(string category)
        {
            try
            {
                var products = await _productService.GetProductsByCategory(category);
                if (products.Count == 0)
                {
                    return NotFound(new ApiResponse<string>("No products found", null));
                }

                var convertedProducts = _productService.ConvertToDto(products);
                return Ok(new ApiResponse<List<ProductDto>>("success", convertedProducts));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameter: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }

        /// <summary>
        /// Count products by brand and name.
        /// </summary>
        [HttpGet("product/count/by-brand/and-name")]
        [AllowAnonymous]
        public async Task<IActionResult> CountProductsByBrandAndName([FromQuery] string brand, [FromQuery] string name)
        {
            try
            {
                var productCount = await _productService.CountProductsByBrandAndName(brand, name);
                return Ok(new ApiResponse<int>("Product count!", productCount));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid search parameters: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error counting products: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("error", ex.Message));
            }
        }
    }
}
