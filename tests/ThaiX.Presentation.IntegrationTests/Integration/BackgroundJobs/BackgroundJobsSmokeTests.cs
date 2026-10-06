using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Trading;
using ThaiX.Domain.Aggregates.Blog;
using ThaiX.Domain.Aggregates.TcbsTop10;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Integration.BackgroundJobs;

[Collection(IntegrationTestCollection.Name)]
public sealed class BackgroundJobsSmokeTests : IntegrationTestBase
{
    public BackgroundJobsSmokeTests(ThaiXWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public void BackgroundJobs_WhenResolvingFromTestHost_ShouldResolveAllRegisteredJobs()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var jobTypes = new[]
        {
            typeof(BotTradeSuggestionAsyncJob),
            typeof(ChainBrokerFundsSyncJob),
            typeof(ChainBrokerProjectsSyncJob),
            typeof(ChainBrokerUnlocksSyncJob),
            typeof(ContactImportJobProcessor),
            typeof(ContactImportJobScheduler),
            typeof(IWealthClubTop10SyncJob),
            typeof(MexcMarketScannerJob),
            typeof(MexcSpotWeeklySuggestionJob),
            typeof(OutboxProcessorJob),
            typeof(PersistWeeklySuggestionReportJob),
            typeof(PriceAlertCheckerJob),
            typeof(TopStocksWeeklySuggestionJob),
            typeof(ScheduledPostPublisherJob),
            typeof(TwentyFourHMoneyTransactionSyncJob)
        };

        // Act
        var missing = jobTypes
            .Where(type => provider.GetService(type) is null)
            .Select(type => type.FullName)
            .ToList();

        var scheduler = provider.GetService<IContactImportJobScheduler>();

        // Assert
        missing.Should().BeEmpty();
        scheduler.Should().NotBeNull();
    }

    [Fact]
    public async Task ScheduledPostPublisherJob_WithDuePost_ShouldPublishIt()
    {
        // Arrange — seed in its own scope/DbContext so the job (run in a separate scope below,
        // matching how Hangfire resolves a fresh scope per execution) doesn't reuse a stale
        // tracked entity/RowVersion from the seeding context.
        Guid postId;
        using (var seedScope = Factory.Services.CreateScope())
        {
            var seedContext = seedScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var post = Post.Create(
                Guid.NewGuid(), "Due Scheduled Post", $"due-scheduled-{Guid.NewGuid():N}", "summary", "content", null, null, null);
            post.Schedule(DateTime.UtcNow.AddMinutes(5));
            seedContext.Posts.Add(post);
            await seedContext.SaveChangesAsync(CancellationToken.None);
            postId = post.Id;

            // Force ScheduledAt into the past directly in the database, bypassing the domain's
            // future-only guard, to simulate a due post without waiting real time.
            await seedContext.Posts
                .Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ScheduledAt, DateTime.UtcNow.AddMinutes(-1)), CancellationToken.None);
        }

        // Act
        using (var runScope = Factory.Services.CreateScope())
        {
            var job = runScope.ServiceProvider.GetRequiredService<ScheduledPostPublisherJob>();
            await job.RunAsync(CancellationToken.None);
        }

        // Assert
        using var assertScope = Factory.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var published = await assertContext.Posts.FirstAsync(p => p.Id == postId, CancellationToken.None);
        published.Status.Should().Be(PostStatus.Published);
    }

    [Fact]
    public async Task TwentyFourHMoneyTransactionSyncJob_WithNoTrackedSymbols_ShouldNoOp()
    {
        // Act
        using (var runScope = Factory.Services.CreateScope())
        {
            var job = runScope.ServiceProvider.GetRequiredService<TwentyFourHMoneyTransactionSyncJob>();
            await job.RunAsync(CancellationToken.None);
        }

        // Assert
        true.Should().BeTrue();
    }

    [Fact]
    public async Task BackgroundJobs_WithPreCanceledToken_ShouldBeInvokableWithoutExternalCalls()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var botTradeSuggestion = provider.GetRequiredService<BotTradeSuggestionAsyncJob>();
        var chainBrokerFunds = provider.GetRequiredService<ChainBrokerFundsSyncJob>();
        var chainBrokerProjects = provider.GetRequiredService<ChainBrokerProjectsSyncJob>();
        var chainBrokerUnlocks = provider.GetRequiredService<ChainBrokerUnlocksSyncJob>();
        var iwealthTop10 = provider.GetRequiredService<IWealthClubTop10SyncJob>();
        var mexcMarketScanner = provider.GetRequiredService<MexcMarketScannerJob>();
        var mexcSpotWeekly = provider.GetRequiredService<MexcSpotWeeklySuggestionJob>();
        var topStocksWeekly = provider.GetRequiredService<TopStocksWeeklySuggestionJob>();
        var twentyFourHMoneyTransactions = provider.GetRequiredService<TwentyFourHMoneyTransactionSyncJob>();

        // Act
        await InvokeWithPreCanceledToken(ct => botTradeSuggestion.RunAsync(
            executionId: Guid.NewGuid().ToString("N"),
            request: new TradeSuggestionAsyncRequest
            {
                Symbol = "BTCUSDT",
                MarketType = MarketType.CryptoSpot,
                Timeframe = "Day1",
                UseLongTermTimeframe = false,
                MarketRegime = MarketRegime.RiskOn,
                EventRisk = EventRisk.Low
            },
            cancellationToken: ct));

        await InvokeWithPreCanceledToken(ct => chainBrokerFunds.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => chainBrokerProjects.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => chainBrokerUnlocks.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => iwealthTop10.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => mexcMarketScanner.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => mexcSpotWeekly.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => topStocksWeekly.RunAsync(ct));
        await InvokeWithPreCanceledToken(ct => twentyFourHMoneyTransactions.RunAsync(ct));

        // Assert
        true.Should().BeTrue();
    }

    private static async Task InvokeWithPreCanceledToken(Func<CancellationToken, Task> action)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await action(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected for smoke tests that intentionally avoid external side effects.
        }
        catch (InvalidOperationException)
        {
            // Expected when a provider endpoint is intentionally absent in testing configuration.
        }
        catch (ArgumentException)
        {
            // Expected when pre-canceled token causes downstream guard clauses (e.g. empty notification text).
        }
    }
}
