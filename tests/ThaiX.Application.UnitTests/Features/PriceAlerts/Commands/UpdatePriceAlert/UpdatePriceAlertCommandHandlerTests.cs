using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Commands.UpdatePriceAlert;

public sealed class UpdatePriceAlertCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingAlert_ShouldUpdateValues()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("UpdatePriceAlertCommandHandlerTests")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var existing = PriceAlert.Create("VNM", AssetType.VnStock, AlertCondition.Below, 10000m, "initial", false);
        context.Set<PriceAlert>().Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdatePriceAlertCommandHandler(context);
        var command = new UpdatePriceAlertCommand
        {
            Id = existing.Id,
            Condition = AlertCondition.Above,
            TargetPrice = 9500m,
            Note = "updated note",
            IsOneTime = true,
            IsEnabled = false
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await context.Set<PriceAlert>().FirstAsync(a => a.Id == existing.Id, CancellationToken.None);
        updated.Condition.Should().Be(AlertCondition.Above);
        updated.TargetPrice.Should().Be(9500m);
        updated.Note.Should().Be("updated note");
        updated.IsOneTime.Should().BeTrue();
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAlertDoesNotExist_ShouldThrowOperationFailedException()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("UpdatePriceAlertCommandHandlerTests_NotFound")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var handler = new UpdatePriceAlertCommandHandler(context);

        var command = new UpdatePriceAlertCommand
        {
            Id = Guid.NewGuid(),
            Condition = AlertCondition.Above,
            TargetPrice = 1m,
            Note = null,
            IsOneTime = false,
            IsEnabled = true
        };

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.OperationFailedException>()
            .WithMessage("Price alert '* not found.");
    }
}
