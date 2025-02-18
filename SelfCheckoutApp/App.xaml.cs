using SelfCheckoutApp.Services;
using SelfCheckoutApp.Pages;
using Microsoft.Maui.Dispatching;

namespace SelfCheckoutApp
{
    public partial class App : Application
    {
        public App(UserSession userSession, ServerStatusService serverStatusService)
        {
            InitializeComponent();

            // Set the initial MainPage. (We’re using AppShell here.)
            // The new recommended way to get the current page is via Windows[0].Page.
            Application.Current.Windows[0].Page = new AppShell();

            // Subscribe to the ServerOffline event.
            serverStatusService.ServerOffline += async (sender, args) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Only log out if the current page is not LoginPage.
                    if (!(Application.Current.Windows[0].Page is NavigationPage nav && nav.CurrentPage is LoginPage))
                    {
                        await Application.Current.Windows[0].Page.DisplayAlert(
                            "Server Offline",
                            "The server is currently unreachable. You will be logged out.",
                            "OK"
                        );

                        userSession.Clear();

                        // Reset the current window's page to a new LoginPage.
                        Application.Current.Windows[0].Page = new NavigationPage(new LoginPage(userSession));
                    }
                });
            };

            // Start checking the server status.
            serverStatusService.StartChecking();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Return the current window that we set in the constructor.
            return new Window(Application.Current.Windows[0].Page);
        }
    }
}