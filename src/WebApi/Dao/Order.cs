namespace WebApi.Dao
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public User User { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
