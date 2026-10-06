namespace ThaiX.Application.Features.CredentialAccounts;

internal static class CredentialAccountLastUsedFormatter
{
    public static string Format(DateTime? usedAtUtc, DateTime utcNow)
    {
        if (!usedAtUtc.HasValue)
        {
            return string.Empty;
        }

        var elapsed = utcNow - usedAtUtc.Value;
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        if (elapsed.TotalMinutes < 60)
        {
            var minutes = Math.Max(1, (int)Math.Floor(elapsed.TotalMinutes));
            return $"{minutes} phút trước";
        }

        if (elapsed.TotalHours < 24)
        {
            var hours = Math.Max(1, (int)Math.Floor(elapsed.TotalHours));
            return $"{hours} giờ trước";
        }

        if (elapsed.TotalDays < 14)
        {
            var days = Math.Max(1, (int)Math.Floor(elapsed.TotalDays));
            return $"{days} ngày trước";
        }

        if (elapsed.TotalDays < 60)
        {
            var weeks = Math.Max(1, (int)Math.Floor(elapsed.TotalDays / 7));
            return $"{weeks} tuần trước";
        }

        if (elapsed.TotalDays < 365)
        {
            var months = Math.Max(1, (int)Math.Floor(elapsed.TotalDays / 30));
            return $"{months} tháng trước";
        }

        var years = Math.Max(1, (int)Math.Floor(elapsed.TotalDays / 365));
        return $"{years} năm trước";
    }
}
