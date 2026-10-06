using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins;

internal static class JsonBinCodeResolver
{
    private const int MaxGenerateAttempts = 8;

    /// <summary>
    /// Uses the provided code when present; otherwise generates a unique category-prefixed code.
    /// </summary>
    public static async Task<string> ResolveUniqueAsync(
        IApplicationDbContext context,
        string? requestedCode,
        JsonBinCategories category,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var explicitCode = JsonBin.NormalizeCodeOrNull(requestedCode);
        if (explicitCode is not null)
        {
            await EnsureAvailableAsync(context, explicitCode, excludeId, cancellationToken);
            return explicitCode;
        }

        for (var attempt = 0; attempt < MaxGenerateAttempts; attempt++)
        {
            var generated = JsonBin.GenerateCode(category);
            var taken = await ExistsAsync(context, generated, excludeId, cancellationToken);
            if (!taken)
                return generated;
        }

        throw new OperationFailedException(
            ErrorCodes.OPERATION_NOT_ALLOWED,
            "Unable to generate a unique JsonBin code. Retry the request.");
    }

    private static async Task EnsureAvailableAsync(
        IApplicationDbContext context,
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        if (await ExistsAsync(context, code, excludeId, cancellationToken))
        {
            throw new OperationFailedException(
                ErrorCodes.DUPLICATE_ENTRY,
                $"JsonBin with code '{code}' already exists.");
        }
    }

    private static Task<bool> ExistsAsync(
        IApplicationDbContext context,
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var query = context.JsonBins.Where(x => x.Code == code);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return query.AnyAsync(cancellationToken);
    }
}
