using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Features.Blog.Categories.Commands.CreateCategory;
using ThaiX.Application.Features.Blog.Categories.Commands.DeleteCategory;
using ThaiX.Application.Features.Blog.Categories.Commands.UpdateCategory;
using ThaiX.Application.Features.Blog.Categories.Queries.GetCategories;
using ThaiX.Application.Features.Blog.Tags.Commands.CreateTag;
using ThaiX.Application.Features.Blog.Tags.Commands.DeleteTag;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.UnitTests.Features.Blog;

public sealed class CategoryTagHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task CreateCategory_ShouldPersistAndReturnId()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var handler = new CreateCategoryCommandHandler(context);

        var id = await handler.Handle(new CreateCategoryCommand
        {
            Name = "Markets",
            Slug = "markets",
            Description = "Market news",
            Icon = null,
            Color = null
        }, CancellationToken.None);

        var saved = await context.Set<Category>().FirstAsync(c => c.Id == id, CancellationToken.None);
        saved.Name.Should().Be("Markets");
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ShouldThrow()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        context.Set<Category>().Add(Category.Create("Existing", "dup-slug", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCategoryCommandHandler(context);

        await handler.Invoking(h => h.Handle(new CreateCategoryCommand
        {
            Name = "New",
            Slug = "dup-slug"
        }, CancellationToken.None))
            .Should().ThrowAsync<OperationFailedException>();
    }

    [Fact]
    public async Task UpdateCategory_ShouldModifyFields()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var category = Category.Create("Old Name", "old-slug", null, null, null);
        context.Set<Category>().Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCategoryCommandHandler(context);
        await handler.Handle(new UpdateCategoryCommand
        {
            Id = category.Id,
            Name = "New Name",
            Slug = "new-slug"
        }, CancellationToken.None);

        var updated = await context.Set<Category>().FirstAsync(c => c.Id == category.Id, CancellationToken.None);
        updated.Name.Should().Be("New Name");
        updated.Slug.Should().Be("new-slug");
    }

    [Fact]
    public async Task DeleteCategory_ShouldSoftDelete()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var category = Category.Create("ToDelete", "to-delete", null, null, null);
        context.Set<Category>().Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCategoryCommandHandler(context);
        await handler.Handle(new DeleteCategoryCommand { Id = category.Id }, CancellationToken.None);

        var deleted = await context.Set<Category>().IgnoreQueryFilters().FirstAsync(c => c.Id == category.Id, CancellationToken.None);
        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetCategories_ShouldExcludeDeletedAndOrderByName()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var zebra = Category.Create("Zebra", "zebra", null, null, null);
        var alpha = Category.Create("Alpha", "alpha", null, null, null);
        var deleted = Category.Create("Deleted", "deleted-cat", null, null, null);
        deleted.SoftDelete();
        context.Set<Category>().AddRange(zebra, alpha, deleted);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCategoriesQueryHandler(context);
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().ContainInOrder("Alpha", "Zebra");
    }

    [Fact]
    public async Task CreateTag_WhenNameAlreadyExists_ShouldReturnExistingId()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var existing = Tag.Create("Finance", "finance");
        context.Set<Tag>().Add(existing);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateTagCommandHandler(context);
        var id = await handler.Handle(new CreateTagCommand { Name = "Finance" }, CancellationToken.None);

        id.Should().Be(existing.Id);
        var count = await context.Set<Tag>().CountAsync(CancellationToken.None);
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateTag_WhenNew_ShouldCreateWithDerivedSlug()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var handler = new CreateTagCommandHandler(context);

        var id = await handler.Handle(new CreateTagCommand { Name = "Machine Learning" }, CancellationToken.None);

        var saved = await context.Set<Tag>().FirstAsync(t => t.Id == id, CancellationToken.None);
        saved.Slug.Should().Be("machine-learning");
    }

    [Fact]
    public async Task DeleteTag_WhenUsingInMemoryProvider_ShouldReportUnsupportedBulkDelete()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var tag = Tag.Create("Obsolete", "obsolete");
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-tagtest", "summary", "content", null, null, null);
        context.Set<Tag>().Add(tag);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);
        context.Set<PostTag>().Add(PostTag.Create(post.Id, tag.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteTagCommandHandler(context);
        await handler.Invoking(h => h.Handle(new DeleteTagCommand { Id = tag.Id }, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ExecuteDelete*not supported*");
    }
}
