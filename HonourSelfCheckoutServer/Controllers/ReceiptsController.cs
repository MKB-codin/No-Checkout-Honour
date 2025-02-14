using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;


namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public ReceiptsController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserReceipts(int userId)
        {
            var receipts = await _databaseContext.Receipts
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.ReceiptId,
                    r.StoreId,
                    StoreName = _databaseContext.Stores
                        .Where(s => s.StoreId == r.StoreId)
                        .Select(s => s.StoreName)
                        .FirstOrDefault(), // Get the store name
                    r.UserId,
                    r.Total,
                    r.PurchaseDate,
                    ReceiptItems = r.ReceiptItems.Select(ri => new
                    {
                        ri.ItemId,
                        ri.ReceiptId,
                        ri.ProductId,
                        ri.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(receipts);
        }

        // GET: api/Receipts/5
        [HttpGet("{receiptId}")]
        public async Task<IActionResult> GetReceiptDetails(int receiptId)
        {
            var receipt = await _databaseContext.Receipts
                .Include(r => r.ReceiptItems)
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId);
            if (receipt == null)
                return NotFound(new { Message = "Receipt not found." });
            return Ok(receipt);
        }
    }
}

