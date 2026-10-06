using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private static async Task<HttpResponseMessage> HandleExpenseTrackerAsync(
        string method,
        string path,
        int pageNumber,
        int pageSize,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var seed = ExpenseTrackerDemoSeed.Instance;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resource = segments.Length >= 3 ? segments[2].ToLowerInvariant() : string.Empty;
        Guid entityId = default;
        var hasId = segments.Length >= 4 && Guid.TryParse(segments[3], out entityId);

        lock (seed.Sync)
        {
            if (method == "GET" && resource == "dashboard")
                return DemoEnvelope.SuccessData(seed.BuildDashboard());

            if (method == "GET")
            {
                return resource switch
                {
                    "wallets" => hasId
                        ? DemoEnvelope.SuccessData(seed.Wallets.FirstOrDefault(x => x.Id == entityId) ?? seed.Wallets[0])
                        : DemoEnvelope.Paged(seed.Wallets.ToList(), pageNumber, pageSize),
                    "categories" => hasId
                        ? DemoEnvelope.SuccessData(seed.Categories.FirstOrDefault(x => x.Id == entityId) ?? seed.Categories[0])
                        : DemoEnvelope.Paged(seed.Categories.ToList(), pageNumber, pageSize),
                    "transactions" => hasId
                        ? DemoEnvelope.SuccessData(seed.Transactions.FirstOrDefault(x => x.Id == entityId) ?? seed.Transactions[0])
                        : DemoEnvelope.Paged(seed.Transactions.ToList(), pageNumber, pageSize),
                    "transfers" => hasId
                        ? DemoEnvelope.SuccessData(seed.Transfers.FirstOrDefault(x => x.Id == entityId) ?? seed.Transfers[0])
                        : DemoEnvelope.Paged(seed.Transfers.ToList(), pageNumber, pageSize),
                    "budgets" => hasId
                        ? DemoEnvelope.SuccessData(seed.Budgets.FirstOrDefault(x => x.Id == entityId) ?? seed.Budgets[0])
                        : DemoEnvelope.Paged(seed.Budgets.ToList(), pageNumber, pageSize),
                    "saving-goals" => hasId
                        ? DemoEnvelope.SuccessData(seed.SavingGoals.FirstOrDefault(x => x.Id == entityId) ?? seed.SavingGoals[0])
                        : DemoEnvelope.Paged(seed.SavingGoals.ToList(), pageNumber, pageSize),
                    "tags" => hasId
                        ? DemoEnvelope.SuccessData(seed.Tags.FirstOrDefault(x => x.Id == entityId) ?? seed.Tags[0])
                        : DemoEnvelope.Paged(seed.Tags.ToList(), pageNumber, pageSize),
                    "transaction-tags" => hasId
                        ? DemoEnvelope.SuccessData(seed.TransactionTags.FirstOrDefault(x => x.Id == entityId) ?? seed.TransactionTags[0])
                        : DemoEnvelope.Paged(seed.TransactionTags.ToList(), pageNumber, pageSize),
                    "recurring-transactions" => hasId
                        ? DemoEnvelope.SuccessData(seed.RecurringTransactions.FirstOrDefault(x => x.Id == entityId) ?? seed.RecurringTransactions[0])
                        : DemoEnvelope.Paged(seed.RecurringTransactions.ToList(), pageNumber, pageSize),
                    _ => DemoEnvelope.EmptyPaged(pageNumber, pageSize)
                };
            }
        }

        if (method == "POST")
        {
            var id = Guid.NewGuid();
            switch (resource)
            {
                case "wallets":
                {
                    var body = await ReadJsonAsync<ExpenseWalletWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Wallets.Add(new ExpenseWalletDto
                        {
                            Id = id,
                            Name = body.Name,
                            WalletType = body.WalletType,
                            Currency = body.Currency,
                            CurrentBalance = body.InitialBalance
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "categories":
                {
                    var body = await ReadJsonAsync<ExpenseCategoryWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Categories.Add(new ExpenseCategoryDto
                        {
                            Id = id,
                            Name = body.Name,
                            CategoryType = body.CategoryType,
                            ParentCategoryId = body.ParentCategoryId
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "transactions":
                {
                    var body = await ReadJsonAsync<ExpenseTransactionWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Transactions.Add(new ExpenseTransactionDto
                        {
                            Id = id,
                            WalletId = body.WalletId,
                            CategoryId = body.CategoryId,
                            TransactionType = body.TransactionType,
                            Amount = body.Amount,
                            OccurredOn = body.OccurredOn,
                            Note = body.Note
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "transfers":
                {
                    var body = await ReadJsonAsync<ExpenseTransferWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Transfers.Add(new ExpenseTransferDto
                        {
                            Id = id,
                            SourceWalletId = body.SourceWalletId,
                            TargetWalletId = body.TargetWalletId,
                            Amount = body.Amount,
                            TransferredOn = body.TransferredOn,
                            Note = body.Note
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "budgets":
                {
                    var body = await ReadJsonAsync<ExpenseBudgetWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Budgets.Add(new ExpenseBudgetDto
                        {
                            Id = id,
                            CategoryId = body.CategoryId,
                            Period = body.Period,
                            Amount = body.Amount,
                            StartDate = body.StartDate,
                            EndDate = body.EndDate
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "saving-goals":
                {
                    var body = await ReadJsonAsync<ExpenseSavingGoalWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.SavingGoals.Add(new ExpenseSavingGoalDto
                        {
                            Id = id,
                            WalletId = body.WalletId,
                            Name = body.Name,
                            TargetAmount = body.TargetAmount,
                            CurrentAmount = body.CurrentAmount,
                            TargetDate = body.TargetDate
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "tags":
                {
                    var body = await ReadJsonAsync<ExpenseTagWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.Tags.Add(new ExpenseTagDto { Id = id, Name = body.Name, ColorHex = body.ColorHex });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "transaction-tags":
                {
                    var body = await ReadJsonAsync<ExpenseTransactionTagWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.TransactionTags.Add(new ExpenseTransactionTagDto
                        {
                            Id = id,
                            TransactionId = body.TransactionId,
                            TagId = body.TagId
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                case "recurring-transactions":
                {
                    var body = await ReadJsonAsync<ExpenseRecurringWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        seed.RecurringTransactions.Add(new ExpenseRecurringTransactionDto
                        {
                            Id = id,
                            WalletId = body.WalletId,
                            CategoryId = body.CategoryId,
                            TransactionType = body.TransactionType,
                            Amount = body.Amount,
                            Frequency = body.Frequency,
                            NextRun = body.NextRun,
                            EndDate = body.EndDate,
                            IsActive = true,
                            Note = body.Note
                        });
                    }
                    return DemoEnvelope.SuccessData(id);
                }
                default:
                    return DemoEnvelope.SuccessData(id);
            }
        }

        if (method is "PUT" or "PATCH")
        {
            if (!hasId)
                return DemoEnvelope.Success();

            switch (resource)
            {
                case "wallets":
                {
                    var body = await ReadJsonAsync<ExpenseWalletWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Wallets.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.Wallets[idx] = new ExpenseWalletDto
                            {
                                Id = entityId,
                                Name = body.Name,
                                WalletType = body.WalletType,
                                Currency = body.Currency,
                                CurrentBalance = seed.Wallets[idx].CurrentBalance
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "categories":
                {
                    var body = await ReadJsonAsync<ExpenseCategoryWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Categories.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.Categories[idx] = new ExpenseCategoryDto
                            {
                                Id = entityId,
                                Name = body.Name,
                                CategoryType = body.CategoryType,
                                ParentCategoryId = body.ParentCategoryId
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "transactions":
                {
                    var body = await ReadJsonAsync<ExpenseTransactionWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Transactions.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.Transactions[idx] = new ExpenseTransactionDto
                            {
                                Id = entityId,
                                WalletId = body.WalletId,
                                CategoryId = body.CategoryId,
                                TransactionType = body.TransactionType,
                                Amount = body.Amount,
                                OccurredOn = body.OccurredOn,
                                Note = body.Note
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "transfers":
                {
                    var body = await ReadJsonAsync<ExpenseTransferWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Transfers.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.Transfers[idx] = new ExpenseTransferDto
                            {
                                Id = entityId,
                                SourceWalletId = body.SourceWalletId,
                                TargetWalletId = body.TargetWalletId,
                                Amount = body.Amount,
                                TransferredOn = body.TransferredOn,
                                Note = body.Note
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "budgets":
                {
                    var body = await ReadJsonAsync<ExpenseBudgetWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Budgets.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.Budgets[idx] = new ExpenseBudgetDto
                            {
                                Id = entityId,
                                CategoryId = body.CategoryId,
                                Period = body.Period,
                                Amount = body.Amount,
                                StartDate = body.StartDate,
                                EndDate = body.EndDate
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "saving-goals":
                {
                    var body = await ReadJsonAsync<ExpenseSavingGoalWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.SavingGoals.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.SavingGoals[idx] = new ExpenseSavingGoalDto
                            {
                                Id = entityId,
                                WalletId = body.WalletId,
                                Name = body.Name,
                                TargetAmount = body.TargetAmount,
                                CurrentAmount = body.CurrentAmount,
                                TargetDate = body.TargetDate
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "tags":
                {
                    var body = await ReadJsonAsync<ExpenseTagWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.Tags.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                            seed.Tags[idx] = new ExpenseTagDto { Id = entityId, Name = body.Name, ColorHex = body.ColorHex };
                    }
                    return DemoEnvelope.Success();
                }
                case "transaction-tags":
                {
                    var body = await ReadJsonAsync<ExpenseTransactionTagWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.TransactionTags.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.TransactionTags[idx] = new ExpenseTransactionTagDto
                            {
                                Id = entityId,
                                TransactionId = body.TransactionId,
                                TagId = body.TagId
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                case "recurring-transactions":
                {
                    var body = await ReadJsonAsync<ExpenseRecurringWriteRequest>(request, cancellationToken) ?? new();
                    lock (seed.Sync)
                    {
                        var idx = seed.RecurringTransactions.FindIndex(x => x.Id == entityId);
                        if (idx >= 0)
                        {
                            seed.RecurringTransactions[idx] = new ExpenseRecurringTransactionDto
                            {
                                Id = entityId,
                                WalletId = body.WalletId,
                                CategoryId = body.CategoryId,
                                TransactionType = body.TransactionType,
                                Amount = body.Amount,
                                Frequency = body.Frequency,
                                NextRun = body.NextRun,
                                EndDate = body.EndDate,
                                IsActive = body.IsActive,
                                Note = body.Note
                            };
                        }
                    }
                    return DemoEnvelope.Success();
                }
                default:
                    return DemoEnvelope.Success();
            }
        }

        if (method == "DELETE" && hasId)
        {
            lock (seed.Sync)
            {
                switch (resource)
                {
                    case "wallets":
                        seed.Wallets.RemoveAll(x => x.Id == entityId);
                        break;
                    case "categories":
                        seed.Categories.RemoveAll(x => x.Id == entityId);
                        break;
                    case "transactions":
                        seed.Transactions.RemoveAll(x => x.Id == entityId);
                        break;
                    case "transfers":
                        seed.Transfers.RemoveAll(x => x.Id == entityId);
                        break;
                    case "budgets":
                        seed.Budgets.RemoveAll(x => x.Id == entityId);
                        break;
                    case "saving-goals":
                        seed.SavingGoals.RemoveAll(x => x.Id == entityId);
                        break;
                    case "tags":
                        seed.Tags.RemoveAll(x => x.Id == entityId);
                        break;
                    case "transaction-tags":
                        seed.TransactionTags.RemoveAll(x => x.Id == entityId);
                        break;
                    case "recurring-transactions":
                        seed.RecurringTransactions.RemoveAll(x => x.Id == entityId);
                        break;
                }
            }
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.SuccessData(new { demo = true, path });
    }

    /// <summary>
    /// Mutable in-memory demo catalog for Expense Tracker GET/POST/PUT/DELETE.
    /// </summary>
    private sealed class ExpenseTrackerDemoSeed
    {
        public static ExpenseTrackerDemoSeed Instance { get; } = Create();

        public object Sync { get; } = new();
        public required List<ExpenseWalletDto> Wallets { get; init; }
        public required List<ExpenseCategoryDto> Categories { get; init; }
        public required List<ExpenseTransactionDto> Transactions { get; init; }
        public required List<ExpenseTransferDto> Transfers { get; init; }
        public required List<ExpenseBudgetDto> Budgets { get; init; }
        public required List<ExpenseSavingGoalDto> SavingGoals { get; init; }
        public required List<ExpenseTagDto> Tags { get; init; }
        public required List<ExpenseTransactionTagDto> TransactionTags { get; init; }
        public required List<ExpenseRecurringTransactionDto> RecurringTransactions { get; init; }

        public ExpenseDashboardDataDto BuildDashboard()
        {
            var now = DateTime.UtcNow;
            var incomeToday = Transactions
                .Where(t => t.TransactionType == "Income" && t.OccurredOn.Date == now.Date)
                .Sum(t => t.Amount);
            var expenseToday = Transactions
                .Where(t => t.TransactionType == "Expense" && t.OccurredOn.Date == now.Date)
                .Sum(t => t.Amount);
            var incomeMonth = Transactions
                .Where(t => t.TransactionType == "Income" && t.OccurredOn.Year == now.Year && t.OccurredOn.Month == now.Month)
                .Sum(t => t.Amount);
            var expenseMonth = Transactions
                .Where(t => t.TransactionType == "Expense" && t.OccurredOn.Year == now.Year && t.OccurredOn.Month == now.Month)
                .Sum(t => t.Amount);

            return new ExpenseDashboardDataDto
            {
                Summary = new ExpenseDashboardSummaryDto
                {
                    TotalAsset = Wallets.Sum(x => x.CurrentBalance),
                    IncomeToday = incomeToday,
                    ExpenseToday = expenseToday,
                    IncomeThisMonth = incomeMonth,
                    ExpenseThisMonth = expenseMonth
                },
                CashFlow = incomeMonth - expenseMonth,
                ExpenseByCategory = Categories
                    .Where(c => c.CategoryType == "Expense")
                    .Select(c => new ExpenseCategoryAmountDto
                    {
                        CategoryName = c.Name,
                        Amount = Transactions.Where(t => t.CategoryId == c.Id).Sum(t => t.Amount)
                    })
                    .Where(x => x.Amount > 0)
                    .ToList(),
                IncomeByCategory = Categories
                    .Where(c => c.CategoryType == "Income")
                    .Select(c => new ExpenseCategoryAmountDto
                    {
                        CategoryName = c.Name,
                        Amount = Transactions.Where(t => t.CategoryId == c.Id).Sum(t => t.Amount)
                    })
                    .Where(x => x.Amount > 0)
                    .ToList(),
                RecentTransactions = Transactions.OrderByDescending(t => t.OccurredOn).Take(10).ToList(),
                MonthlyExpenseChart =
                [
                    new ExpenseMonthlySeriesDto { Year = now.Year, Month = now.Month, Amount = expenseMonth }
                ],
                MonthlyIncomeChart =
                [
                    new ExpenseMonthlySeriesDto { Year = now.Year, Month = now.Month, Amount = incomeMonth }
                ]
            };
        }

        private static ExpenseTrackerDemoSeed Create()
        {
            var cashId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var bankId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var foodId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var salaryId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var tx1 = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var tx2 = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var tagId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var now = DateTime.UtcNow;

            var wallets = new List<ExpenseWalletDto>
            {
                new() { Id = cashId, Name = "Cash", WalletType = "Cash", Currency = "VND", CurrentBalance = 2_500_000 },
                new() { Id = bankId, Name = "Vietcombank", WalletType = "Bank", Currency = "VND", CurrentBalance = 18_750_000 }
            };

            var categories = new List<ExpenseCategoryDto>
            {
                new() { Id = foodId, Name = "Food", CategoryType = "Expense" },
                new() { Id = salaryId, Name = "Salary", CategoryType = "Income" }
            };

            var transactions = new List<ExpenseTransactionDto>
            {
                new()
                {
                    Id = tx1, WalletId = cashId, CategoryId = foodId, TransactionType = "Expense",
                    Amount = 85_000, OccurredOn = now.Date.AddHours(12), Note = "Lunch"
                },
                new()
                {
                    Id = tx2, WalletId = bankId, CategoryId = salaryId, TransactionType = "Income",
                    Amount = 20_000_000, OccurredOn = now.Date.AddDays(-2), Note = "Payroll"
                }
            };

            var transfers = new List<ExpenseTransferDto>
            {
                new()
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    SourceWalletId = bankId, TargetWalletId = cashId, Amount = 500_000,
                    TransferredOn = now.Date.AddDays(-1), Note = "ATM cash"
                }
            };

            var budgets = new List<ExpenseBudgetDto>
            {
                new()
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    CategoryId = foodId, Period = "Monthly", Amount = 3_000_000,
                    StartDate = new DateTime(now.Year, now.Month, 1),
                    EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1)
                }
            };

            var savingGoals = new List<ExpenseSavingGoalDto>
            {
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    WalletId = bankId, Name = "Emergency fund", TargetAmount = 50_000_000,
                    CurrentAmount = 12_000_000, TargetDate = now.Date.AddMonths(6)
                }
            };

            var tags = new List<ExpenseTagDto>
            {
                new() { Id = tagId, Name = "Work", ColorHex = "#2563eb" }
            };

            var transactionTags = new List<ExpenseTransactionTagDto>
            {
                new() { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), TransactionId = tx1, TagId = tagId }
            };

            var recurring = new List<ExpenseRecurringTransactionDto>
            {
                new()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    WalletId = bankId, CategoryId = foodId, TransactionType = "Expense",
                    Amount = 200_000, Frequency = "Weekly", NextRun = now.Date.AddDays(3),
                    IsActive = true, Note = "Groceries"
                }
            };

            return new ExpenseTrackerDemoSeed
            {
                Wallets = wallets,
                Categories = categories,
                Transactions = transactions,
                Transfers = transfers,
                Budgets = budgets,
                SavingGoals = savingGoals,
                Tags = tags,
                TransactionTags = transactionTags,
                RecurringTransactions = recurring
            };
        }
    }


}
