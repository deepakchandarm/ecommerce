namespace WebApi.Dto
{
    public class ProductUpdateRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; }
    }
}
