using TracePass.Domain.Entities;

namespace TracePass.Application.Materials;

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MaterialCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public string OriginCountry { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public MaterialCategory Category { get; set; }
    public string OriginCountry { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public MaterialCategory Category { get; set; }
    public string OriginCountry { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
