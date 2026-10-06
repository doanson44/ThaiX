using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Application.UnitTests.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;

public sealed class GetTcbsTop10PortfoliosQueryHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_ShouldReturnCurrentHoldingsDerivedFromChanges()
    {
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);

        var older = TcbsTop10Portfolio.Create(1, DateOnly.Parse("2026-05-01"), DateOnly.Parse("2026-05-01"));
        older.AddTicker("A", TcbsPortfolioChangeType.Added);
        older.AddTicker("B", TcbsPortfolioChangeType.Added);
        older.AddTicker("C", TcbsPortfolioChangeType.Added);
        older.AddImage(TcbsPortfolioImageType.PeriodPerformance, Guid.NewGuid());

        var newer = TcbsTop10Portfolio.Create(2, DateOnly.Parse("2026-06-01"), DateOnly.Parse("2026-06-01"));
        newer.AddTicker("A", TcbsPortfolioChangeType.Removed);
        newer.AddTicker("B", TcbsPortfolioChangeType.Added);
        newer.AddTicker("D", TcbsPortfolioChangeType.Added);
        newer.AddImage(TcbsPortfolioImageType.PortfolioDetail, Guid.NewGuid());

        context.Add(older);
        context.Add(newer);
        await context.SaveChangesAsync();

        var handler = new GetTcbsTop10PortfoliosQueryHandler(context);
        var result = await handler.Handle(new GetTcbsTop10PortfoliosQuery(), CancellationToken.None);

        result.Portfolios.Should().HaveCount(2);
        result.CurrentHoldings.Should().BeEquivalentTo(new[] { "B", "C", "D" }, options => options.WithStrictOrdering());
        result.Portfolios.SelectMany(p => p.Images).Should().ContainSingle(i => i.ImageType == (int)TcbsPortfolioImageType.PortfolioDetail);
    }
}
