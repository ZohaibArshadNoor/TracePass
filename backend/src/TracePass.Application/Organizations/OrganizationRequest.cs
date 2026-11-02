using TracePass.Domain.Entities;

namespace TracePass.Application.Organizations;

public class OrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public OrganizationType Type { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}