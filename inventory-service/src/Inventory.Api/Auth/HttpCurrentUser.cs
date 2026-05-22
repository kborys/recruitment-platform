using Inventory.Application.Abstractions;

namespace Inventory.Api.Auth;

public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserName =>
        httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("Current user is not available (no 'sub' claim).");
}
