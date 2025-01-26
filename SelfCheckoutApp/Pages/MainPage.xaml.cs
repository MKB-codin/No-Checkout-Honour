using System.Windows.Input;

namespace SelfCheckoutApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public ICommand LogoutCommand { get; }

        public MainPage()
        {
            InitializeComponent();

            // Bind the logout command to the back button behavior
            LogoutCommand = new Command(async () =>
            {
                bool confirmLogout = await DisplayAlert("Logout", "Are you sure you want to log out?", "Logout", "No");
                if (confirmLogout)
                {
                    await Navigation.PopToRootAsync(); // Navigate back to LoginPage
                }
            });

            BindingContext = this;
        }

        private async void OnStartShoppingClicked(object sender, EventArgs e)
        {
            // Navigate to the Start Shopping page (to be implemented)
            //await Navigation.PushAsync(new ShoppingPage()); 
        }

        private async void OnReceiptsClicked(object sender, EventArgs e)
        {
            // Navigate to the Receipts page (to be implemented)
            //await Navigation.PushAsync(new ReceiptsPage()); 
        }
    }
}
