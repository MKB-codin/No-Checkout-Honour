using SelfCheckoutApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace SelfCheckoutApp.Pages
{
    public partial class BasketPage : ContentPage
    {
        private readonly UserSession _userSession;
        public ObservableCollection<Services.UserSession.CartItem> CartItems => _userSession.CartItems;

        public BasketPage(UserSession userSession)
        {
            InitializeComponent();
            _userSession = userSession;
            BindingContext = this;
            UpdateTotalPrice();
        }

        private void OnIncreaseQuantity(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Services.UserSession.CartItem item)
            {
                item.ItemQuantity++;
                UpdateTotalPrice();
            }
        }

        private async void OnDecreaseQuantity(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Services.UserSession.CartItem item)
            {
                if (item.ItemQuantity > 1)
                {
                    item.ItemQuantity--;
                }
                else
                {
                    // Prompt the user to confirm removal when quantity is 1.
                    bool confirm = await DisplayAlert("Confirm Removal",
                        $"Do you really want to remove  {item.ItemName}  from your basket?", "Yes", "No");
                    if (confirm)
                    {
                        CartItems.Remove(item);
                        _userSession.CartItems.Remove(item); // Update the session cart as well.
                    }
                }
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            double total = CartItems.Sum(item => item.ItemPrice * item.ItemQuantity);
            TotalPriceLabel.Text = $"Total: £{total:F2}";
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            // Navigate to the Scan Item page, passing the UserSession
            await Navigation.PushAsync(new ScanItemPage(_userSession));
        }

        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (!CartItems.Any())
            {
                await DisplayAlert("Empty Cart", "Your basket is empty.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Checkout", "Proceed to checkout?", "Yes", "No");
            if (confirm)
            {
                // Send the cart to the server (to be implemented)
                await DisplayAlert("Success", "Checkout complete!", "OK");

                _userSession.CartItems.Clear();
                UpdateTotalPrice();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateTotalPrice();
        }

    }
}
