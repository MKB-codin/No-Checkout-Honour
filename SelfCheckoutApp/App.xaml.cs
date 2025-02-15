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

            // Set the initial MainPage (assuming the user is logged in, for example MainPage)
            // You might initially set it to LoginPage if the user is not logged in.
            MainPage = new AppShell(); // or a NavigationPage(new MainPage(userSession))

            // Subscribe globally to the ServerOffline event.
            serverStatusService.ServerOffline += async (sender, args) =>
            {
                // Ensure we run on the UI thread.
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Only log out if the current page is not LoginPage.
                    if (!(Application.Current.MainPage is NavigationPage nav && nav.CurrentPage is LoginPage))
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Server Offline",
                            "The server is currently unreachable. You will be logged out.",
                            "OK"
                        );

                        // Optionally clear the user session.
                        userSession.Clear();

                        // Set the MainPage to a new LoginPage, so the user is logged out.
                        Application.Current.MainPage = new NavigationPage(new LoginPage(userSession));
                    }
                });
            };

            // Start checking the server status.
            serverStatusService.StartChecking();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage);
        }
    }
}
