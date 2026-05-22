using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Product.IntegrationTest.Common;

internal static class AuthenticatedClientExtensions
{
    public static HttpClient CreateAuthenticatedClient<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        string user,
        params string[] roles) where TEntryPoint : class
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestTokenFactory.Create(user, roles));
        return client;
    }
}
