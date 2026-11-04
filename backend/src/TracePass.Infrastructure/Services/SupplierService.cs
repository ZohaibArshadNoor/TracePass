using Microsoft.EntityFrameworkCore;
using TracePass.Application.Common.Paging;
using TracePass.Application.Suppliers;
using TracePass.Domain.Entities;
using TracePass.Infrastructure.Data;

namespace TracePass.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _dbContext;

    public SupplierService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SupplierProfileDto>> GetPagedAsync(PagedRequest request)
    {
        var query = _dbContext.Organizations
            .AsNoTracking()
            .Where(o => o.Type == OrganizationType.Supplier && o.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(o =>
                o.Name.ToLower().Contains(search) ||
                o.Country.ToLower().Contains(search) ||
                (o.ContactEmail != null && o.ContactEmail.ToLower().Contains(search)));
        }

        var isDescending = request.SortDescending.GetValueOrDefault(false);
        query = request.SortBy?.ToLower() switch
        {
            "country" => isDescending ? query.OrderByDescending(o => o.Country) : query.OrderBy(o => o.Country),
            _ => isDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name)
        };

        var totalCount = await query.CountAsync();

        if (request.IsPagingRequested)
        {
            query = query
                .Skip((request.ActualPageNumber - 1) * request.ActualPageSize)
                .Take(request.ActualPageSize);
        }

        var organizations = await query.ToListAsync();
        var orgIds = organizations.Select(o => o.Id).ToList();

        var profiles = await _dbContext.SupplierProfiles
            .Include(p => p.MaterialCategories)
            .Where(p => orgIds.Contains(p.OrganizationId))
            .ToDictionaryAsync(p => p.OrganizationId);

        var items = organizations.Select(org =>
        {
            profiles.TryGetValue(org.Id, out var profile);
            return new SupplierProfileDto
            {
                Id = profile?.Id ?? Guid.Empty,
                OrganizationId = org.Id,
                OrganizationName = org.Name,
                Country = org.Country,
                RegistrationNumber = org.RegistrationNumber,
                ContactEmail = org.ContactEmail,
                ContactPhone = org.ContactPhone,
                IsVerified = org.IsVerified,
                Rating = profile?.Rating ?? 0.0m,
                Notes = profile?.Notes,
                SupportedCategories = profile?.MaterialCategories.Select(c => c.Category.ToString()).ToList() ?? new List<string>(),
                CreatedAt = profile?.CreatedAt ?? org.CreatedAt
            };
        }).ToList();

        var returnedPageSize = request.IsPagingRequested ? request.ActualPageSize : (totalCount == 0 ? 1 : totalCount);
        return PagedResult<SupplierProfileDto>.Create(items, totalCount, request.ActualPageNumber, returnedPageSize);
    }

    public async Task<SupplierProfileDto?> GetByOrganizationIdAsync(Guid organizationId)
    {
        var org = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.Type == OrganizationType.Supplier);

        if (org == null)
        {
            return null;
        }

        var profile = await _dbContext.SupplierProfiles
            .Include(p => p.MaterialCategories)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId);

        return new SupplierProfileDto
        {
            Id = profile?.Id ?? Guid.Empty,
            OrganizationId = org.Id,
            OrganizationName = org.Name,
            Country = org.Country,
            RegistrationNumber = org.RegistrationNumber,
            ContactEmail = org.ContactEmail,
            ContactPhone = org.ContactPhone,
            IsVerified = org.IsVerified,
            Rating = profile?.Rating ?? 0.0m,
            Notes = profile?.Notes,
            SupportedCategories = profile?.MaterialCategories.Select(c => c.Category.ToString()).ToList() ?? new List<string>(),
            CreatedAt = profile?.CreatedAt ?? org.CreatedAt
        };
    }

    public async Task<SupplierProfileDto> CreateSupplierAsync(CreateSupplierRequest request)
    {
        var organization = new Organization
        {
            Name = request.Name.Trim(),
            Type = OrganizationType.Supplier,
            Country = request.Country.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Organizations.Add(organization);

        var profile = new SupplierProfile
        {
            OrganizationId = organization.Id,
            Rating = request.Rating,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.MaterialCategories != null && request.MaterialCategories.Any())
        {
            foreach (var category in request.MaterialCategories.Distinct())
            {
                profile.MaterialCategories.Add(new SupplierMaterialCategory
                {
                    SupplierProfileId = profile.Id,
                    Category = category
                });
            }
        }

        _dbContext.SupplierProfiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        return new SupplierProfileDto
        {
            Id = profile.Id,
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            Country = organization.Country,
            RegistrationNumber = organization.RegistrationNumber,
            ContactEmail = organization.ContactEmail,
            ContactPhone = organization.ContactPhone,
            IsVerified = organization.IsVerified,
            Rating = profile.Rating,
            Notes = profile.Notes,
            SupportedCategories = profile.MaterialCategories.Select(c => c.Category.ToString()).ToList(),
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<SupplierProfileDto?> UpdateProfileAsync(Guid organizationId, UpdateSupplierProfileRequest request)
    {
        var org = await _dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.Type == OrganizationType.Supplier);

        if (org == null)
        {
            return null;
        }

        var profile = await _dbContext.SupplierProfiles
            .Include(p => p.MaterialCategories)
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId);

        if (profile == null)
        {
            profile = new SupplierProfile
            {
                OrganizationId = organizationId,
                Rating = request.Rating,
                Notes = request.Notes?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.SupplierProfiles.Add(profile);
        }
        else
        {
            profile.Rating = request.Rating;
            profile.Notes = request.Notes?.Trim();
            profile.UpdatedAt = DateTimeOffset.UtcNow;
            profile.MaterialCategories.Clear();
        }

        foreach (var category in request.MaterialCategories.Distinct())
        {
            profile.MaterialCategories.Add(new SupplierMaterialCategory
            {
                SupplierProfileId = profile.Id,
                Category = category
            });
        }

        await _dbContext.SaveChangesAsync();

        return await GetByOrganizationIdAsync(organizationId);
    }
}
