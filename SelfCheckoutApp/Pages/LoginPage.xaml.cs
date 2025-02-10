using SelfCheckoutApp.Services;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace SelfCheckoutApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;

        public LoginPage(UserSession userSession)
        {
            InitializeComponent();


            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            })
            {
                BaseAddress = new Uri("https://192.168.0.41:7249")
            };

            _userSession = userSession;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
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
                        /*Preferences.Set("UserId", responseData.UserId);
                        Preferences.Set("UserName", responseData.Name);*/

                        _userSession.UserId = responseData.UserId;
                        _userSession.UserName = responseData.Name;

                        await DisplayAlert("Success", "Login successful!", "OK");

                        // Navigate to the next page
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

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private bool IsValidEmail(string email)
        {
            string emialPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emialPattern);
        }

        public class LoginResponse
        {
            public string Message { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; }
        }
    }

}
