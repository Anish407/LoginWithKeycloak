namespace LoginWithKeycloak
{
    public struct WellKnownConfiguration
    {
        public string issuer { get; set; }
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string introspection_endpoint { get; set; }
        public string userinfo_endpoint { get; set; }
        public string end_session_endpoint { get; set; }

        public override string ToString()
        {
            return $"Issuer:{issuer},AuthEndpoint: {authorization_endpoint}, TokenEndpoint:{token_endpoint} ";
        }
    }
}