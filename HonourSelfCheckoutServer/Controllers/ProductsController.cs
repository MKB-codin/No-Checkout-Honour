using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using Microsoft.EntityFrameworkCore;

namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public ProductsController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }


        /*        [HttpGet("{barcode}")]
                public async Task<IActionResult> GetProductByBarcode(string barcode)
                {
                    Console.WriteLine($"Barcode received {barcode}");
                    var product = await _databaseContext.Products.FirstOrDefaultAsync( p => p.BarcodeId == barcode);
                    if (product == null)
                    {
                        return NotFound(new { Message = "Product not found" });
                    }

                    return Ok(product);
                }*/ //Depreciated because we need to get the product for a specific store.

        [HttpGet("GetProductByBarcode/{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode, [FromQuery] int storeId)
        {
            Console.WriteLine($"Barcode received {barcode}");

            var allStoreProducts = await _databaseContext.StoreProducts
                                        .Include(sp => sp.Product)
                                        .ToListAsync();  // Fetch all store products for debugging

            foreach (var sp in allStoreProducts)
            {
                Console.WriteLine($"DB Entry - StoreId: {sp.StoreId}, ProductId: {sp.ProductId}, Barcode: {sp.Product.BarcodeId}, Price: {sp.Price}");
            }

            var storeProduct = await _databaseContext.StoreProducts
                                        .Include(sp => sp.Product)
                                        .FirstOrDefaultAsync(sp => sp.Product.BarcodeId == barcode && sp.StoreId == storeId);

            if (storeProduct == null)
            {
                Console.WriteLine("Product not found for the selected store.");
                return NotFound(new { Message = "Product not found for the selected store." });
            }


            // Return a response that includes product details and the store-specific price.
            var response = new
            {
                ProductId = storeProduct.Product.ProductId,
                ProductName = storeProduct.Product.ProductName,
                BarcodeId = storeProduct.Product.BarcodeId,
                Price = storeProduct.Price,
                StoreId = storeProduct.StoreId
            };

            return Ok(response);
        }

    }
}
