using System.Collections.ObjectModel;

namespace SelfCheckoutApp.Pages
{
    public partial class BasketPage : ContentPage
    {
        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();
        private double _totalPrice = 0;

        public BasketPage()
        {
            InitializeComponent();
            BindingContext = this;
            UpdateTotalPrice();
        }

        private void OnIncreaseQuantity(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button.BindingContext as CartItem;
            if (item != null)
            {
                item.Quantity++;
                UpdateTotalPrice();
            }
        }

        private void OnDecreaseQuantity(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button.BindingContext as CartItem;
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                UpdateTotalPrice();
            }
            else if (item.Quantity == 1)
            {
                CartItems.Remove(item); // Remove item if quantity reaches zero
            }
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            _totalPrice = CartItems.Sum(item => item.Price * item.Quantity);
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

        public void AddItemToCart(CartItem item)
        {
            var existingItem = CartItems.FirstOrDefault(x => x.Name == item.Name);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartItems.Add(item);
            }
            UpdateTotalPrice();
        }
    }

    public class CartItem
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
