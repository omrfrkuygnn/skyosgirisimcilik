namespace SkyOS.Domain.Common;

/// <summary>
/// Base type for every persisted entity. Kept as a pure POCO — no data-annotation
/// attributes here. All persistence concerns live in the Infrastructure configurations.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
