using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;

namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public StoresController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("GetAllStores")]
        public async Task<IActionResult> GetAllStores()
        {
            try
            {
                var stores = await _databaseContext.Stores.ToListAsync();
                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving stores.", Error = ex.Message });
            }
        }
    }
}
