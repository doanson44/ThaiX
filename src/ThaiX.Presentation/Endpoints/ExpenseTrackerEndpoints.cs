using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateBudget;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseCategory;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateRecurringTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateSavingGoal;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransactionTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransfer;
using ThaiX.Application.Features.ExpenseTracker.Commands.CreateWallet;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteBudget;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseCategory;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteRecurringTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteSavingGoal;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransactionTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransfer;
using ThaiX.Application.Features.ExpenseTracker.Commands.DeleteWallet;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateBudget;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseCategory;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateRecurringTransaction;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateSavingGoal;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransactionTag;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransfer;
using ThaiX.Application.Features.ExpenseTracker.Commands.UpdateWallet;
using ThaiX.Application.Features.ExpenseTracker.Models;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseDashboard;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferList;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletDetail;
using ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletList;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class ExpenseTrackerEndpoints
{
    public static void MapExpenseTrackerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapWalletEndpoints(endpoints);
        MapCategoryEndpoints(endpoints);
        MapTransactionEndpoints(endpoints);
        MapTransferEndpoints(endpoints);
        MapBudgetEndpoints(endpoints);
        MapSavingGoalEndpoints(endpoints);
        MapTagEndpoints(endpoints);
        MapTransactionTagEndpoints(endpoints);
        MapRecurringTransactionEndpoints(endpoints);
        MapDashboardEndpoints(endpoints);
    }

    private static void MapWalletEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/wallets").WithTags("Expense Tracker - Wallets");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetWalletListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.WalletRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetWalletDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseWalletDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.WalletRead);

        group.MapPost("/", async ([FromBody] CreateWalletCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/wallets/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.WalletWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateWalletCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.WalletWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteWalletCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.WalletDelete);
    }

    private static void MapCategoryEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/categories").WithTags("Expense Tracker - Categories");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseCategoryListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseCategoryRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseCategoryDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseCategoryDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseCategoryRead);

        group.MapPost("/", async ([FromBody] CreateExpenseCategoryCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/categories/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseCategoryWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateExpenseCategoryCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseCategoryWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteExpenseCategoryCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseCategoryDelete);
    }

    private static void MapTransactionEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/transactions").WithTags("Expense Tracker - Transactions");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseTransactionListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseTransactionDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseTransactionDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionRead);

        group.MapPost("/", async ([FromBody] CreateExpenseTransactionCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/transactions/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateExpenseTransactionCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteExpenseTransactionCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionDelete);
    }

    private static void MapTransferEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/transfers").WithTags("Expense Tracker - Transfers");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTransferListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransferRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTransferDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseTransferDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransferRead);

        group.MapPost("/", async ([FromBody] CreateTransferCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/transfers/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransferWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTransferCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransferWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteTransferCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransferDelete);
    }

    private static void MapBudgetEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/budgets").WithTags("Expense Tracker - Budgets");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetBudgetListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.BudgetRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetBudgetDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseBudgetDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.BudgetRead);

        group.MapPost("/", async ([FromBody] CreateBudgetCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/budgets/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.BudgetWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateBudgetCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.BudgetWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteBudgetCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.BudgetDelete);
    }

    private static void MapSavingGoalEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/saving-goals").WithTags("Expense Tracker - Saving Goals");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSavingGoalListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.SavingGoalRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSavingGoalDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseSavingGoalDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.SavingGoalRead);

        group.MapPost("/", async ([FromBody] CreateSavingGoalCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/saving-goals/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.SavingGoalWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateSavingGoalCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.SavingGoalWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteSavingGoalCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.SavingGoalDelete);
    }

    private static void MapTagEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/tags").WithTags("Expense Tracker - Tags");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseTagListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTagRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseTagDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseTagDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTagRead);

        group.MapPost("/", async ([FromBody] CreateExpenseTagCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/tags/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTagWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateExpenseTagCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTagWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteExpenseTagCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTagDelete);
    }

    private static void MapTransactionTagEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/transaction-tags").WithTags("Expense Tracker - Transaction Tags");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTransactionTagListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransactionTagRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTransactionTagDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseTransactionTagDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransactionTagRead);

        group.MapPost("/", async ([FromBody] CreateTransactionTagCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/transaction-tags/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransactionTagWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTransactionTagCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransactionTagWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteTransactionTagCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.TransactionTagDelete);
    }

    private static void MapRecurringTransactionEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/recurring-transactions").WithTags("Expense Tracker - Recurring Transactions");

        group.MapGet("/", async ([AsParameters] PagedQueryParameters p, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRecurringTransactionListQuery
            {
                PageNumber = p.PageNumber ?? PagedQueryParameters.DefaultPageNumber,
                PageSize = p.PageSize ?? PagedQueryParameters.DefaultPageSize,
                SortBy = p.SortBy,
                SortDescending = p.SortDescending ?? false,
                Search = p.SearchTerm
            }, ct);
            return Results.Ok(result.ToPagedApiResponse(ctx.GetCorrelationId()));
        }).RequireAuthorization(Domain.Common.Constants.Permissions.RecurringTransactionRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRecurringTransactionDetailQuery { Id = id }, ct);
            var response = ApiResponse<ExpenseRecurringTransactionDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.RecurringTransactionRead);

        group.MapPost("/", async ([FromBody] CreateRecurringTransactionCommand command, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Created($"/api/expense-tracker/recurring-transactions/{id}", response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.RecurringTransactionWrite);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateRecurringTransactionCommand body, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(body with { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.RecurringTransactionWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteRecurringTransactionCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.RecurringTransactionDelete);
    }

    private static void MapDashboardEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/expense-tracker/dashboard").WithTags("Expense Tracker - Dashboard");

        group.MapGet("/", async (IMediator mediator, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetExpenseDashboardQuery(), ct);
            var response = ApiResponse<ExpenseDashboardDataDto>.SuccessResult(result);
            response.Metadata.CorrelationId = ctx.GetCorrelationId();
            return Results.Ok(response);
        }).RequireAuthorization(Domain.Common.Constants.Permissions.ExpenseTransactionRead);
    }
}

public sealed record PagedQueryParameters
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(DefaultPageNumber)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(DefaultPageSize)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(false)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}
