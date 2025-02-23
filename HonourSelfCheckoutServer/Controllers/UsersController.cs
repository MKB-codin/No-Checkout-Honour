using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using HonourSelfCheckoutServer.Helpers;


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
            string emailHash = HashHelper.Hash(user.Email.ToLower());

            if (await _databaseContext.Users.AnyAsync(u => u.HashedEmail == emailHash))
                return BadRequest(new { Message = "This email is already in use" });

            // encrypt
            user.Name = EncryptionHelper.Encrypt(user.Name);
            user.Email = EncryptionHelper.Encrypt(user.Email);
            user.Phone = EncryptionHelper.Encrypt(user.Phone);

            // hash
            user.HashedEmail = emailHash;
            user.Password = HashHelper.Hash(user.Password);

            _databaseContext.Users.Add(user);
            await _databaseContext.SaveChangesAsync();

            return Ok(new { Message = "Registration complete" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            string hashedEmail = HashHelper.Hash(loginRequest.Email.ToLower());

            var dbUser = await _databaseContext.Users.FirstOrDefaultAsync(u => u.HashedEmail == hashedEmail);


            if (dbUser == null || dbUser.Password.ToLower() != HashHelper.Hash(loginRequest.Password).ToLower())
                return Unauthorized(new { Message = "Invalid email or password" });


            return Ok(new
            {
                Message = "Login successful",
                UserId = dbUser.UserId,
                Name = EncryptionHelper.Decrypt(dbUser.Name)
            });
        }
    }
}
