using System;
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
        private const string KeycloakUrl = "http://localhost:8080/";
        private const string ClientId = "WPFAPP";
        private const string ClientSecret = "UYggTukD6J9y6pukl9vB39FlaJQq9KEg";
        private const string RedirectUri = "http://localhost:8081/callback";
        private const string RealmName = "AnishTestRealm";

        private readonly KeycloakClient _keycloakClient;

        public MainWindow()
        {
            InitializeComponent();
            _keycloakClient = new KeycloakClient(KeycloakUrl, ClientId, ClientSecret);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Redirect the user to the Keycloak login page
            var loginUrl = $"{KeycloakUrl}/realms/{RealmName}/protocol/openid-connect/auth?response_type=code&client_id={ClientId}&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}&scope=openid%20profile%20email";
            Browser.Navigate(new Uri(loginUrl));
        }

        private async void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Check if the URL contains the authorization code
            if (e.Uri.AbsoluteUri.StartsWith(RedirectUri))
            {
                var uri = new Uri(e.Uri.AbsoluteUri);
                var query = HttpUtility.ParseQueryString(uri.Query);
                var code = query.Get("code");

                if (!string.IsNullOrEmpty(code))
                {
                    // Exchange the authorization code for tokens
                    var (accessToken, idToken) = await _keycloakClient.GetTokensAsync(code, RedirectUri);

                    // Use the access token and ID token as needed
                    MessageBox.Show($"Access Token: {accessToken}\nID Token: {idToken}");
                }
            }
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
    }
}

   
