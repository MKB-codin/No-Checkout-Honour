namespace HonourSelfCheckoutServer.Models
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }
    }
}

