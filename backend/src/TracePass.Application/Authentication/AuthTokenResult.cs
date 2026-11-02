namespace TracePass.Application.Authentication;

public class AuthTokenResult
{
    public LoginResponse Response { get; set; } = new();

    public string RefreshToken { get; set; } = string.Empty;

    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
}