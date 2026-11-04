namespace TracePass.Application.Organizations;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationResponse>> GetAllAsync();

    Task<OrganizationResponse?> GetByIdAsync(Guid id);

    Task<OrganizationResponse?> GetAccessibleByIdAsync(Guid id);

    Task<OrganizationResponse> CreateAsync(
        OrganizationRequest request);

    Task<OrganizationResponse?> UpdateAsync(
        Guid id,
        OrganizationRequest request);

    Task<bool> DeactivateAsync(Guid id);
}