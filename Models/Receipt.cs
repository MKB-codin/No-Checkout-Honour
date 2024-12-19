namespace HonourSelfCheckoutServer.Models
{
    public class Receipt
    {
        public int ReceiptId { get; set; }
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
