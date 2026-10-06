using Microsoft.Extensions.DependencyInjection;
using ThaiX.Application.Common.Services.Scoring;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Services;
using ThaiX.Application.Strategies;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.DependencyInjection;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IKlineStrategyFactory>().Should().BeOfType<KlineStrategyFactory>();
        provider.GetRequiredService<IBotCommandCatalog>().Should().BeOfType<BotCommandCatalog>();
        provider.GetRequiredService<IEventRiskEvaluator>().Should().NotBeNull();
        provider.GetRequiredService<IPortfolioScoreCalculator>().Should().NotBeNull();
    }
}
