using SelfCheckoutApp.Services;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Maui.Dispatching;
using System.Threading.Tasks;
using SelfCheckoutApp.Constants;

namespace SelfCheckoutApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;
        private bool _serverOnline = true; // assume online initially
        private bool _isLoginInProgress = false;   // throttle flag for login
        private bool _isSignUpInProgress = false;    // throttle flag for sign-up

        public LoginPage(UserSession userSession)
        {
            InitializeComponent();
            _userSession = userSession;
            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(ApiConstants.BaseUri),
                Timeout = TimeSpan.FromSeconds(2)
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _serverOnline = await IsServerOnline();
            UpdateUIBasedOnServerStatus(_serverOnline);
        }

        private async Task<bool> IsServerOnline()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                Console.WriteLine($"Health check returned: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Health check exception: {ex.Message}");
                return false;
            }
        }

        private void UpdateUIBasedOnServerStatus(bool online)
        {
            // Show the server status message if the server is offline.
            ServerStatusMessage.IsVisible = !online;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Throttle login requests: if one is already in progress, do nothing.
            if (_isLoginInProgress)
                return;

            _isLoginInProgress = true;
            try
            {
                // Re-check server status before attempting login.
                _serverOnline = await IsServerOnline();
                UpdateUIBasedOnServerStatus(_serverOnline);

                if (!_serverOnline)
                {
                    await DisplayAlert("Server Offline", "The server is currently unreachable. Please try again later.", "OK");
                    return;
                }

                ErrorMessage.IsVisible = false;
                string email = EmailEntry.Text;
                string password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage.Text = "Email and Password are required.";
                    ErrorMessage.IsVisible = true;
                    return;
                }

                if (!IsValidEmail(email))
                {
                    ErrorMessage.Text = "Please enter a valid email address.";
                    ErrorMessage.IsVisible = true;
                    return;
                }

                var loginRequest = new { email, password };

                try
                {
                    var response = await _httpClient.PostAsJsonAsync("/api/Users/Login", loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();

                        if (responseData != null)
                        {
                            _userSession.UserId = responseData.UserId;
                            _userSession.UserName = responseData.Name;

                            await DisplayAlert("Success", "Login successful!", "OK");

                            await Navigation.PushAsync(new MainPage(_userSession));
                        }
                    }
                    else
                    {
                        ErrorMessage.Text = "Invalid email or password.";
                        ErrorMessage.IsVisible = true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = $"Error: {ex.Message}";
                    ErrorMessage.IsVisible = true;
                }
            }
            finally
            {
                // Throttle: wait 2 seconds before allowing another login attempt.
                await Task.Delay(2000);
                _isLoginInProgress = false;
            }
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            // Throttle sign-up requests.
            if (_isSignUpInProgress)
                return;

            _isSignUpInProgress = true;
            try
            {
                // Check if the server is online before proceeding.
                bool serverUp = await IsServerOnline();
                UpdateUIBasedOnServerStatus(serverUp);

                if (!serverUp)
                {
                    await DisplayAlert("Server Offline", "The server is currently unreachable. Please try again later.", "OK");
                    return;
                }

                // If server is online, navigate to the Sign Up page.
                await Navigation.PushAsync(new SignUpPage());
            }
            finally
            {
                // Wait 2 seconds to throttle subsequent sign-up clicks.
                await Task.Delay(2000);
                _isSignUpInProgress = false;
            }
        }

        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        public class LoginResponse
        {
            public string Message { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; }
        }
    }
}
