using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoginWithKeycloak
{
    public class KeycloakClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public KeycloakClient(string keycloakBaseUrl, string clientId, string clientSecret)
        {
            _httpClient = new HttpClient();
            _tokenEndpoint = $"{keycloakBaseUrl}/realms/AnishTestRealm/protocol/openid-connect/token";
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<(string AccessToken, string IdToken)> GetTokensAsync(string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "code", code },
                { "redirect_uri", redirectUri }
            });

            var response = await _httpClient.PostAsync(_tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token request failed: {response.ReasonPhrase}");
            }

            var c = await response.Content.ReadAsStringAsync();
            var tokenResponse =  JsonSerializer.Deserialize<TokenResponse>(c);
            return (tokenResponse.AccessToken, tokenResponse.IdToken);
            //
            //     return (tokenResponse.AccessToken, tokenResponse.IdToken);
            // using (var stream = await response.Content.ReadAsStreamAsync())
            // {
            //     // Deserialize the response content
            //     var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(stream);
            //
            //     return (tokenResponse.AccessToken, tokenResponse.IdToken);
            // }
        }
        
    }
}