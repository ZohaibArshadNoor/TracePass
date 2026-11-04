using Microsoft.EntityFrameworkCore;
using TracePass.Application.Common.Paging;
using TracePass.Application.Materials;
using TracePass.Domain.Entities;
using TracePass.Infrastructure.Data;

namespace TracePass.Infrastructure.Services;

public class MaterialService : IMaterialService
{
    private readonly ApplicationDbContext _dbContext;

    public MaterialService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MaterialDto>> GetPagedAsync(PagedRequest request, string? category = null)
    {
        var query = _dbContext.Materials.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(search) ||
                m.OriginCountry.ToLower().Contains(search) ||
                (m.Notes != null && m.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<MaterialCategory>(category, true, out var parsedCategory))
        {
            query = query.Where(m => m.Category == parsedCategory);
        }

        var isDescending = request.SortDescending.GetValueOrDefault(false);
        query = request.SortBy?.ToLower() switch
        {
            "category" => isDescending ? query.OrderByDescending(m => m.Category) : query.OrderBy(m => m.Category),
            "origincountry" or "country" => isDescending ? query.OrderByDescending(m => m.OriginCountry) : query.OrderBy(m => m.OriginCountry),
            _ => isDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name)
        };

        var totalCount = await query.CountAsync();

        // If pagination is requested, apply Skip & Take. Otherwise, fetch all records.
        if (request.IsPagingRequested)
        {
            query = query
                .Skip((request.ActualPageNumber - 1) * request.ActualPageSize)
                .Take(request.ActualPageSize);
        }

        var items = await query
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Name = m.Name,
                Category = m.Category,
                OriginCountry = m.OriginCountry,
                Notes = m.Notes,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        var returnedPageSize = request.IsPagingRequested ? request.ActualPageSize : (totalCount == 0 ? 1 : totalCount);
        return PagedResult<MaterialDto>.Create(items, totalCount, request.ActualPageNumber, returnedPageSize);
    }

    public async Task<MaterialDto?> GetByIdAsync(Guid id)
    {
        var material = await _dbContext.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return material == null ? null : MapToDto(material);
    }

    public async Task<MaterialDto> CreateAsync(CreateMaterialRequest request)
    {
        var material = new Material
        {
            Name = request.Name.Trim(),
            Category = request.Category,
            OriginCountry = request.OriginCountry.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Materials.Add(material);
        await _dbContext.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task<MaterialDto?> UpdateAsync(Guid id, UpdateMaterialRequest request)
    {
        var material = await _dbContext.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (material == null)
        {
            return null;
        }

        material.Name = request.Name.Trim();
        material.Category = request.Category;
        material.OriginCountry = request.OriginCountry.Trim();
        material.Notes = request.Notes?.Trim();
        material.IsActive = request.IsActive;
        material.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var material = await _dbContext.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (material == null)
        {
            return false;
        }

        material.IsActive = false;
        material.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static MaterialDto MapToDto(Material m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Category = m.Category,
        OriginCountry = m.OriginCountry,
        Notes = m.Notes,
        IsActive = m.IsActive,
        CreatedAt = m.CreatedAt
    };
}
