using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPostById;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPostBySlug;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.UnitTests.Features.Blog;

public sealed class PostQueryHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task GetPostBySlug_WhenPostIsDraft_ShouldReturnNull()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Draft Post", "draft-post", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPostBySlugQueryHandler(context);
        var result = await handler.Handle(new GetPostBySlugQuery { Slug = "draft-post" }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPostBySlug_WhenPostIsPublished_ShouldReturnDtoWithCategoryAndTags()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var category = Category.Create("Tech", "tech", null, null, null);
        var tag = Tag.Create("AI", "ai");
        var post = Post.Create(Guid.NewGuid(), "Published Post", "published-post", "summary", "content", category.Id, null, null);
        post.Publish();

        context.Set<Category>().Add(category);
        context.Set<Tag>().Add(tag);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);
        context.Set<PostTag>().Add(PostTag.Create(post.Id, tag.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPostBySlugQueryHandler(context);
        var result = await handler.Handle(new GetPostBySlugQuery { Slug = "published-post" }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Tech");
        result.TagNames.Should().ContainSingle(t => t == "AI");
    }

    [Fact]
    public async Task GetPostById_WhenPostIsDraft_ShouldStillReturnDto()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Draft Post", "draft-post-2", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPostByIdQueryHandler(context);
        var result = await handler.Handle(new GetPostByIdQuery { Id = post.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public async Task GetPostById_WhenNotFound_ShouldReturnNull()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var handler = new GetPostByIdQueryHandler(context);

        var result = await handler.Handle(new GetPostByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }
}
