namespace ThaiX.Application.Common.Services.Scoring;

public interface IStockScoringPipeline
{
    StockScoringResult Execute(StockScoringContext context);
}
