using SelfCheckoutApp.Services;
using System.Collections.ObjectModel;

namespace SelfCheckoutApp.Pages
{
    public partial class BasketPage : ContentPage
    {
        public ObservableCollection<Services.UserSession.CartItem> CartItems { get; set; }
        private double _totalPrice = 0;
        private readonly UserSession _userSession;
        public BasketPage(UserSession userSession)
        {
            InitializeComponent();
            BindingContext = this;
            _userSession = userSession;

            CartItems = new ObservableCollection<Services.UserSession.CartItem>(_userSession.CartItems);
            UpdateTotalPrice();
        }

        private void OnIncreaseQuantity(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button.BindingContext as Services.UserSession.CartItem;
            if (item != null)
            {
                item.ItemQuantity++;
                UpdateTotalPrice();
            }
        }

        private void OnDecreaseQuantity(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button.BindingContext as Services.UserSession.CartItem;
            if (item != null && item.ItemQuantity > 1)
            {
                item.ItemQuantity--;
                UpdateTotalPrice();
            }
            else if (item.ItemQuantity == 1)
            {
                CartItems.Remove(item); // Remove item if quantity reaches zero
            }
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            _totalPrice = CartItems.Sum(item => item.ItemPrice * item.ItemQuantity);
            TotalPriceLabel.Text = $"Total: £{_totalPrice:F2}";
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new ScanItemPage(this)); // Navigate to scan page
        }

        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (CartItems.Count == 0)
            {
                await DisplayAlert("Empty Cart", "Your basket is empty.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Checkout", "Proceed to checkout?", "Yes", "No");
            if (confirm)
            {
                // Send cart to server (to be implemented)
                await DisplayAlert("Success", "Checkout complete!", "OK");
                CartItems.Clear();
                UpdateTotalPrice();
            }
        }

        public void AddItemToCart(Services.UserSession.CartItem item)
        {
            var existingItem = CartItems.FirstOrDefault(x => x.ItemName == item.ItemName);
            if (existingItem != null)
            {
                existingItem.ItemQuantity++;
            }
            else
            {
                CartItems.Add(item);
            }
            UpdateTotalPrice();
        }
    }
}
