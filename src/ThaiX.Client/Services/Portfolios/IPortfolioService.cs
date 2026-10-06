using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Portfolios;

namespace ThaiX.Client.Services.Portfolios;

public interface IPortfolioService
{
    Task<PagedApiResponse<PortfolioListItemDto>> GetListAsync(
        PortfoliosListRequest request,
        CancellationToken cancellationToken = default);

    Task<PortfolioDetailDto> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PortfolioExportFileResult> GetImportTemplateAsync(CancellationToken cancellationToken = default);

    Task<PortfolioImportResultDto> ImportAsync(
        Stream csvStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<PortfolioExportFileResult> ExportAsync(
        PortfolioExportRequest request,
        CancellationToken cancellationToken = default);
}
