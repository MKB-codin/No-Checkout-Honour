using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using HonourSelfCheckoutServer.Helpers;

namespace SelfCheckoutServer.Tests
{
    public class HelperTests
    {
        [Fact]
        public void EncryptionHelperTest()
        {
            string originalTxt = "ThisNeedsToBeEncryted";
            string encryptedTxt = EncryptionHelper.Encrypt(originalTxt);
            string decryptedTxt = EncryptionHelper.Decrypt(encryptedTxt);
            Assert.Equal(originalTxt, decryptedTxt);
        }

        [Fact]
        public void HashHelperTest()
        {
            string originalTxt = "ThisNeedsToBeHashed";
            string expectedHash = "BD3BE566C6CEE1753A4FCE36E26A641FAC41EAE76F1CD36D28E69E2B4ABED3E5";
            string hashedTxt = HashHelper.Hash(originalTxt);
            Assert.Equal(expectedHash, hashedTxt);
        }
    }
}
