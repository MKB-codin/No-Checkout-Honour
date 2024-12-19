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
        private DatabaseContext _databaseContext;

        public ReceiptsController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetReceiptByUser(int userId)
        {
            var receipts = await _databaseContext.Receipts
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (!receipts.Any())
            {
                return NotFound(new { Message = "No receipts found for this user" });
            }

            return Ok(receipts);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] Receipt receipt)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _databaseContext.Receipts.Add(receipt);
            await _databaseContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReceiptByUser), new { userId = receipt.UserId }, receipt);
        }
    }
}
