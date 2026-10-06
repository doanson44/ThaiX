using ThaiX.Client.Models.Resumes;

namespace ThaiX.Client.Services.Resumes;

public interface IResumeService
{
    /// <summary>Returns the caller's own resume, or null if they haven't created one yet.</summary>
    Task<ResumeDto?> GetMineAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the caller's resume. Returns the resume Id.</summary>
    Task<Guid> SaveAsync(UpsertResumeRequest request, CancellationToken cancellationToken = default);

    /// <summary>Public, anonymous lookup by slug. Returns null if not found or not published.</summary>
    Task<ResumeDto?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Rewrites a single field's text for clarity/professionalism via AI.</summary>
    Task<string> PolishTextAsync(PolishResumeTextRequest request, CancellationToken cancellationToken = default);

    /// <summary>Synthesizes a professional summary from the rest of the resume via AI.</summary>
    Task<string> GenerateSummaryAsync(GenerateResumeSummaryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Advisory report on which skills/keywords to emphasize for a given job description.</summary>
    Task<string> OptimizeForJobAsync(OptimizeResumeForJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>Advisory report on spelling/grammar/tense-consistency issues across the resume.</summary>
    Task<string> CheckConsistencyAsync(CheckResumeConsistencyRequest request, CancellationToken cancellationToken = default);
}
