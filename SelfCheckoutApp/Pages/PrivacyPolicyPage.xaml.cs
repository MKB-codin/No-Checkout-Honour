using Microsoft.Maui.ApplicationModel;

namespace SelfCheckoutApp.Pages
{
    public partial class PrivacyPolicyPage : ContentPage
    {
        public PrivacyPolicyPage()
        {
            InitializeComponent();
        }

        private async void OnStripeLinkTapped(object sender, EventArgs e)
        {
            var url = "https://stripe.com/privacy";
            await Launcher.OpenAsync(url);
        }
    }
}
