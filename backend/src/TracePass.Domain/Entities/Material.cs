namespace TracePass.Domain.Entities;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public MaterialCategory Category { get; set; }

    public string OriginCountry { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
