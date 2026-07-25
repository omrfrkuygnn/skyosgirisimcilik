using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyOS.Domain.Entities;

namespace SkyOS.Infrastructure.Persistence.Configurations;

public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.PhotoUrl).HasMaxLength(400);

        builder.HasIndex(x => x.DisplayOrder);

        // Only the two confirmed leaders are seeded; remaining members are added via CMS later.
        builder.HasData(
            new TeamMember
            {
                Id = 1,
                FullName = "Yunus Emre Gözalıcı",
                Role = "Ekip Lideri",
                IsLeader = true,
                DisplayOrder = 1,
                CreatedAtUtc = SeedTimestamps.Initial,
            },
            new TeamMember
            {
                Id = 2,
                FullName = "Enver Sabri Özkartal",
                Role = "Ekip Lideri",
                IsLeader = true,
                DisplayOrder = 2,
                CreatedAtUtc = SeedTimestamps.Initial,
            });
    }
}
