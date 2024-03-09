namespace LoginWithKeycloak
{
    public struct TokenRequestDto
    {
        public string RedirectUri { get; set; }
        public string AuthCode { get; set; }
        public string CodeVerifier { get; set; }
      
    }
}