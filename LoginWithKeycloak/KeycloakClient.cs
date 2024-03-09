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

        public KeycloakClient(string keycloakBaseUrl, string clientId)
        {
            _httpClient = new HttpClient();
            _clientId = clientId;
        }

        public async Task<(string AccessToken, string IdToken)> GetTokensAsync(string code, string codeVerifier, string redirectUri,WellKnownConfiguration wellKnownConfiguration )
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _clientId },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            });

            var response = await _httpClient.PostAsync(wellKnownConfiguration.token_endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token request failed: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);

            return (tokenResponse.AccessToken, tokenResponse.IdToken);
        }
    }
}