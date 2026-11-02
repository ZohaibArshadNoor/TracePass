using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TracePass.Application.Users;
using TracePass.Infrastructure.Data;
using TracePass.Infrastructure.Identity;

namespace TracePass.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(x => x.Organization)
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .ToListAsync();

        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            responses.Add(await MapToResponseAsync(user));
        }

        return responses;
    }

    public async Task<UserResponse?> GetByIdAsync(string id)
    {
        var user = await _dbContext.Users
            .Include(x => x.Organization)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return user is null
            ? null
            : await MapToResponseAsync(user);
    }

    public async Task<(UserResponse? User, string? Error)> CreateAsync(
        CreateUserRequest request)
    {
        var email = request.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return (null, "An account with this email already exists.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return (null, "The selected role does not exist.");
        }

        if (request.OrganizationId.HasValue)
        {
            var organizationExists = await _dbContext.Organizations
                .AnyAsync(x =>
                    x.Id == request.OrganizationId.Value &&
                    x.IsActive);

            if (!organizationExists)
            {
                return (null, "The organization does not exist or is inactive.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            OrganizationId = request.OrganizationId,
            IsActive = true,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(x => x.Description));

            return (null, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            request.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(x => x.Description));

            return (null, errors);
        }

        return (await GetByIdAsync(user.Id), null);
    }

    public async Task<bool> SetActiveStatusAsync(
        string id,
        bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return false;
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description)));
        }

        if (!isActive)
        {
            // Revoke all existing refresh tokens for this user.
            var activeTokens = await _dbContext.RefreshTokens
                .Where(x => x.UserId == id && x.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    private async Task<UserResponse> MapToResponseAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            Roles = roles,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
