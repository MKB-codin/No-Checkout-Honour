using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();

        public void Clear()
        {
            UserId = 0;
            UserName = string.Empty;
            StoreId = 0;
            StoreName = string.Empty;
        }

        public class CartItem : INotifyPropertyChanged
        {
            private int productId;
            private string itemName;
            private double itemPrice;
            private int itemQuantity = 1;

            public int ProductId
            {
                get => productId;
                set
                {
                    if (productId != value)
                    {
                        productId = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string ItemName
            {
                get => itemName;
                set
                {
                    if (itemName != value)
                    {
                        itemName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public double ItemPrice
            {
                get => itemPrice;
                set
                {
                    if (itemPrice != value)
                    {
                        itemPrice = value;
                        OnPropertyChanged();
                    }
                }
            }

            public int ItemQuantity
            {
                get => itemQuantity;
                set
                {
                    if (itemQuantity != value)
                    {
                        itemQuantity = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
