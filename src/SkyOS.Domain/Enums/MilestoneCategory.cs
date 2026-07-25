namespace SkyOS.Domain.Enums;

/// <summary>
/// Classifies a <see cref="Entities.Milestone"/>. Extending this enum (plus a matching
/// display mapping) is the only change needed to introduce a new milestone type (OCP).
/// </summary>
public enum MilestoneCategory
{
    Teknofest = 1,
    Kurumsal = 2,
    Teknik = 3,
}
