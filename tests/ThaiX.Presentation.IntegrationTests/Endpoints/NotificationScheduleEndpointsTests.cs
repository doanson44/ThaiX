using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationScheduleEndpointsTests : IntegrationTestBase
{
    public NotificationScheduleEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private async Task<PagedApiResponse<NotificationScheduleListItemDto>> GetSchedulesAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        var query = $"/api/notification-schedules?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await Client.GetAsync(query);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<NotificationScheduleListItemDto>>(JsonOptions);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        return envelope;
    }

    private async Task<NotificationScheduleDto> CreateScheduleAsync(
        string name = "Test Daily Report",
        string type = "EveryXDays",
        int? intervalDays = 1,
        string? dayOfWeek = null,
        int? dayOfMonth = null,
        string monthlyOverflowPolicy = "SkipMonth",
        string? oneTimeAtLocal = null)
    {
        var request = new
        {
            Name = name,
            TemplateKey = "daily_report",
            Subject = "Daily Report",
            Body = "Here is your daily report.",
            Type = type,
            IntervalDays = intervalDays,
            DayOfWeek = dayOfWeek,
            DayOfMonth = dayOfMonth,
            MonthlyOverflowPolicy = monthlyOverflowPolicy,
            MisfirePolicy = "Skip",
            CatchUpLimit = (int?)null,
            TimeZoneId = "Asia/Ho_Chi_Minh",
            ExecuteTimeLocal = "08:00:00",
            OneTimeAtLocal = oneTimeAtLocal,
            StartDateLocal = (string?)null,
            EndAtUtc = (string?)null,
            MaxConsecutiveFailures = 3
        };

        var response = await Client.PostAsJsonAsync("/api/notification-schedules", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<NotificationScheduleDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        return envelope.Data!;
    }

    #region GET List

    [Fact]
    public async Task GetSchedules_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync("/api/notification-schedules");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSchedules_WhenAuthenticated_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var response = await Client.GetAsync("/api/notification-schedules?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSchedules_WithSearch_ShouldReturnFiltered()
    {
        await CreateAndAuthenticateAdminAsync();
        await CreateScheduleAsync(name: "Special Daily");

        var response = await Client.GetAsync("/api/notification-schedules?search=Special&page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Special Daily");
    }

    #endregion

    #region GET ById

    [Fact]
    public async Task GetScheduleById_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/notification-schedules/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetScheduleById_WithValidId_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "By Name");

        var response = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        body.Should().Contain("By Name");
    }

    [Fact]
    public async Task GetScheduleById_WithUnknownId_ShouldReturnNotFound()
    {
        await CreateAndAuthenticateAdminAsync();
        var response = await Client.GetAsync($"/api/notification-schedules/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Create

    [Fact]
    public async Task CreateSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new { Name = "X", TemplateKey = "tk", Subject = "S", Body = "B", Type = "EveryXDays", IntervalDays = (int?)1, TimeZoneId = "Asia/Ho_Chi_Minh", ExecuteTimeLocal = "08:00:00", MisfirePolicy = "Skip", MonthlyOverflowPolicy = "SkipMonth" };
        var response = await Client.PostAsJsonAsync("/api/notification-schedules", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSchedule_EveryXDays_ShouldReturnCreated()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "Every 2 Days", intervalDays: 2);
        schedule.Name.Should().Be("Every 2 Days");
        schedule.NextExecuteAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSchedule_Weekly_ShouldReturnCreated()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "Weekly Mon", type: "Weekly", dayOfWeek: "Monday");
        schedule.Name.Should().Be("Weekly Mon");
    }

    [Fact]
    public async Task CreateSchedule_Monthly_ShouldReturnCreated()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "Monthly 15", type: "Monthly", dayOfMonth: 15, monthlyOverflowPolicy: "RunOnLastDay");
        schedule.Name.Should().Be("Monthly 15");
    }

    [Fact]
    public async Task CreateSchedule_OneTime_ShouldReturnCreated()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "OneTime", type: "OneTime", oneTimeAtLocal: DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss"));
        schedule.Name.Should().Be("OneTime");
    }

    [Fact]
    public async Task CreateSchedule_AfterListCachePrimed_ShouldReturnFreshList()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var scheduleName = $"cache-create-{Guid.NewGuid():N}";
        var initialList = await GetSchedulesAsync(search: scheduleName);
        initialList.Data.Should().BeEmpty();

        // Act
        await CreateScheduleAsync(name: scheduleName);
        var refreshedList = await GetSchedulesAsync(search: scheduleName);

        // Assert
        refreshedList.Data.Should().ContainSingle(x => x.Name == scheduleName);
    }

    #endregion

    #region PUT Update

    [Fact]
    public async Task UpdateSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new { Id = Guid.NewGuid(), Name = "X", TemplateKey = "tk", Subject = "S", Body = "B", Type = "EveryXDays", IntervalDays = (int?)1, ExecuteTimeLocal = "08:00:00", MisfirePolicy = "Skip", MonthlyOverflowPolicy = "SkipMonth" };
        var response = await Client.PutAsJsonAsync($"/api/notification-schedules/{Guid.NewGuid()}", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSchedule_WithValidData_ShouldReturnNoContent()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: "Original");

        var request = new
        {
            Id = schedule.Id,
            Name = "Updated Name",
            TemplateKey = schedule.TemplateKey,
            Subject = "Updated",
            Body = "Updated",
            Type = "EveryXDays",
            IntervalDays = (int?)3,
            ExecuteTimeLocal = "10:00:00",
            MisfirePolicy = "Skip",
            MonthlyOverflowPolicy = "SkipMonth"
        };

        var response = await Client.PutAsJsonAsync($"/api/notification-schedules/{schedule.Id}", request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSchedule_WithIdMismatch_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var request = new
        {
            Id = Guid.NewGuid(),
            Name = "Mismatch",
            TemplateKey = schedule.TemplateKey,
            Subject = "S",
            Body = "B",
            Type = "EveryXDays",
            IntervalDays = (int?)1,
            ExecuteTimeLocal = "08:00:00",
            MisfirePolicy = "Skip",
            MonthlyOverflowPolicy = "SkipMonth"
        };

        var response = await Client.PutAsJsonAsync($"/api/notification-schedules/{schedule.Id}", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSchedule_WithUnknownId_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        var unknownId = Guid.NewGuid();

        var request = new
        {
            Id = unknownId,
            Name = "Ghost",
            TemplateKey = "tk",
            Subject = "S",
            Body = "B",
            Type = "EveryXDays",
            IntervalDays = (int?)1,
            ExecuteTimeLocal = "08:00:00",
            MisfirePolicy = "Skip",
            MonthlyOverflowPolicy = "SkipMonth"
        };

        var response = await Client.PutAsJsonAsync($"/api/notification-schedules/{unknownId}", request);
        // Command handler throws InvalidOperationException -> middleware returns 400
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSchedule_AfterDetailCachePrimed_ShouldReturnFreshDetail()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: $"cache-update-{Guid.NewGuid():N}");

        var initialDetailResponse = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");
        initialDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initialDetail = await ReadResponseAsync<NotificationScheduleDto>(initialDetailResponse);
        initialDetail.Data.Should().NotBeNull();
        initialDetail.Data!.Name.Should().Be(schedule.Name);

        var updatedName = $"updated-{Guid.NewGuid():N}";
        var request = new
        {
            Id = schedule.Id,
            Name = updatedName,
            TemplateKey = schedule.TemplateKey,
            Subject = "Updated",
            Body = "Updated",
            Type = "EveryXDays",
            IntervalDays = (int?)3,
            ExecuteTimeLocal = "10:00:00",
            MisfirePolicy = "Skip",
            MonthlyOverflowPolicy = "SkipMonth"
        };

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/api/notification-schedules/{schedule.Id}", request);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshedDetailResponse = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");

        // Assert
        refreshedDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedDetail = await ReadResponseAsync<NotificationScheduleDto>(refreshedDetailResponse);
        refreshedDetail.Data.Should().NotBeNull();
        refreshedDetail.Data!.Name.Should().Be(updatedName);
    }

    #endregion

    #region DELETE

    [Fact]
    public async Task DeleteSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.DeleteAsync($"/api/notification-schedules/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteSchedule_WithValidId_ShouldReturnNoContent()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var response = await Client.DeleteAsync($"/api/notification-schedules/{schedule.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // After delete, GET should return NotFound (soft-deleted)
        var verify = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");
        verify.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSchedule_WithUnknownId_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        // Command handler throws InvalidOperationException -> middleware returns 400
        var response = await Client.DeleteAsync($"/api/notification-schedules/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSchedule_AfterDetailCachePrimed_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync(name: $"cache-delete-{Guid.NewGuid():N}");

        var initialDetailResponse = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");
        initialDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/notification-schedules/{schedule.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshedDetailResponse = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}");

        // Assert
        refreshedDetailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Activate

    [Fact]
    public async Task ActivateSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivateSchedule_WithValidId_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();
        await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/pause", null);

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateSchedule_WithUnknownId_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        // Command handler throws InvalidOperationException -> middleware returns 400
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pause

    [Fact]
    public async Task PauseSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/pause", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PauseSchedule_WithValidId_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/pause", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PauseSchedule_WithUnknownId_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        // Command handler throws InvalidOperationException -> middleware returns 400
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/pause", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Resume

    [Fact]
    public async Task ResumeSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/resume", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResumeSchedule_WithValidId_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();
        await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/pause", null);

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/resume", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Disable

    [Fact]
    public async Task DisableSchedule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/disable", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DisableSchedule_WithValidId_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/disable", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Run-Now

    [Fact]
    public async Task RunNow_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/run-now", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RunNow_WithValidId_ShouldReturnOkWithExecution()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/run-now", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ScheduleExecutionDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.ScheduleId.Should().Be(schedule.Id);
        envelope.Data.Status.Should().Be(ScheduleExecutionStatus.Succeeded);
    }

    [Fact]
    public async Task RunNow_WithUnknownId_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        // Command handler throws InvalidOperationException -> middleware returns 400
        var response = await Client.PostAsync($"/api/notification-schedules/{Guid.NewGuid()}/run-now", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RunNow_AfterDisable_ShouldStillWork()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();
        await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/disable", null);

        var response = await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/run-now", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ScheduleExecutionDto>(response);
        envelope.Data!.Status.Should().Be(ScheduleExecutionStatus.Succeeded);
    }

    #endregion

    #region Executions

    [Fact]
    public async Task GetExecutions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/notification-schedules/{Guid.NewGuid()}/executions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExecutions_AfterRunNow_ShouldContainExecution()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();
        await Client.PostAsync($"/api/notification-schedules/{schedule.Id}/run-now", null);

        var response = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}/executions?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<ScheduleExecutionDto>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data.Should().NotBeEmpty();
        envelope.Data.Should().Contain(e => e.Status == ScheduleExecutionStatus.Succeeded);
    }

    #endregion

    #region Upcoming

    [Fact]
    public async Task GetUpcoming_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync("/api/notification-schedules/upcoming?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUpcoming_WhenAuthenticated_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        await CreateScheduleAsync();

        var response = await Client.GetAsync("/api/notification-schedules/upcoming?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<UpcomingScheduleDto>>(response);
        envelope.Success.Should().BeTrue();
    }

    #endregion

    #region Preview

    [Fact]
    public async Task Preview_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/notification-schedules/{Guid.NewGuid()}/preview?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Preview_WithValidId_ShouldReturnDates()
    {
        await CreateAndAuthenticateAdminAsync();
        var schedule = await CreateScheduleAsync();

        var response = await Client.GetAsync($"/api/notification-schedules/{schedule.Id}/preview?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<DateTime>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Preview_WithUnknownId_ShouldReturnNotFound()
    {
        await CreateAndAuthenticateAdminAsync();
        // Query handler returns null -> endpoint returns 404
        var response = await Client.GetAsync($"/api/notification-schedules/{Guid.NewGuid()}/preview?count=5");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
