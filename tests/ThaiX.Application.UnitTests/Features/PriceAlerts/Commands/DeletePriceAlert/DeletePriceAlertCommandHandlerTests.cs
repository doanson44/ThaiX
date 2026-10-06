using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.PriceAlerts.Commands.DeletePriceAlert;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Commands.DeletePriceAlert;

public sealed class DeletePriceAlertCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingAlert_ShouldSoftDeleteAlert()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("DeletePriceAlertCommandHandlerTests")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var existing = PriceAlert.Create("BTCUSDT", AssetType.CryptoSpot, AlertCondition.Below, 10000m, null, false);
        context.Set<PriceAlert>().Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeletePriceAlertCommandHandler(context);
        var command = new DeletePriceAlertCommand { Id = existing.Id };

        await handler.Handle(command, CancellationToken.None);

        var deleted = await context.Set<PriceAlert>().IgnoreQueryFilters().FirstAsync(a => a.Id == existing.Id, CancellationToken.None);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenAlertDoesNotExist_ShouldThrowOperationFailedException()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("DeletePriceAlertCommandHandlerTests_NotFound")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var handler = new DeletePriceAlertCommandHandler(context);

        var command = new DeletePriceAlertCommand { Id = Guid.NewGuid() };

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.OperationFailedException>()
            .WithMessage("Price alert '* not found.");
    }
}
