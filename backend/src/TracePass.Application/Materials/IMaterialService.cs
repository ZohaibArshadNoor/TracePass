using TracePass.Application.Common.Paging;

namespace TracePass.Application.Materials;

public interface IMaterialService
{
    Task<PagedResult<MaterialDto>> GetPagedAsync(PagedRequest request, string? category = null);

    Task<MaterialDto?> GetByIdAsync(Guid id);

    Task<MaterialDto> CreateAsync(CreateMaterialRequest request);

    Task<MaterialDto?> UpdateAsync(Guid id, UpdateMaterialRequest request);

    Task<bool> DeleteAsync(Guid id);
}
