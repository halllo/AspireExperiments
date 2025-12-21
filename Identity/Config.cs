using Duende.IdentityServer.Models;

namespace Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId()
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [];

    public static IEnumerable<Client> Clients =>
        [];
}
