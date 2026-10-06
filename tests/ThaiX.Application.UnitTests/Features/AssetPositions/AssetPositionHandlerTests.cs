using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.AssetPositions.Commands.CreateCryptoPosition;
using ThaiX.Application.Features.AssetPositions.Commands.CreateStockPosition;
using ThaiX.Application.Features.AssetPositions.Commands.UpdateStockPositionTargets;
using ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.UnitTests.Features.AssetPositions;

public sealed class AssetPositionHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task CreateStockPositionCommandHandler_WhenPortfolioDoesNotExist_ThrowsNotFound()
    {
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var handler = new CreateStockPositionCommandHandler(context, currentUser.Object);

        await handler.Invoking(x => x.Handle(new CreateStockPositionCommand
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "AAPL",
            Exchange = StockExchange.HOSE,
            Quantity = 10m,
            Price = 100m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TransactionNote = "Initial buy",
            ExternalRef = "ref-1"
        }, CancellationToken.None))
            .Should().ThrowAsync<OperationFailedException>()
            .Where(ex => ex.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND);
    }

    [Fact]
    public async Task CreateStockPositionCommandHandler_WhenPortfolioExists_AddsPosition()
    {
        var userId = Guid.NewGuid();
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var portfolio = Portfolio.Create(userId, "Test Portfolio", PortfolioType.Trading, null);
        context.Add(portfolio);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        var handler = new CreateStockPositionCommandHandler(context, currentUser.Object);
        var command = new CreateStockPositionCommand
        {
            PortfolioId = portfolio.Id,
            Symbol = "VIC",
            Exchange = StockExchange.HOSE,
            Quantity = 10m,
            Price = 100m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TransactionNote = "Buy shares",
            ExternalRef = "ref-1"
        };

        var positionId = await handler.Handle(command, CancellationToken.None);
        var position = await context.Set<StockPosition>().FindAsync(positionId);

        position.Should().NotBeNull();
        position!.Symbol.Should().Be("VIC");
        position.Quantity.Should().Be(10m);
        position.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateCryptoPositionCommandHandler_WhenPortfolioExists_AddsCryptoPosition()
    {
        var userId = Guid.NewGuid();
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var portfolio = Portfolio.Create(userId, "Crypto Guard", PortfolioType.Trading, null);
        context.Add(portfolio);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        var handler = new CreateCryptoPositionCommandHandler(context, currentUser.Object);
        var command = new CreateCryptoPositionCommand
        {
            PortfolioId = portfolio.Id,
            Symbol = "BTCUSDT",
            Quantity = 1m,
            Price = 30000m,
            Fee = 10m,
            TransactedAt = DateTime.UtcNow,
            TransactionNote = "Initial crypto buy",
            ExternalRef = "ref-crypto-1"
        };

        var positionId = await handler.Handle(command, CancellationToken.None);
        var position = await context.Set<CryptoPosition>().FindAsync(positionId);

        position.Should().NotBeNull();
        position!.Symbol.Should().Be("BTCUSDT");
        position.Quantity.Should().Be(1m);
        position.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public void StockPosition_AddBuy_IncreasesQuantityAndAddsTransaction()
    {
        var position = StockPosition.Create(Guid.NewGuid(), "SSI", StockExchange.HOSE, null, null, null);
        position.AddBuy(10m, 20m, 1m, DateTime.UtcNow, "Buy", "ref1");

        position.Quantity.Should().Be(10m);
        position.Transactions.Should().HaveCount(1);
        position.AverageEntryPrice.Should().Be(20m);
    }

    [Fact]
    public void StockPosition_AddSell_ClosesPositionWhenQuantityGoesToZero()
    {
        var position = StockPosition.Create(Guid.NewGuid(), "SSI", StockExchange.HOSE, null, null, null);
        position.AddBuy(5m, 20m, 0m, DateTime.UtcNow, "Buy", "ref1");
        position.AddSell(5m, 25m, 0m, DateTime.UtcNow, "Sell", "ref2");

        position.Quantity.Should().Be(0m);
        position.IsClosed.Should().BeTrue();
        position.Transactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStockPositionsQueryHandler_WhenPortfolioExists_ReturnsPagedResults()
    {
        var userId = Guid.NewGuid();
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var portfolio = Portfolio.Create(userId, "Equity", PortfolioType.Trading, null);
        context.Add(portfolio);
        var position = StockPosition.Create(portfolio.Id, "FPT", StockExchange.HOSE, null, null, "Test note");
        position.AddBuy(5m, 50m, 0m, DateTime.UtcNow, "Buy FPT", "ref3");
        context.Add(position);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetStockPositionsQueryHandler(context, currentUser.Object);
        var result = await handler.Handle(new GetStockPositionsQuery { PortfolioId = portfolio.Id, PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Symbol.Should().Be("FPT");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateStockPositionTargetsCommandHandler_WhenOwner_UpdatesTargets()
    {
        var userId = Guid.NewGuid();
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var portfolio = Portfolio.Create(userId, "Invest", PortfolioType.Trading, null);
        context.Add(portfolio);
        var position = StockPosition.Create(portfolio.Id, "VCB", StockExchange.HOSE, 100m, 95m, "Old note");
        context.Add(position);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        var handler = new UpdateStockPositionTargetsCommandHandler(context, currentUser.Object);
        await handler.Handle(new UpdateStockPositionTargetsCommand
        {
            Id = position.Id,
            TargetPrice = 120m,
            StopLoss = 90m,
            Note = "Updated note"
        }, CancellationToken.None);

        var updated = await context.Set<StockPosition>().FindAsync(position.Id);
        updated!.TargetPrice.Should().Be(120m);
        updated.StopLoss.Should().Be(90m);
        updated.Note.Should().Be("Updated note");
    }
}
