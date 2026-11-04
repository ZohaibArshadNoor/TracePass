using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TracePass.Application.Common.Interfaces;
using TracePass.Infrastructure.Data;

namespace TracePass.Infrastructure.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _dbContext;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?
            .User.Identity?.IsAuthenticated ?? false;

    public bool IsAdministrator =>
    _httpContextAccessor.HttpContext?
        .User.IsInRole("Administrator") ?? false;

    public async Task<Guid?> GetOrganizationIdAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.OrganizationId)
            .SingleOrDefaultAsync(cancellationToken);
    }
}