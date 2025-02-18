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


            MainPage = new AppShell(); 


            serverStatusService.ServerOffline += async (sender, args) =>
            {

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

                        userSession.Clear();

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
