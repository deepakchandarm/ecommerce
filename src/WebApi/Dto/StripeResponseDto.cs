namespace WebApi.Dto
{
    public class StripeResponseDto
    {
        public string SessionId { get; set; }
        public string PublicKey { get; set; }
        public string ClientSecret { get; set; }
        public string PaymentUrl { get; set; }
    }
}
