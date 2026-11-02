using TracePass.Domain.Entities;

namespace TracePass.Application.Organizations;

public class OrganizationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationType Type { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}