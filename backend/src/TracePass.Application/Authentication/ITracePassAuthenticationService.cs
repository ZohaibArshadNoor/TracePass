using System.Security.Claims;

namespace TracePass.Application.Authentication;

public interface ITracePassAuthenticationService
{
    Task<AuthTokenResult?> LoginAsync(LoginRequest request);

    Task<AuthTokenResult?> RefreshAsync(string refreshToken);

    Task RevokeRefreshTokenAsync(string? refreshToken);

    Task<CurrentUserResponse?> GetCurrentUserAsync(
        ClaimsPrincipal principal);
}