## Authenticating users using KeyCloak as an IDP in a WPF client

#### Basic Flow
![image](https://github.com/Anish407/LoginWithKeycloak/assets/51234038/5300867e-82a0-4b1f-8242-1ea4e9b441fb)

##### Steps
<ul>
  <li>1. User starts the App</li>
  <li>2. App create a code verifier and a hash of it called the Code Challenge</li>
  <li>3. The Auth Endpoint is called and the code challenge is passed to the IDP. The IDP stores the Code Challenge</li>
  <li>4. The user is redirect to the login page at the IDP (keyCloack)</li>
  <li>5. The user enter the credentials at the IDP and logs in</li>
   <li>6. The IDP returns an authorization code</li>
   <li>7. Then from the WPF app, we call the token endpoint and passing it the code verifier (Note: Not the code challenge). The token request includes the necessary parameters such as client ID, redirect URI, authorization code, and grant type (authorization_code). </li>
   <li>8. The IDP hashes the code verifier and compares it with the code challenge which was stored at step 3, if they match then the IDP returns the Access and IDToken (based on the scopes passed)  </li>
</ul>

#### Explanation
- The MainWindow class represents the main window of the WPF application.
- The NavigateToKeycloakLoginPage method constructs the Keycloak login URL with the appropriate parameters, including the code challenge.
- The GenerateCodeVerifier and GenerateCodeChallenge methods generate the code verifier and code challenge, respectively, required for PKCE.
- The Base64UrlEncode method encodes bytes to a Base64 URL-safe string, which is used in PKCE.


#### References
<ul>
  <li>Checkout the different branches that contain different implementations</li>
  <li>https://frameworks.readthedocs.io/en/latest/spring-boot/spring-boot2/keycloakOAuth2PKCE.html#creating-a-client</li>
</ul>
