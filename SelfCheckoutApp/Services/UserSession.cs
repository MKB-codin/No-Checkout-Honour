using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SelfCheckoutApp.Services
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public int StoreId { get; set; }
        public string StoreName { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public void Clear()
        {
            UserId = 0;
            UserName = string.Empty;
            StoreId = 0;
            StoreName = string.Empty;
            CartItems = new List<CartItem>();
        }

        public class CartItem
        {
            public string ItemName { get; set; }
            public double ItemPrice { get; set; }
            public int ItemQuantity { get; set; } = 1;
        }
    }
}
