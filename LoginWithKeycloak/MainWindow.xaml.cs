using System;
using System.Linq;
using System.Windows;

namespace LoginWithKeycloak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   public partial class MainWindow : Window
    {
        private const string KeycloakUrl = "http://localhost:8080/realms/AnishTestRealm";
        private const string ClientId = "WPFAPP";
        private const string RedirectUri = "http://localhost:8081/callback";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AuthBrowser.Navigated += (s, args) =>
            {
                // Check if the URL matches the redirect URI
                if (args.Uri.AbsoluteUri.StartsWith(RedirectUri))
                {
                    // Extract query parameters manually
                    var uri = args.Uri;
                    var query = uri.Query.TrimStart('?');
                    var queryStringParams = query.Split('&')
                        .Select(p => p.Split('='))
                        .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1]));

                    // Get the authorization code from the query parameters
                    var code = queryStringParams["code"];

                    // Complete the authentication flow using the authorization code
                    // For example, send the code to your server to exchange for tokens
                    // Handle the code as per your authentication flow requirements
                    MessageBox.Show("Authentication successful! Authorization code: " + code);
                }
            };

            // Load the Keycloak login page in the WebBrowser control
            AuthBrowser.Navigate(new Uri($"{KeycloakUrl}/protocol/openid-connect/auth?response_type=code&client_id={ClientId}&redirect_uri={RedirectUri}&scope=openid%20profile%20email"));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Reload the Keycloak login page in the WebBrowser control
            try
            {
                AuthBrowser.Navigate(new Uri($"{KeycloakUrl}/protocol/openid-connect/auth?response_type=code&client_id={ClientId}&redirect_uri={RedirectUri}&scope=openid%20profile%20email"));

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}

   
