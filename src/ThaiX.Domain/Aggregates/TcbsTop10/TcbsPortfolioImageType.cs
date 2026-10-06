namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>Type of image attached to a TCBS Top 10 portfolio update.</summary>
public enum TcbsPortfolioImageType
{
    /// <summary>Chi tiet danh muc tu ngay...</summary>
    PortfolioDetail = 0,

    /// <summary>Ket qua danh muc tu ngay... (period performance)</summary>
    PeriodPerformance = 1,

    /// <summary>Hieu suat %YTD hoac ke tu khi thanh lap.</summary>
    YtdPerformance = 2
}
