using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Blog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Category '{request.Id}' not found.");

        var normalizedSlug = request.Slug.Trim().Trim('/').ToLowerInvariant();
        var slugTaken = await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Slug == normalizedSlug && c.Id != request.Id, cancellationToken);
        if (slugTaken)
        {
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"A category with slug '{normalizedSlug}' already exists.");
        }

        category.Update(request.Name, request.Slug, request.Description, request.Icon, request.Color);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
