namespace HonourSelfCheckoutServer.Models
{
    public class CheckoutSessionRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
