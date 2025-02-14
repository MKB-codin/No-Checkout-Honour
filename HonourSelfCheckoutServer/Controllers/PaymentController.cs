using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;

namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;

        public PaymentController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        // POST: api/Payment/CreatePaymentIntent
        [HttpPost("CreatePaymentIntent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CheckoutRequest request)
        {
            // Convert total to the smallest currency unit, e.g. pence for GBP.
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Total * 100), // For example, 12.34 GBP becomes 1234 pence
                Currency = "gbp",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", request.UserId.ToString() },
                    { "StoreId", request.StoreId.ToString() }
                }
            };
            var service = new PaymentIntentService();
            PaymentIntent intent = await service.CreateAsync(options);

            // In test mode, we can simulate a payment confirmation automatically.
            // The test payment method "pm_card_visa" simulates a successful card payment.
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = "pm_card_visa"
            };
            PaymentIntent confirmedIntent = await service.ConfirmAsync(intent.Id, confirmOptions);

            return Ok(new { ClientSecret = confirmedIntent.ClientSecret });
        }

        // POST: api/Payment/FinalizeCheckout
        [HttpPost("FinalizeCheckout")]
        public async Task<IActionResult> FinalizeCheckout([FromBody] CheckoutFinalizationRequest request)
        {
            // Here, you would normally verify the payment status (if needed) and then create a Receipt record.
            // For simplicity, assume payment is successful.

            // Create Receipt
            Receipt receipt = new Receipt
            {
                StoreId = request.StoreId,
                UserId = request.UserId,
                Total = decimal.Parse(request.Total.ToString()),
                PurchaseDate = DateTime.UtcNow
            };
            _databaseContext.Receipts.Add(receipt);
            await _databaseContext.SaveChangesAsync();

            // Create ReceiptItems for each cart item
            foreach (var item in request.CartItems)
            {
                ReceiptItem receiptItem = new ReceiptItem
                {
                    ReceiptId = receipt.ReceiptId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                _databaseContext.ReceiptItems.Add(receiptItem);
            }
            await _databaseContext.SaveChangesAsync();

            return Ok(new { Message = "Checkout successful", ReceiptId = receipt.ReceiptId });
        }
        [HttpPost("CreateCheckoutSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            // Create options for a Stripe Checkout Session.
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                // The amount should be in the smallest currency unit (e.g., pence for GBP).
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "gbp",
                            UnitAmount = (long)(request.Total * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Total Order" // You can customize this as needed.
                            }
                        },
                        Quantity = 1
                    }
                },
                SuccessUrl = $"{request.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = request.CancelUrl
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return Ok(new { Url = session.Url });
        }

        [HttpGet("payment-success")]
        public IActionResult PaymentSuccess()
        {
            return Ok(new { Message = "Payment successful! Your receipt has been generated." });
        }

        [HttpGet("payment-cancel")]
        public IActionResult PaymentCancel()
        {
            return Ok(new { Message = "Payment cancelled. No charges were made." });
        }

    }

    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }
    }

    public class CheckoutFinalizationRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }
        public List<CartItemRequest> CartItems { get; set; }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class CheckoutSessionRequest
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public double Total { get; set; }
        // The URLs your app should navigate to on success or cancel. 
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
