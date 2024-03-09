using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Navigation;

namespace LoginWithKeycloak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string KeycloakAuthority = "http://localhost:8080/realms/AnishTestRealm";
        private const string ClientId = "WPFAPP";

        private const string
            RedirectUri = "http://localhost:8081/callback"; // Should match the redirect URI configured in Keycloak

        private const string Scope = "openid profile email";
        private string _codeVerifier = "";

        private readonly KeycloakClient _keycloakClient;
        private WellKnownConfiguration _wellKnownConfiguration = new WellKnownConfiguration();


        private static readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _keycloakClient = new KeycloakClient(ClientId);
        }

        private  void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Navigate to Keycloak login page when window is loaded
            NavigateToKeycloakLoginPage();
        }

        private async void NavigateToKeycloakLoginPage()
        {
            _wellKnownConfiguration = await _keycloakClient.GetWellKnownEndpointInfo(KeycloakAuthority);
           
            // Generate a code verifier and code challenge
            _codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(_codeVerifier);

            // Construct the Keycloak login URL with appropriate parameters
            string authorizeUrl = $"{_wellKnownConfiguration.authorization_endpoint}" +
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
            byte[] bytes = Encoding.UTF8.GetBytes(codeVerifier);
            using (var sha256 = SHA256.Create())
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

                // Call the token endpoint to get the access + IdToken
                // We pass the code verifier and not the code challenge
                (string accessToken, string idToken)  = await _keycloakClient.GetAccessToken(_wellKnownConfiguration, new TokenRequestDto()
                {
                    RedirectUri = RedirectUri,
                    CodeVerifier = _codeVerifier,
                    AuthCode = code
                });
                MessageBox.Show($"AccessToken:{accessToken}, IdToken:{idToken}");
               
                // Cancel the navigation to prevent the WebBrowser from loading the redirect URI
                e.Cancel = true;
            }
        }
    }
}