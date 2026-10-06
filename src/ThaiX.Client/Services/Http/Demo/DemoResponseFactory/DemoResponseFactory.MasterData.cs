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
    private async Task<HttpResponseMessage> HandleMasterDataAsync(
        string method,
        string path,
        string query,
        int pageNumber,
        int pageSize,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resource = segments.Length >= 3 ? segments[2].ToLowerInvariant() : string.Empty;
        Guid entityId = default;
        var hasId = segments.Length >= 4 && Guid.TryParse(segments[3], out entityId);
        var action = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;
        var parentCode = GetQueryValue(query, "parentCode");
        var searchTerm = GetSearchTerm(query);

        if (resource == "symbols" || action == "search" || path.Contains("symbols/search", StringComparison.OrdinalIgnoreCase))
        {
            var assetType = GetQueryValue(query, "assetType") ?? "VnStock";
            var search = GetSearchTerm(query);
            List<SymbolSearchItemDto> all = assetType.Contains("Crypto", StringComparison.OrdinalIgnoreCase)
                ?
                [
                    new SymbolSearchItemDto { Symbol = "BTC", DisplayText = "BTC - Bitcoin" },
                    new SymbolSearchItemDto { Symbol = "ETH", DisplayText = "ETH - Ethereum" },
                    new SymbolSearchItemDto { Symbol = "BNB", DisplayText = "BNB - BNB" },
                    new SymbolSearchItemDto { Symbol = "SOL", DisplayText = "SOL - Solana" },
                    new SymbolSearchItemDto { Symbol = "XRP", DisplayText = "XRP - XRP" }
                ]
                :
                [
                    new SymbolSearchItemDto { Symbol = "VNM", DisplayText = "VNM - Vinamilk" },
                    new SymbolSearchItemDto { Symbol = "FPT", DisplayText = "FPT - FPT Corp" },
                    new SymbolSearchItemDto { Symbol = "HPG", DisplayText = "HPG - Hoa Phat" },
                    new SymbolSearchItemDto { Symbol = "VCB", DisplayText = "VCB - Vietcombank" },
                    new SymbolSearchItemDto { Symbol = "MWG", DisplayText = "MWG - Mobile World" }
                ];

            IEnumerable<SymbolSearchItemDto> filtered = all;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = all.Where(x =>
                    x.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.DisplayText.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var items = filtered.ToList();
            return DemoEnvelope.SuccessData(new SymbolSearchResultDto
            {
                Items = items,
                TotalCount = items.Count
            });
        }

        if (action == "import")
        {
            return DemoEnvelope.SuccessData(new ImportResultDto
            {
                TotalRows = 1,
                InsertedCount = 1,
                UpdatedCount = 0,
                SkippedCount = 0,
                Errors = []
            });
        }

        return resource switch
        {
            "countries" => await HandleCountryCrudAsync(method, hasId, entityId, pageNumber, pageSize, searchTerm, request, cancellationToken),
            "cities" => await HandleCityCrudAsync(method, hasId, entityId, pageNumber, pageSize, parentCode, searchTerm, request, cancellationToken),
            "districts" => await HandleDistrictCrudAsync(method, hasId, entityId, pageNumber, pageSize, parentCode, searchTerm, request, cancellationToken),
            "banks" => await HandleBankCrudAsync(method, hasId, entityId, pageNumber, pageSize, parentCode, searchTerm, request, cancellationToken),
            _ => method == "GET"
                ? DemoEnvelope.EmptyPaged(pageNumber, pageSize)
                : DemoEnvelope.SuccessData(Guid.NewGuid())
        };
    }

    private async Task<HttpResponseMessage> HandleCountryCrudAsync(
        string method, bool hasId, Guid entityId, int pageNumber, int pageSize,
        string? searchTerm,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                IEnumerable<CountryListItemDto> rows = _store.Countries;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    rows = rows.Where(x =>
                        x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "POST")
        {
            var body = await ReadJsonAsync<UpdateMasterDataRequest>(request, cancellationToken)
                       ?? new UpdateMasterDataRequest { Code = "XX", Name = "Demo" };
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Countries.Add(new CountryListItemDto
                {
                    Id = id,
                    Code = body.Code,
                    Name = body.Name,
                    DisplayText = $"{body.Code} - {body.Name}"
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method == "PUT" && hasId)
        {
            var body = await ReadJsonAsync<UpdateMasterDataRequest>(request, cancellationToken);
            _store.WithLock(() =>
            {
                var idx = _store.Countries.FindIndex(x => x.Id == entityId);
                if (idx >= 0 && body is not null)
                {
                    _store.Countries[idx] = _store.Countries[idx] with
                    {
                        Code = body.Code,
                        Name = body.Name,
                        DisplayText = $"{body.Code} - {body.Name}"
                    };
                }
            });
            return DemoEnvelope.Success();
        }

        if (method == "DELETE" && hasId)
        {
            _store.WithLock(() => _store.Countries.RemoveAll(x => x.Id == entityId));
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

    private async Task<HttpResponseMessage> HandleCityCrudAsync(
        string method, bool hasId, Guid entityId, int pageNumber, int pageSize,
        string? parentCode, string? searchTerm,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                IEnumerable<CityListItemDto> rows = _store.Cities;
                if (!string.IsNullOrWhiteSpace(parentCode))
                    rows = rows.Where(x => x.CountryCode.Equals(parentCode, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    rows = rows.Where(x =>
                        x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "POST")
        {
            var body = await ReadJsonAsync<CreateCityRequest>(request, cancellationToken)
                       ?? new CreateCityRequest { Code = "CT", Name = "City", CountryCode = "VN" };
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Cities.Add(new CityListItemDto
                {
                    Id = id,
                    Code = body.Code,
                    Name = body.Name,
                    CountryCode = body.CountryCode,
                    CountryName = "Vietnam",
                    DisplayText = $"{body.Code} - {body.Name}"
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method == "PUT" && hasId)
        {
            var body = await ReadJsonAsync<UpdateChildMasterDataRequest>(request, cancellationToken);
            _store.WithLock(() =>
            {
                var idx = _store.Cities.FindIndex(x => x.Id == entityId);
                if (idx >= 0 && body is not null)
                {
                    _store.Cities[idx] = _store.Cities[idx] with
                    {
                        Code = body.Code,
                        Name = body.Name,
                        CountryCode = body.ParentCode,
                        DisplayText = $"{body.Code} - {body.Name}"
                    };
                }
            });
            return DemoEnvelope.Success();
        }

        if (method == "DELETE" && hasId)
        {
            _store.WithLock(() => _store.Cities.RemoveAll(x => x.Id == entityId));
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

    private async Task<HttpResponseMessage> HandleDistrictCrudAsync(
        string method, bool hasId, Guid entityId, int pageNumber, int pageSize,
        string? parentCode, string? searchTerm,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                IEnumerable<DistrictListItemDto> rows = _store.Districts;
                if (!string.IsNullOrWhiteSpace(parentCode))
                    rows = rows.Where(x => x.CityCode.Equals(parentCode, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    rows = rows.Where(x =>
                        x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "POST")
        {
            var body = await ReadJsonAsync<CreateDistrictRequest>(request, cancellationToken)
                       ?? new CreateDistrictRequest { Code = "D", Name = "District", CityCode = "HCM" };
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Districts.Add(new DistrictListItemDto
                {
                    Id = id,
                    Code = body.Code,
                    Name = body.Name,
                    CityCode = body.CityCode,
                    CityName = "Ho Chi Minh",
                    DisplayText = $"{body.Code} - {body.Name}"
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method is "PUT" or "DELETE")
        {
            if (method == "DELETE" && hasId)
            {
                _store.WithLock(() => _store.Districts.RemoveAll(x => x.Id == entityId));
            }

            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

    private async Task<HttpResponseMessage> HandleBankCrudAsync(
        string method, bool hasId, Guid entityId, int pageNumber, int pageSize,
        string? parentCode, string? searchTerm,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                IEnumerable<BankListItemDto> rows = _store.Banks;
                if (!string.IsNullOrWhiteSpace(parentCode))
                    rows = rows.Where(x => x.CountryCode.Equals(parentCode, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    rows = rows.Where(x =>
                        x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "POST")
        {
            var body = await ReadJsonAsync<CreateBankRequest>(request, cancellationToken)
                       ?? new CreateBankRequest { Code = "BNK", Name = "Bank", CountryCode = "VN" };
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Banks.Add(new BankListItemDto
                {
                    Id = id,
                    Code = body.Code,
                    Name = body.Name,
                    CountryCode = body.CountryCode,
                    CountryName = "Vietnam",
                    DisplayText = $"{body.Code} - {body.Name}"
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method is "PUT" or "DELETE")
        {
            if (method == "DELETE" && hasId)
            {
                _store.WithLock(() => _store.Banks.RemoveAll(x => x.Id == entityId));
            }

            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

}
