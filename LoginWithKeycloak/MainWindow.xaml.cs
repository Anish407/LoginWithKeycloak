using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using IdentityModel.Client;

namespace LoginWithKeycloak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   public partial class MainWindow : Window
    {
        private const string KeycloakBaseUrl = "http://localhost:8080";
        private const string ClientId = "WPFAPP";
        private const string RedirectUri = "http://localhost:8081/callback";
        private readonly KeycloakClient _keycloakClient;
        private readonly string _realmName = "AnishTestRealm";
        private string _codeVerifier;
        private WellKnownConfiguration _configurationDetails;

        public MainWindow()
        {
            InitializeComponent();
            _keycloakClient = new KeycloakClient(KeycloakBaseUrl, ClientId);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _configurationDetails = await CallWellKnowEndpoint();
            _codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(_codeVerifier);
            string loginUrl = $"{_configurationDetails.authorization_endpoint}?response_type=code&client_id={ClientId}&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}&scope=openid&code_challenge={HttpUtility.UrlEncode(codeChallenge)}&code_challenge_method=S256";
            Browser.Navigate(new Uri(loginUrl));
        }

        private  async ValueTask<WellKnownConfiguration> CallWellKnowEndpoint()
        {
            // Dont call the configuration endpoint always
            if (_configurationDetails != null)
            {
                return _configurationDetails;
            }
            string wellKnownEndpoint = $"{KeycloakBaseUrl}/realms/{_realmName}/.well-known/openid-configuration";

            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(wellKnownEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<WellKnownConfiguration>(content);
                }
                else
                {
                    throw new Exception($"Cannot read Configuration details from Keycloak: URL:{wellKnownEndpoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        private async void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;

            if (url.StartsWith(RedirectUri))
            {
                var query = HttpUtility.ParseQueryString(e.Uri.Query);
                string code = query.Get("code");

                if (!string.IsNullOrEmpty(code))
                {
                    var (accessToken, idToken) = await _keycloakClient.GetTokensAsync(code, _codeVerifier, RedirectUri, _configurationDetails);
                    MessageBox.Show($"Access Token: {accessToken}\nID Token: {idToken}");
                }
            }
        }

        private string GenerateCodeVerifier()
        {
            // Generate a random code verifier (43-128 characters)
            Random random = new Random();
            byte[] buffer = new byte[32];
            random.NextBytes(buffer);
            return Base64UrlEncode(buffer);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            // Generate SHA-256 hash of the code verifier
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(codeVerifier);
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Base64UrlEncode(hashBytes);
            }
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}


   
