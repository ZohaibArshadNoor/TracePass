using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TracePass.Application.Authentication;
using TracePass.Infrastructure.Configuration;
using TracePass.Infrastructure.Data;
using TracePass.Infrastructure.Identity;

namespace TracePass.Infrastructure.Services;

public class TracePassAuthenticationService
    : ITracePassAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public TracePassAuthenticationService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthTokenResult?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordIsValid)
        {
            return null;
        }

        return await CreateAuthTokenResultAsync(user);
    }

    public async Task<AuthTokenResult?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        var now = DateTimeOffset.UtcNow;

        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= now ||
            !storedToken.User.IsActive)
        {
            return null;
        }

        // Invalidate the old token before creating its replacement.
        storedToken.RevokedAt = now;

        var replacement = await CreateAuthTokenResultAsync(
            storedToken.User,
            saveChanges: false);

        storedToken.ReplacedByTokenHash =
            HashToken(replacement.RefreshToken);

        try
        {
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return replacement;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request may already have rotated this token.
            return null;
        }
    }

    public async Task RevokeRefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = HashToken(refreshToken);

        var token = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash);

        if (token is null || token.RevokedAt is not null)
        {
            return;
        }

        token.RevokedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        ClaimsPrincipal principal)
    {
        var userId = _userManager.GetUserId(principal);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationId = user.OrganizationId,
            Roles = roles
        };
    }

    private async Task<AuthTokenResult> CreateAuthTokenResultAsync(
        ApplicationUser user,
        bool saveChanges = true)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var now = DateTime.UtcNow;

        var expiresAt = now.AddMinutes(
            _jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email ?? string.Empty),
            new("first_name", user.FirstName),
            new("last_name", user.LastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(jwt);

        var rawRefreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(
            _jwtSettings.RefreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = refreshExpiresAt
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        if (saveChanges)
        {
            await _dbContext.SaveChangesAsync();
        }

        return new AuthTokenResult
        {
            Response = new LoginResponse
            {
                AccessToken = accessToken,
                ExpiresAt = new DateTimeOffset(expiresAt),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            },
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}