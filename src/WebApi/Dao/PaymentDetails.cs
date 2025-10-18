namespace WebApi.Dao
{
    public class PaymentDetails
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public int OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public Order Order { get; set; }
    }
}
