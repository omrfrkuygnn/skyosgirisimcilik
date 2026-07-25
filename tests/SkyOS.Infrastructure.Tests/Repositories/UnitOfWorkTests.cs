using FluentAssertions;
using SkyOS.Domain.Entities;
using SkyOS.Domain.Enums;
using SkyOS.Infrastructure.Repositories;
using SkyOS.Infrastructure.Tests.TestSupport;

namespace SkyOS.Infrastructure.Tests.Repositories;

public sealed class UnitOfWorkTests : IClassFixture<SqliteDbContextFactory>
{
    private readonly SqliteDbContextFactory _factory;

    public UnitOfWorkTests(SqliteDbContextFactory factory) => _factory = factory;

    [Fact]
    public async Task Repository_ReturnsSameInstancePerType()
    {
        await using var context = _factory.Create();
        var unitOfWork = new UnitOfWork(context);

        var first = unitOfWork.Repository<TeamMember>();
        var second = unitOfWork.Repository<TeamMember>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task SeededLeaders_AreQueryable()
    {
        await using var context = _factory.Create();
        var unitOfWork = new UnitOfWork(context);

        var leaders = await unitOfWork.Repository<TeamMember>().ListAsync(m => m.IsLeader);

        leaders.Should().HaveCount(2);
        leaders.Select(l => l.FullName).Should().Contain(["Yunus Emre Gözalıcı", "Enver Sabri Özkartal"]);
    }

    [Fact]
    public async Task SeededMilestones_ContainCorporateVerification()
    {
        await using var context = _factory.Create();
        var unitOfWork = new UnitOfWork(context);

        var corporate = await unitOfWork.Repository<Milestone>().ListAsync(m => m.Category == MilestoneCategory.Kurumsal);

        corporate.Should().ContainSingle();
        corporate[0].Title.Should().Contain("HAVELSAN");
    }

    [Fact]
    public async Task AddingContactMessage_SetsCreatedTimestampAndPersists()
    {
        await using var context = _factory.Create();
        var unitOfWork = new UnitOfWork(context);
        var repository = unitOfWork.Repository<ContactMessage>();

        var message = new ContactMessage
        {
            FullName = "Grace Hopper",
            Email = "grace@example.com",
            InterestType = InterestType.Basin,
            Message = "Basın bülteni talebi.",
            IpAddress = "198.51.100.7",
        };

        await repository.AddAsync(message);
        await unitOfWork.SaveChangesAsync();

        message.Id.Should().BeGreaterThan(0);
        message.CreatedAtUtc.Should().Be(new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc));

        var count = await repository.CountAsync(m => m.IpAddress == "198.51.100.7");
        count.Should().Be(1);
    }
}
