using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Configuration;

namespace ThaiX.Application.Common.Services.Scoring;

/// <summary>
/// Derives liquidity penalty from VnDirect average-volume fields (composite score additive term, typically negative).
/// </summary>
public sealed class LiquidityPenaltyEvaluator : ILiquidityPenaltyEvaluator
{
    private readonly IOptions<LiquidityScoringOptions> _options;

    public LiquidityPenaltyEvaluator(IOptions<LiquidityScoringOptions> options)
    {
        _options = options;
    }

    public int Evaluate(StockScoringContext context)
    {
        var opt = _options.Value;
        if (!opt.Enabled)
        {
            return 0;
        }

        var effectiveVolume = Math.Max(context.TotalVolumeAvgCr20D, context.NmVolumeAvgCr20D);
        var basePenalty =
            effectiveVolume >= opt.AdequateVolumeAvgThreshold ? opt.PenaltyAdequateOrBetter :
            effectiveVolume >= opt.ModerateVolumeAvgThreshold ? opt.PenaltyBetweenModerateAndAdequate :
            effectiveVolume >= opt.ThinVolumeAvgThreshold ? opt.PenaltyBetweenThinAndModerate :
            effectiveVolume > 0m ? opt.PenaltyBelowThinButPositive :
            opt.PenaltyNoVolumeSignal;

        var extra = 0;
        var relThreshold = opt.RelativeNmVolumePoorPctThreshold;
        if (relThreshold > 0m &&
            context.NmVolNmVolAvg20DPctCr > 0m &&
            context.NmVolNmVolAvg20DPctCr < relThreshold &&
            effectiveVolume >= opt.ModerateVolumeAvgThreshold)
        {
            extra += opt.RelativeNmVolumePoorExtraPenalty;
        }

        var total = basePenalty + extra;
        return total < opt.MaxLiquidityPenalty ? opt.MaxLiquidityPenalty : total;
    }
}
