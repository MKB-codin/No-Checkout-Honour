namespace HonourSelfCheckoutServer.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeId { get; set; }

        public List<StoreProduct> StoreProducts { get; set; }
    }
}
