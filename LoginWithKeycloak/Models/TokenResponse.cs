using System.Text.Json.Serialization;

namespace LoginWithKeycloak
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string Access_Token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
        public int not_before_policy { get; set; }
        public string session_state { get; set; }
        public string scope { get; set; }
    }
}