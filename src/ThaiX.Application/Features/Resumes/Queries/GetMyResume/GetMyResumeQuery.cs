using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Resumes.Queries.GetMyResume;

/// <summary>
/// Returns the caller's own resume (or null if they haven't created one yet) for the admin editor.
/// Not cached — this is low-traffic, admin-only, and should always reflect the latest draft.
/// </summary>
public sealed record GetMyResumeQuery : IAppQuery<ResumeDto?>;
