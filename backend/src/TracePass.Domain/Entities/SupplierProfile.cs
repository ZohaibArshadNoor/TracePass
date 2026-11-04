namespace TracePass.Domain.Entities;

public class SupplierProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public decimal Rating { get; set; } = 0.0m;

    public string? Notes { get; set; }

    public ICollection<SupplierMaterialCategory> MaterialCategories { get; set; } = new List<SupplierMaterialCategory>();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
