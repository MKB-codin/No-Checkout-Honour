using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;


namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public UsersController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _databaseContext.Users.AnyAsync(u => u.Email == user.Email)) 
                { return BadRequest(new { Message = "This email is already in use" }); }

            user.Password = HashPassword(user.Password);

            _databaseContext.Users.Add(user);
            await _databaseContext.SaveChangesAsync();

            return Ok(new { Message = "Registration complete" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dbUser = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            var inputHash = HashPassword(loginRequest.Password);
            /* Log to compare hashes
            if (dbUser == null){
                return Unauthorized(new { Message = "No user exists" });
            }
            Console.WriteLine($"Input Hash: {inputHash}");
            Console.WriteLine($"Stored Hash: {dbUser.Password}");
            Console.WriteLine($"Input Email: {loginRequest.Email}");
            Console.WriteLine($"Stored Email:  {dbUser.Email}");*/


            if (dbUser == null || dbUser.Password.ToLower() != HashPassword(loginRequest.Password).ToLower())
                return Unauthorized(new { Message = "Invalid email or password" });

            return Ok(new { Message = "Login successful", UserId = dbUser.UserId });
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
    }
}
