namespace HonourSelfCheckoutServer.Models
{
    public class ReceiptItem
    {
        public int ItemId { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
