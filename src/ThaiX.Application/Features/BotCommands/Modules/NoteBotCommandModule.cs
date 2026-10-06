using MediatR;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.Notes.Commands.CreateNote;
using ThaiX.Application.Features.Notes.Queries.GetNotes;
using DomainPermissions = ThaiX.Domain.Common.Constants.Permissions;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class NoteBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "note",
        Aliases = ["n"],
        Syntax = "/note <content> | /note list",
        Description = "Create a quick note or list recent notes.",
        Category = BotCommandCategory.System,
        RequiredPermissions = [DomainPermissions.NoteWrite, DomainPermissions.NoteRead],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public NoteBotCommandModule(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        if (context.ParsedCommand.Arguments.Count == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Usage: /note <content> | /note list",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        // /note list → list notes
        if (string.Equals(context.ParsedCommand.Arguments[0], "list", StringComparison.OrdinalIgnoreCase))
        {
            return await ListNotesAsync(cancellationToken);
        }

        return await CreateNoteAsync(context, cancellationToken);
    }

    private async Task<BotCommandExecutionDto> CreateNoteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        var content = context.ParsedCommand.RawText;
        // Strip the command prefix (e.g. "/note " or "/n ")
        var slashIndex = content.IndexOf(' ');
        if (slashIndex > 0)
        {
            content = content[(slashIndex + 1)..].Trim();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "Note content is required. Usage: /note <content>",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        // Auto-generate title from first line or first ~80 chars
        var title = content.Length <= 80 ? content : content[..80] + "...";

        var id = await _mediator.Send(new CreateNoteCommand
        {
            Title = title,
            Content = content,
            Color = Domain.Aggregates.Notes.NoteColor.Default
        }, cancellationToken);

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = $"Note created (id: {id})."
        };
    }

    private async Task<BotCommandExecutionDto> ListNotesAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "User not authenticated. Notes are user-specific.",
                ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS
            };
        }

        var result = await _mediator.Send(new GetNotesQuery
        {
            OwnerId = _currentUser.UserId,
            PageNumber = 1,
            PageSize = 20
        }, cancellationToken);

        if (result.TotalCount == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "No notes found."
            };
        }

        var rows = result.Items
            .Select((note, i) => (IReadOnlyList<string>)new[]
            {
                (i + 1).ToString(),
                note.Title.Length <= 50 ? note.Title : note.Title[..47] + "...",
                note.CreatedAt.ToString("yyyy-MM-dd")
            })
            .ToList();

        var plainText = $"Notes ({result.TotalCount} total):"
            + "\n" + string.Join("\n",
                rows.Select(r => $"{r[0]}. [{r[2]}] {r[1]}"));

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Notes",
                Title = $"{result.TotalCount} Notes",
                Type = NotificationCardType.Summary,
                Metrics = result.Items
                    .Select(n => new NotificationMetric { Label = $"[{n.CreatedAt:yyyy-MM-dd}]", Value = n.Title.Length <= 60 ? n.Title : n.Title[..57] + "..." })
                    .ToList()
            }
        };
    }
}
