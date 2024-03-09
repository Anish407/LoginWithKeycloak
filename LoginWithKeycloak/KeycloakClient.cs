using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LoginWithKeycloak
{
    public interface IKeycloakClient
    {
        Task<WellKnownConfiguration> GetWellKnownEndpointInfo(string keyCloakBaseUrl);

        Task<(string accessToken, string idToken)> GetAccessToken(WellKnownConfiguration openIdConfigurationDto,
            TokenRequestDto tokenRequestDto);
    }

    public class KeycloakClient : IKeycloakClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _tokenEndpoint;
        private readonly string _clientId;

        public KeycloakClient(string clientId)
        {
            _httpClient = new HttpClient();
            _clientId = clientId;
        }

        // public async Task<(string AccessToken, string IdToken)> GetTokensAsync(string code, string codeVerifier, string redirectUri,WellKnownConfiguration wellKnownConfiguration )
        // {
        //     var content = new FormUrlEncodedContent(new Dictionary<string, string>
        //     {
        //         { "grant_type", "authorization_code" },
        //         { "client_id", _clientId },
        //         { "code", code },
        //         { "redirect_uri", redirectUri },
        //         { "code_verifier", codeVerifier }
        //     });
        //
        //     var response = await _httpClient.PostAsync(wellKnownConfiguration.token_endpoint, content);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         throw new Exception($"Token request failed: {response.ReasonPhrase}");
        //     }
        //
        //     var responseContent = await response.Content.ReadAsStringAsync();
        //     var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);
        //
        //     return (tokenResponse.Access_Token, tokenResponse.IdToken);
        // }
        public async Task<WellKnownConfiguration> GetWellKnownEndpointInfo(string keyCloakBaseUrl)
        {
            string discoveryEndpoint = $"{keyCloakBaseUrl}/.well-known/openid-configuration";
            try
            {
                // Make GET request to the well-known endpoint
                HttpResponseMessage response = await _httpClient.GetAsync(discoveryEndpoint);
                response.EnsureSuccessStatusCode();
                // Read as stream (Change later)
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WellKnownConfiguration>(responseBody);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error Calling Discovery Endpoint: {keyCloakBaseUrl}, ErrorMessage: {ex.Message}");
            }
        }

        public async Task<(string accessToken, string idToken)> GetAccessToken(WellKnownConfiguration openIdConfigurationDto,
            TokenRequestDto tokenRequestDto)
        {
            // Exchange authorization code for tokens using HttpClient
            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("redirect_uri", tokenRequestDto.RedirectUri),
                new KeyValuePair<string, string>("code", tokenRequestDto.AuthCode),
                new KeyValuePair<string, string>("code_verifier", tokenRequestDto.CodeVerifier), // Include the code verifier
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("Content-Type", "application/json")
            });

            try
            {
                var response = await _httpClient.PostAsync($"{openIdConfigurationDto.issuer}/protocol/openid-connect/token",
                    tokenRequestContent);
               
                response.EnsureSuccessStatusCode();
               
                // Parse and process token response
                var tokenResponseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);

                return (tokenResponse.Access_Token, tokenResponse.IdToken);

            }
            catch (Exception e)
            {
                throw new Exception($"Error Getting token, Endpoint Details: {openIdConfigurationDto}");
            }
        }
    }
}