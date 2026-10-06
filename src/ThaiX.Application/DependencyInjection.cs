using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThaiX.Application.Common.Behaviors;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Modules;
using ThaiX.Application.Features.BotCommands.Services;
using ThaiX.Application.Services;
using ThaiX.Application.Strategies;
using ThaiX.Application.Trading;

namespace ThaiX.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR with pipeline behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Register pipeline behaviors (order matters - they execute in order)
            config.AddOpenBehavior(typeof(LoggingBehavior<,>))
                    .AddOpenBehavior(typeof(ValidationBehavior<,>))
                    .AddOpenBehavior(typeof(TransactionBehavior<,>))
                    .AddOpenBehavior(typeof(QueryCachingBehavior<,>))
                    .AddOpenBehavior(typeof(CommandCacheInvalidationBehavior<,>));
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IKlineExecutionEngine, KlineExecutionEngine>();
        services.AddSingleton<IKlineStrategyFactory, KlineStrategyFactory>();
        services.AddSingleton<Common.Services.Scoring.ITechnicalScoreCalculator, Common.Services.Scoring.TechnicalScoreCalculator>();
        services.AddSingleton<Common.Services.Scoring.IPortfolioScoreCalculator, Common.Services.Scoring.PortfolioScoreCalculator>();
        services.AddSingleton<Common.Services.Scoring.IEventRiskEvaluator, Common.Services.Scoring.EventRiskEvaluator>();
        services.AddSingleton<Common.Services.Scoring.ICompositeScoreCalculator, Common.Services.Scoring.CompositeScoreCalculator>();
        services.AddSingleton<Common.Services.Scoring.ILiquidityPenaltyEvaluator, Common.Services.Scoring.LiquidityPenaltyEvaluator>();
        services.AddSingleton<Common.Services.Scoring.IStockScoringPipeline, Common.Services.Scoring.StockScoringPipeline>();

        services.AddScoped<Common.Interfaces.INotificationService, NotificationService>();

        // Bot command platform (plugin-based)
        services.AddSingleton<IBotCommandCatalog, BotCommandCatalog>();
        services.AddScoped<IBotCommandParser, BotCommandParser>();
        services.AddScoped<IBotCommandRegistry, BotCommandRegistry>();
        services.AddScoped<IBotCommandIntentResolver, BotCommandIntentResolver>();
        services.AddScoped<IBotCommandResponseNarrator, BotCommandResponseNarrator>();
        services.AddScoped<IBotConversationalResponder, BotConversationalResponder>();
        services.AddScoped<IBotCommandModule, HelpBotCommandModule>();
        services.AddScoped<IBotCommandModule, PriceCryptoBotCommandModule>();
        services.AddScoped<IBotCommandModule, PriceBotCommandModule>();
        services.AddScoped<IBotCommandModule, PriceStockBotCommandModule>();
        services.AddScoped<IBotCommandModule, PriceCryptoIndicatorsBotCommandModule>();
        services.AddScoped<IBotCommandModule, PriceStockIndicatorsBotCommandModule>();
        services.AddScoped<IBotCommandModule, SignalBotCommandModule>();
        services.AddScoped<IBotCommandModule, PortfolioBotCommandModule>();
        services.AddScoped<IBotCommandModule, AlertBotCommandModule>();
        services.AddScoped<IBotCommandModule, NoteBotCommandModule>();
        services.AddScoped<IBotCommandModule, TcbsBotCommandModule>();

        return services;
    }
}
