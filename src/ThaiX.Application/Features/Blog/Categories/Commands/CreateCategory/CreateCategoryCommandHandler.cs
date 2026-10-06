using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().Trim('/').ToLowerInvariant();
        var slugTaken = await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Slug == normalizedSlug, cancellationToken);
        if (slugTaken)
        {
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"A category with slug '{normalizedSlug}' already exists.");
        }

        var category = Category.Create(request.Name, request.Slug, request.Description, request.Icon, request.Color);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
