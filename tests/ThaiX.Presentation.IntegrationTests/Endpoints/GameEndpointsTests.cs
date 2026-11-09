using Xunit;
using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Games;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class GameEndpointsTests : IntegrationTestBase
{
    public GameEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region POST /api/games/scores
    [Fact]
    public async Task SubmitScore_ShouldReturnLeaderboardRank_WhenGuestScoreValid()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var sessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var sessionEnvelope = await sessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var request = new
        {
            GameId = "2048",
            RunId = Guid.NewGuid(),
            Score = 2048,
            DurationSeconds = 120,
            Mode = (string?)null,
            GuestPlayerId = sessionEnvelope!.Data!.PlayerId.ToString("D"),
            GuestToken = sessionEnvelope.Data.Token,
            GuestPlayerName = "Test Guest"
        };

        // Act
        var submit = await Client.PostAsJsonAsync("/api/games/scores", request);
        var submitEnvelope = await submit.Content.ReadFromJsonAsync<ApiResponse<SubmitGameScoreResultDto>>(JsonOptions);
        var leaderboard = await Client.GetAsync("/api/games/leaderboards/2048");

        // Assert
        submit.StatusCode.Should().Be(HttpStatusCode.OK);
        submitEnvelope!.Data!.Rank.Should().BeGreaterThan(0);
        leaderboard.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SubmitScore_ShouldNotCountTheSameRunTwice_WhenRequestIsRetried()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var sessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var session = await sessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var runId = Guid.NewGuid();
        var request = new
        {
            GameId = "2048",
            RunId = runId,
            Score = 2048,
            DurationSeconds = 90,
            Mode = (string?)null,
            GuestPlayerId = session!.Data!.PlayerId.ToString("D"),
            GuestToken = session.Data.Token,
            GuestPlayerName = "Idempotent Guest"
        };

        // Act
        var firstResponse = await Client.PostAsJsonAsync("/api/games/scores", request);
        var retryResponse = await Client.PostAsJsonAsync("/api/games/scores", request);
        var leaderboardResponse = await Client.GetAsync("/api/games/leaderboards/2048");
        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<ApiResponse<GameLeaderboardDto>>(JsonOptions);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        leaderboard!.Data!.Entries.Should().Contain(entry =>
            entry.PlayerName == "Idempotent Guest" && entry.GamesPlayed == 1);
    }

    [Fact]
    public async Task SubmitScore_ShouldReturnUnauthorized_WhenGuestTokenIsInvalid()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var sessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var session = await sessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var request = new
        {
            GameId = "2048",
            RunId = Guid.NewGuid(),
            Score = 100,
            DurationSeconds = 20,
            Mode = (string?)null,
            GuestPlayerId = session!.Data!.PlayerId.ToString("D"),
            GuestToken = $"{session.Data.Token}invalid",
            GuestPlayerName = "Test Guest"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/games/scores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitScore_ShouldPreserveBothEntries_WhenGuestSubmissionsAreConcurrent()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var firstSessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var secondSessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var firstSession = await firstSessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var secondSession = await secondSessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var firstRequest = new
        {
            GameId = "2048",
            RunId = Guid.NewGuid(),
            Score = 2048,
            DurationSeconds = 60,
            Mode = (string?)null,
            GuestPlayerId = firstSession!.Data!.PlayerId.ToString("D"),
            GuestToken = firstSession.Data.Token,
            GuestPlayerName = "Concurrent One"
        };
        var secondRequest = new
        {
            GameId = "2048",
            RunId = Guid.NewGuid(),
            Score = 4096,
            DurationSeconds = 55,
            Mode = (string?)null,
            GuestPlayerId = secondSession!.Data!.PlayerId.ToString("D"),
            GuestToken = secondSession.Data.Token,
            GuestPlayerName = "Concurrent Two"
        };

        // Act
        var responses = await Task.WhenAll(
            Client.PostAsJsonAsync("/api/games/scores", firstRequest),
            Client.PostAsJsonAsync("/api/games/scores", secondRequest));
        var leaderboardResponse = await Client.GetAsync("/api/games/leaderboards/2048");
        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<ApiResponse<GameLeaderboardDto>>(JsonOptions);

        // Assert
        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.OK);
        leaderboard!.Data!.Entries.Should().Contain(entry => entry.PlayerName == "Concurrent One");
        leaderboard.Data.Entries.Should().Contain(entry => entry.PlayerName == "Concurrent Two");
    }

    [Fact]
    public async Task SubmitScore_ShouldUseAuthenticatedAccountName_WhenSignedIn()
    {
        // Arrange
        var email = $"games-user-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email);
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var profile = await dbContext.UserProfiles
                .FirstOrDefaultAsync(item => item.UserId == userId);
            if (profile is null)
            {
                dbContext.UserProfiles.Add(new UserProfile
                {
                    UserId = userId,
                    ContactId = Guid.NewGuid(),
                    FullName = "Games Player",
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                profile.FullName = "Games Player";
            }

            await dbContext.SaveChangesAsync();
        }
        await AuthenticateAsync(email, "Test@Pass123");
        var request = new
        {
            GameId = "snake",
            RunId = Guid.NewGuid(),
            Score = 10,
            DurationSeconds = 30,
            Mode = (string?)null,
            GuestPlayerId = (string?)null,
            GuestToken = (string?)null,
            GuestPlayerName = (string?)null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/games/scores", request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<SubmitGameScoreResultDto>>(JsonOptions);
        var leaderboardResponse = await Client.GetAsync("/api/games/leaderboards/snake");
        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<ApiResponse<GameLeaderboardDto>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        envelope!.Data!.Rank.Should().BeGreaterThan(0);
        leaderboard!.Data!.Entries.Should().Contain(entry => entry.PlayerName == "Games Player");
    }

    [Fact]
    public async Task SubmitScore_ShouldReturnBadRequest_WhenGameIdUnsupported()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var sessionResponse = await Client.PostAsync("/api/games/guest-session", content: null);
        var sessionEnvelope = await sessionResponse.Content.ReadFromJsonAsync<ApiResponse<GuestIdentityToken>>(JsonOptions);
        var request = new
        {
            GameId = "unknown-game",
            RunId = Guid.NewGuid(),
            Score = 10,
            DurationSeconds = 10,
            Mode = (string?)null,
            GuestPlayerId = sessionEnvelope!.Data!.PlayerId.ToString("D"),
            GuestToken = sessionEnvelope.Data.Token,
            GuestPlayerName = "Test Guest"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/games/scores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    #endregion
}
