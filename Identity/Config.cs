using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [

    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "web",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { "https://localhost:8443/backend/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:8443/backend/signout-callback-oidc" },
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile
            }
        }
    ];
}
