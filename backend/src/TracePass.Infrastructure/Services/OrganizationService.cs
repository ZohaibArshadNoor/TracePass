using Microsoft.EntityFrameworkCore;
using TracePass.Application.Organizations;
using TracePass.Domain.Entities;
using TracePass.Infrastructure.Data;

namespace TracePass.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _dbContext;

    public OrganizationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrganizationResponse>> GetAllAsync()
    {
        var organizations = await _dbContext.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return organizations.Select(MapToResponse).ToList();
    }

    public async Task<OrganizationResponse?> GetByIdAsync(Guid id)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return organization is null
            ? null
            : MapToResponse(organization);
    }

    public async Task<OrganizationResponse> CreateAsync(
        OrganizationRequest request)
    {
        var organization = new Organization
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            Country = request.Country.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            IsActive = true
        };

        _dbContext.Organizations.Add(organization);

        await _dbContext.SaveChangesAsync();

        return MapToResponse(organization);
    }

    public async Task<OrganizationResponse?> UpdateAsync(
        Guid id,
        OrganizationRequest request)
    {
        var organization = await _dbContext.Organizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization is null)
        {
            return null;
        }

        organization.Name = request.Name.Trim();
        organization.Type = request.Type;
        organization.RegistrationNumber =
            request.RegistrationNumber?.Trim();
        organization.Country = request.Country.Trim();
        organization.ContactEmail = request.ContactEmail?.Trim();
        organization.ContactPhone = request.ContactPhone?.Trim();
        organization.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(organization);
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var organization = await _dbContext.Organizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization is null)
        {
            return false;
        }

        organization.IsActive = false;
        organization.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static OrganizationResponse MapToResponse(
        Organization organization)
    {
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Type = organization.Type,
            RegistrationNumber = organization.RegistrationNumber,
            Country = organization.Country,
            ContactEmail = organization.ContactEmail,
            ContactPhone = organization.ContactPhone,
            IsVerified = organization.IsVerified,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }
}