using FluentAssertions;
using ThaiX.Infrastructure.ExternalApis.Parsing;

namespace ThaiX.Presentation.IntegrationTests.Unit;

public sealed class TcbsPotentialStocksParserTests
{
    private const string PageHtml = """
        <html><body>
        <div class="card"><a href="https://static.tcbs.com.vn/oneclick/BVH.pdf">BVH</a></div>
        <div class="card"><a href="https://static.tcbs.com.vn/oneclick/CSM.pdf">CSM</a></div>
        <div class="card"><a href="https://static.tcbs.com.vn/oneclick/BVH.pdf">BVH again</a></div>
        <a href="https://www.tcbs.com.vn/ca-nhan/">not a ticker link</a>
        <img src="https://static.tcbs.com.vn/oneclick/logo.png" />
        </body></html>
        """;

    [Fact]
    public void Parse_ExtractsDistinctTickersWithTheirPdfUrl()
    {
        var result = TcbsPotentialStocksParser.Parse(PageHtml);

        result.Select(x => x.Ticker).Should().Equal("BVH", "CSM");
        result.Should().OnlyContain(x => x.Url.StartsWith("https://static.tcbs.com.vn/oneclick/"));
    }

    [Fact]
    public void Parse_ReturnsEmpty_WhenNoTickerLinkExists()
    {
        TcbsPotentialStocksParser.Parse("<html><body>Just a moment...</body></html>").Should().BeEmpty();
        TcbsPotentialStocksParser.Parse(null).Should().BeEmpty();
    }
}
