using SelfCheckoutApp.Constants;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace SelfCheckoutApp.Pages
{
    public partial class SignUpPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public SignUpPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(ApiConstants.BaseUri)
            };
        }

        // Navigate to Terms of Service page when tapped
        private async void OnTermsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TermsOfServicePage());
        }

        // Navigate to Privacy Policy page when tapped
        private async void OnPrivacyTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PrivacyPolicyPage());
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            ErrorMessage.IsVisible = false;

            string name = NameEntry.Text;
            string email = EmailEntry.Text;
            string phone = PhoneEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "All fields are required.";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (!Regex.IsMatch(name, @"^[a-zA-Z\s]+$"))
            {
                ErrorMessage.Text = "Name can only contain letters and spaces.";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage.Text = "Invalid email format. Please enter a valid email (e.g., John@example.com).";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (phone.Length != 10 || !Regex.IsMatch(phone, @"^\d{10}$"))
            {
                ErrorMessage.Text = "Phone number must be exactly 10 digits.";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                ErrorMessage.Text = "Password must be at least 8 characters long, include at least one uppercase letter, one lowercase letter, and one number.";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage.Text = "Passwords do not match.";
                ErrorMessage.IsVisible = true;
                return;
            }
            var signUpRequest = new
            {
                name = name,
                email = email,
                phone = phone,
                password = password
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Users/Register", signUpRequest);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Account created successfully!", "OK");
                    await Navigation.PopAsync(); 
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage.Text = $"Sign up failed: {errorContent}";
                    ErrorMessage.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error: {ex.Message}";
                ErrorMessage.IsVisible = true;
            }
        }
    }
}
