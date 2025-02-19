using HonourSelfCheckoutServer.Controllers;

namespace HonourSelfCheckoutServer.Models
{

    public class CheckoutFinalizationRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }
        public List<CartItemRequest> CartItems { get; set; }
    }
}
