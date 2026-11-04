using TracePass.Domain.Entities;

namespace TracePass.Application.Suppliers;

public class SupplierProfileDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsVerified { get; set; }
    public decimal Rating { get; set; }
    public string? Notes { get; set; }
    public IList<string> SupportedCategories { get; set; } = new List<string>();
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public decimal Rating { get; set; } = 0.0m;
    public string? Notes { get; set; }
    public IList<MaterialCategory> MaterialCategories { get; set; } = new List<MaterialCategory>();
}

public class UpdateSupplierProfileRequest
{
    public decimal Rating { get; set; }
    public string? Notes { get; set; }
    public IList<MaterialCategory> MaterialCategories { get; set; } = new List<MaterialCategory>();
}
