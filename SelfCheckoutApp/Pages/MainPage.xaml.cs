using SelfCheckoutApp.Services;
using System.Windows.Input;

namespace SelfCheckoutApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public ICommand LogoutCommand { get; }
        private readonly UserSession _userSession;
        public MainPage(UserSession userSession)
        {
            InitializeComponent();

            // Bind the logout command to the back button behavior
            LogoutCommand = new Command(async () =>
            {
                bool confirmLogout = await DisplayAlert("Logout", "Are you sure you want to log out?", "Logout", "No");
                if (confirmLogout)
                {
                    _userSession.Clear();
                    await Navigation.PopToRootAsync();
                }
            });

            _userSession = userSession;

            WelcomeLabel.Text = $"Welcome, {_userSession.UserName}";
            BindingContext = this;
        }

        private async void OnStartShoppingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StoreSelectionPage(_userSession)); 
        }

        private async void OnReceiptsClicked(object sender, EventArgs e)
        {
            // Navigate to the Receipts page (to be implemented)
            //await Navigation.PushAsync(new ReceiptsPage()); 
        }
    }
}
