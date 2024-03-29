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

#### KeyCloak Configuration
- I have created a separate realm(similar to tenants in azure ad) in keycloak
##### Create a client
![image](https://github.com/Anish407/LoginWithKeycloak/assets/51234038/67f76725-9ce8-4f7a-b115-777671b772a5)



  ![image](https://github.com/Anish407/LoginWithKeycloak/assets/51234038/91015ecc-f9d7-402a-9b43-940b1baa3d8f)
  ![image](https://github.com/Anish407/LoginWithKeycloak/assets/51234038/244c2e37-c12a-41d5-86e9-c53c7185d33a)
  ![image](https://github.com/Anish407/LoginWithKeycloak/assets/51234038/628c2306-b225-4c40-aa2a-067e73de9494)

<p>
 Client authentication-> This defines the type of the OIDC client. When it's ON, the OIDC type is set to confidential access type. When it's OFF, it is set to public access type
</p>

## INTEGRATE KEYCLOAK WITH AZURE AD (EXISTING USERS IN AZURE AD CAN LOGIN TO THE APP USING THEIR CREDENTIALS

#### Create a keycloak client
<img width="806" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/258d48f7-a645-4d1b-b7c8-06b39f24c082">
<img width="757" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/dd087405-9ce0-4430-9356-427c17240ade">

## Adding Azure Ad as an identityProvider in Keycloak



#### Keycloak Settings
<img width="815" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/6bd329fb-f440-40e5-9914-a4602b3f9032">

From the list of Identity providers select Keycloak OpenId Connect
<img width="739" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/4d1da610-1312-4802-a8f4-060039894d0e">

Then give it a name (AzureAd) in my case.
<img width="821" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/cbd9efc6-4a3d-482f-85d2-de2fc9b0c253">

Copy the redirect uri from keycloak 

#### Azure Ad (create a client)
Entra -> App registrations -> new Registrations
<img width="416" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/80e3b898-1993-46fe-b7db-0c02158cfdc6">

paste the redirect uri from keycloak as the redirect uri in the azure ad app registration
<img width="808" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/be025f89-c33f-4ce0-8f76-cf168b6fb256">

Create a client secret for the app registration in azure ad and navigate back to keycloak. Now we will fill in rest of the IDP settings

Paste the OpenID Connect metadata document from azure ad as the discovery document in keycloak
<img width="877" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/fdc76c6b-c264-483e-a078-bc79144845b5">

<img width="981" alt="image" src="https://github.com/Anish407/LoginWithKeycloak/assets/51234038/c0ec2b5d-1da9-4d4f-b0f0-bbe63893d290">

we set the PKCE method to 256 since i will generate the code challenge as a Sha256 hash of the code verifier

#### References
<ul>
  <li>Checkout the different branches that contain different implementations</li>
  <li>https://frameworks.readthedocs.io/en/latest/spring-boot/spring-boot2/keycloakOAuth2PKCE.html#creating-a-client</li>
</ul>
