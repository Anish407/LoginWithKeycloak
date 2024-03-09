using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;
using IdentityModel.Client;

namespace LoginWithKeycloak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
  public partial class MainWindow : Window
    {
        private const string KeycloakAuthority = "http://localhost:8080/realms/AnishTestRealm";
        private const string ClientId = "WPFAPP";
        private const string RedirectUri ="http://localhost:8081/callback"; // Should match the redirect URI configured in Keycloak
        private const string Scope = "openid profile email";
        private  string _codeVerifier = "";
   

        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            _httpClient = new HttpClient();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Navigate to Keycloak login page when window is loaded
            NavigateToKeycloakLoginPage();
        }

        private void NavigateToKeycloakLoginPage()
        {
            // Generate a code verifier and code challenge
            _codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(_codeVerifier);

            // Store the code verifier for later use in token exchange
            // You may want to store it securely or pass it through navigation context

            // Construct the Keycloak login URL with appropriate parameters
            string authorizeUrl = $"{KeycloakAuthority}/protocol/openid-connect/auth" +
                                  $"?client_id={ClientId}" +
                                  $"&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}" +
                                  $"&response_type=code" +
                                  $"&scope={Scope}" +
                                  $"&code_challenge={codeChallenge}" +
                                  $"&code_challenge_method=S256"; // Using PKCE with SHA-256 code challenge method

            // Navigate the WebBrowser control to the Keycloak login page
            webBrowser.Navigate(authorizeUrl);
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

        private async void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check if the URL being navigated to matches the redirect URI
            if (e.Uri.AbsoluteUri.StartsWith(RedirectUri))
            {
                // Extract authentication response parameters from the query string
                var queryParameters = HttpUtility.ParseQueryString(e.Uri.Query);
                string code = queryParameters["code"];
                string state = queryParameters["state"];

                // Process the authentication response (e.g., exchange code for tokens)
                await ProcessAuthenticationResponse(code, state, _codeVerifier);

                // Cancel the navigation to prevent the WebBrowser from loading the redirect URI
                e.Cancel = true;
            }
        }

        private async Task ProcessAuthenticationResponse(string code, string state, string codeVerifier)
        {
            // Exchange authorization code for tokens using HttpClient
            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("code_verifier", codeVerifier), // Include the code verifier
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("Content-Type", "application/json")
            });

            var response = await _httpClient.PostAsync($"{KeycloakAuthority}/protocol/openid-connect/token", tokenRequestContent);

            if (response.IsSuccessStatusCode)
            {
                // Parse and process token response
                var tokenResponseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);

                string accessToken = tokenResponse.Access_Token;
                string idToken = tokenResponse.IdToken;

                MessageBox.Show($"Access Token: {accessToken}\nID Token: {idToken}");
            }
            else
            {
                MessageBox.Show($"Error occurred while requesting tokens: {response.ReasonPhrase}");
            }
        }
    }
}

   
