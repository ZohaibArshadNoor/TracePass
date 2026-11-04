using TracePass.Application.Common.Paging;

namespace TracePass.Application.Suppliers;

public interface ISupplierService
{
    Task<PagedResult<SupplierProfileDto>> GetPagedAsync(PagedRequest request);

    Task<SupplierProfileDto?> GetByOrganizationIdAsync(Guid organizationId);

    Task<SupplierProfileDto> CreateSupplierAsync(CreateSupplierRequest request);

    Task<SupplierProfileDto?> UpdateProfileAsync(Guid organizationId, UpdateSupplierProfileRequest request);
}
