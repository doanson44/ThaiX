using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Lottery.Queries.GetPower655Analysis;
using ThaiX.Domain.Aggregates.Lottery;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class LotteryAnalysisEndpointsTests : IntegrationTestBase
{
    public LotteryAnalysisEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    // ===================================================================
    // Happy path
    // ===================================================================

    [Fact]
    public async Task GetPower655Analysis_WithSeededData_ShouldReturnAllDraws()
    {
        // Arrange
        await SeedDrawsAsync(30);

        // Act
        var response = await Client.GetAsync("/api/lottery/power-655/analysis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<Power655AnalysisResponse>>();
        var data = envelope!.Data!;

        data.Draws.Should().HaveCount(30);
        data.Draws.Should().BeInDescendingOrder(d => d.DrawDate);
    }

    // ===================================================================
    // Frequency statistics
    // ===================================================================

    [Fact]
    public async Task GetPower655Analysis_WithFrequencyData_ShouldCountNumbersCorrectly()
    {
        // Arrange: 5 draws where number 55 appears in every draw, number 1 appears once
        await CleanupDrawsAsync();

        var draws = new List<Power655Result>();
        for (var i = 0; i < 5; i++)
        {
            var date = new DateOnly(2026, 7, 14).AddDays(-i * 3);
            var nums = new List<int> { 55, 2 + i, 10 + i, 20 + i, 30 + i, 40 + i };
            draws.Add(Power655Result.Create(date, "T7", nums[0], nums[1], nums[2], nums[3], nums[4], nums[5], 33, 100000000, 0));
        }

        // Extra draw with number 1
        draws.Add(Power655Result.Create(new DateOnly(2026, 7, 1), "T3", 1, 2, 3, 4, 5, 6, 7, 200000000, 0));

        await SeedEntitiesAsync(draws);

        // Act
        var response = await Client.GetAsync("/api/lottery/power-655/analysis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<Power655AnalysisResponse>>();
        var data = envelope!.Data!;

        data.TopMostFrequent.Should().NotBeEmpty();
        data.TopLeastFrequent.Should().NotBeEmpty();

        // Number 55 appeared 5 times -- should be in top 6 most frequent
        var topFreq = data.TopMostFrequent.First();
        topFreq.Number.Should().Be(55);
        topFreq.Frequency.Should().Be(5);

        // Number 1 appeared 1 time -- should be in top 6 least frequent
        data.TopLeastFrequent.Should().Contain(x => x.Number == 1 && x.Frequency == 1);
    }

    // ===================================================================
    // Predictions
    // ===================================================================

    [Fact]
    public async Task GetPower655Analysis_WithPredictions_ShouldIncludeAllPredictions()
    {
        // Arrange
        await CleanupDrawsAsync();

        List<Power655Result> draws =
        [
            Power655Result.Create(new DateOnly(2026, 7, 14), "T7", 1, 2, 3, 10, 11, 12, 33, 100000000, 0),
        ];

        await SeedEntitiesAsync(draws);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            db.Power655Predictions.Add(Power655Prediction.Create(
                new DateOnly(2026, 7, 14), PredictionType.Pick6,
                1, 2, 3, 10, 11, 12, 3, "Test prediction"));
            db.Power655Predictions.Add(Power655Prediction.Create(
                new DateOnly(2026, 7, 15), PredictionType.Pick3,
                5, 10, 15, 20, 25, 30, 1, "Future prediction"));
            await db.SaveChangesAsync(CancellationToken.None);
        }

        // Act
        var response = await Client.GetAsync("/api/lottery/power-655/analysis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<Power655AnalysisResponse>>();
        var data = envelope!.Data!;

        data.Predictions.Should().HaveCount(2);
        data.Predictions.Should().BeInDescendingOrder(p => p.TargetDrawDate);

        var pick6 = data.Predictions.First(p => p.PredictionType == "Pick6");
        pick6.Numbers.Should().Equal([1, 2, 3, 10, 11, 12]);
        pick6.TupleFrequency.Should().Be(3);
        pick6.Reasoning.Should().Be("Test prediction");
        pick6.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetPower655Analysis_WithCompletedPrediction_ShouldShowMatchedNumbers()
    {
        // Arrange
        await CleanupDrawsAsync();

        // Create a draw result first
        var result = Power655Result.Create(
            new DateOnly(2026, 7, 11), "T7", 1, 2, 3, 10, 11, 12, 33, 100000000, 0);

        List<Power655Result> draws = [result];
        await SeedEntitiesAsync(draws);

        // Create a prediction and link it to the result via DB
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var prediction = Power655Prediction.Create(
                new DateOnly(2026, 7, 11), PredictionType.Pick6,
                1, 2, 3, 10, 11, 12, 3, "Matched all 6");
            prediction.SetResult(result.Id, 6);
            db.Power655Predictions.Add(prediction);
            await db.SaveChangesAsync(CancellationToken.None);
        }

        // Act
        var response = await Client.GetAsync("/api/lottery/power-655/analysis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<Power655AnalysisResponse>>();
        var data = envelope!.Data!;

        data.Predictions.Should().HaveCount(1);
        var pred = data.Predictions[0];
        pred.IsCompleted.Should().BeTrue();
        pred.MatchedNumbers.Should().Be(6);
        pred.Numbers.Should().Equal([1, 2, 3, 10, 11, 12]);
        pred.TupleFrequency.Should().Be(3);
        pred.Reasoning.Should().Be("Matched all 6");
    }

    // ===================================================================
    // Empty database
    // ===================================================================

    [Fact]
    public async Task GetPower655Analysis_WithNoData_ShouldReturnEmptyResults()
    {
        // Arrange
        await CleanupDrawsAsync();

        // Act
        var response = await Client.GetAsync("/api/lottery/power-655/analysis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<Power655AnalysisResponse>>();
        var data = envelope!.Data!;

        data.Draws.Should().BeEmpty();
        data.TopMostFrequent.Should().BeEmpty();
        data.TopLeastFrequent.Should().BeEmpty();
        data.Predictions.Should().BeEmpty();
    }

    // ===================================================================
    // Seed helpers
    // ===================================================================

    private async Task CleanupDrawsAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var all = await db.Power655Results.ToListAsync();
        db.Power655Results.RemoveRange(all);
        var allPreds = await db.Power655Predictions.ToListAsync();
        db.Power655Predictions.RemoveRange(allPreds);
        await db.SaveChangesAsync(CancellationToken.None);
        await cache.InvalidateGroupAsync(CacheGroups.Lottery, CancellationToken.None);
    }

    private async Task SeedDrawsAsync(int count)
    {
        await CleanupDrawsAsync();

        var draws = new List<Power655Result>();
        var startDate = new DateOnly(2026, 7, 14);
        for (var i = 0; i < count; i++)
        {
            var date = startDate.AddDays(-i * 3);
            var rng = new Random(date.DayNumber);
            var numbers = Enumerable.Range(1, 55).OrderBy(_ => rng.Next()).Take(6).OrderBy(n => n).ToList();
            draws.Add(Power655Result.Create(
                date,
                date.DayOfWeek switch
                {
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Saturday => "T7",
                    _ => "CN"
                },
                numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5],
                33,
                100000000000,
                0));
        }

        await SeedEntitiesAsync(draws);
    }

    private async Task SeedEntitiesAsync(IReadOnlyList<Power655Result> draws)
    {
        using var scope = Factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        db.Power655Results.AddRange(draws);
        await db.SaveChangesAsync(CancellationToken.None);
        await cache.InvalidateGroupAsync(CacheGroups.Lottery, CancellationToken.None);
    }
}
