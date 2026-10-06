using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Commands.CreatePriceAlert;

public sealed class CreatePriceAlertCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPersistAlertAndReturnId()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("CreatePriceAlertCommandHandlerTests")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var handler = new CreatePriceAlertCommandHandler(context);

        var command = new CreatePriceAlertCommand
        {
            Symbol = "ethusdt",
            AssetType = AssetType.CryptoSpot,
            Condition = AlertCondition.Above,
            TargetPrice = 2000m,
            Note = "buy when green",
            IsOneTime = false
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        var saved = await context.Set<PriceAlert>().FirstOrDefaultAsync(a => a.Id == result, CancellationToken.None);
        saved.Should().NotBeNull();
        saved!.Symbol.Should().Be("ETHUSDT");
        saved.Note.Should().Be("buy when green");
        saved.IsEnabled.Should().BeTrue();
    }
}
