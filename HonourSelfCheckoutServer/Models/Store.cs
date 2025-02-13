namespace HonourSelfCheckoutServer.Models
{
    public class Store
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }

        public List<StoreProduct> StoreProducts { get; set; }
    }
}
