using System.Text;
using System.Security.Cryptography;

namespace HonourSelfCheckoutServer.Helpers
{
    public class HashHelper
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
    }
}
