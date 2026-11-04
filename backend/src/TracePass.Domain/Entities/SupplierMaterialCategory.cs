namespace TracePass.Domain.Entities;

public class SupplierMaterialCategory
{
    public Guid SupplierProfileId { get; set; }
    public SupplierProfile SupplierProfile { get; set; } = null!;

    public MaterialCategory Category { get; set; }
}
