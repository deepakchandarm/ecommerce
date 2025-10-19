namespace WebApi.Dao
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentIntentId { get; set; } // Stripe Payment Intent ID
        public string PaymentStatus { get; set; } // "processing", "succeeded", "failed"
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public User User { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
