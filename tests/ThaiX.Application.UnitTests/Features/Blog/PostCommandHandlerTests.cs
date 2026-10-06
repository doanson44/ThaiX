using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Blog.Posts.Commands.ArchivePost;
using ThaiX.Application.Features.Blog.Posts.Commands.CreatePost;
using ThaiX.Application.Features.Blog.Posts.Commands.DeletePost;
using ThaiX.Application.Features.Blog.Posts.Commands.DuplicatePost;
using ThaiX.Application.Features.Blog.Posts.Commands.PublishPost;
using ThaiX.Application.Features.Blog.Posts.Commands.RestorePost;
using ThaiX.Application.Features.Blog.Posts.Commands.SchedulePost;
using ThaiX.Application.Features.Blog.Posts.Commands.UnpublishPost;
using ThaiX.Application.Features.Blog.Posts.Commands.UpdatePost;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.UnitTests.Features.Blog;

public sealed class PostCommandHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static Mock<ICurrentUserService> CreateCurrentUser(Guid? userId = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId ?? Guid.NewGuid());
        return currentUser;
    }

    [Fact]
    public async Task CreatePost_ShouldPersistDraftPostAndReturnId()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var currentUser = CreateCurrentUser();
        var handler = new CreatePostCommandHandler(context, currentUser.Object);

        var id = await handler.Handle(new CreatePostCommand
        {
            Title = "Hello World",
            Slug = "hello-world",
            Summary = "A short summary",
            ContentHtml = "<h1>Hello</h1><p>Some content here.</p>",
            CategoryId = null,
            MetaTitle = null,
            MetaDescription = null,
            TagIds = []
        }, CancellationToken.None);

        id.Should().NotBeEmpty();
        var saved = await context.Set<Post>().FirstAsync(p => p.Id == id, CancellationToken.None);
        saved.Status.Should().Be(PostStatus.Draft);
        saved.Slug.Should().Be("hello-world");
    }

    [Fact]
    public async Task CreatePost_WithDuplicateSlug_ShouldThrowOperationFailedException()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var currentUser = CreateCurrentUser();
        context.Set<Post>().Add(Post.Create(currentUser.Object.UserId, "First", "same-slug", "s", "c", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreatePostCommandHandler(context, currentUser.Object);

        await handler.Invoking(h => h.Handle(new CreatePostCommand
        {
            Title = "Second",
            Slug = "same-slug",
            Summary = "s",
            ContentHtml = "c",
            TagIds = []
        }, CancellationToken.None))
            .Should().ThrowAsync<OperationFailedException>();
    }

    [Fact]
    public async Task UpdatePost_ShouldReplaceContentAndTags()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var authorId = Guid.NewGuid();
        var post = Post.Create(authorId, "Title", "slug", "summary", "content", null, null, null);
        var tag = Tag.Create("Finance", "finance");
        context.Set<Post>().Add(post);
        context.Set<Tag>().Add(tag);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdatePostCommandHandler(context);
        await handler.Handle(new UpdatePostCommand
        {
            Id = post.Id,
            Title = "Updated Title",
            Slug = "updated-slug",
            Summary = "Updated summary",
            ContentHtml = "Updated content",
            CategoryId = null,
            MetaTitle = "Meta",
            MetaDescription = "Desc",
            TagIds = [tag.Id]
        }, CancellationToken.None);

        var updated = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        updated.Title.Should().Be("Updated Title");
        updated.Slug.Should().Be("updated-slug");

        var links = await context.Set<PostTag>().Where(pt => pt.PostId == post.Id).ToListAsync(CancellationToken.None);
        links.Should().ContainSingle(pt => pt.TagId == tag.Id);
    }

    [Fact]
    public async Task DeletePost_ShouldSoftDelete()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-del", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeletePostCommandHandler(context);
        await handler.Handle(new DeletePostCommand { Id = post.Id }, CancellationToken.None);

        var deleted = await context.Set<Post>().IgnoreQueryFilters().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePost_WhenNotFound_ShouldThrow()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var handler = new DeletePostCommandHandler(context);

        await handler.Invoking(h => h.Handle(new DeletePostCommand { Id = Guid.NewGuid() }, CancellationToken.None))
            .Should().ThrowAsync<OperationFailedException>();
    }

    [Fact]
    public async Task RestorePost_ShouldClearIsDeleted()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-restore", "summary", "content", null, null, null);
        post.SoftDelete();
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RestorePostCommandHandler(context);
        await handler.Handle(new RestorePostCommand { Id = post.Id }, CancellationToken.None);

        var restored = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        restored.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task DuplicatePost_ShouldCreateDraftCopyWithUniqueSlug()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var authorId = Guid.NewGuid();
        var source = Post.Create(authorId, "Original", "original-post", "summary", "content", null, null, null);
        source.Publish();
        var tag = Tag.Create("News", "news");
        context.Set<Post>().Add(source);
        context.Set<Tag>().Add(tag);
        await context.SaveChangesAsync(CancellationToken.None);
        context.Set<PostTag>().Add(PostTag.Create(source.Id, tag.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var currentUser = CreateCurrentUser(authorId);
        var handler = new DuplicatePostCommandHandler(context, currentUser.Object);
        var newId = await handler.Handle(new DuplicatePostCommand { Id = source.Id }, CancellationToken.None);

        var copy = await context.Set<Post>().FirstAsync(p => p.Id == newId, CancellationToken.None);
        copy.Status.Should().Be(PostStatus.Draft);
        copy.Slug.Should().Be("original-post-copy");
        copy.Title.Should().Be("Original (Copy)");

        var copyTags = await context.Set<PostTag>().Where(pt => pt.PostId == newId).ToListAsync(CancellationToken.None);
        copyTags.Should().ContainSingle(pt => pt.TagId == tag.Id);
    }

    [Fact]
    public async Task PublishPost_ShouldSetStatusPublishedAndPublishedAt()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-publish", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new PublishPostCommandHandler(context);
        await handler.Handle(new PublishPostCommand { Id = post.Id }, CancellationToken.None);

        var published = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        published.Status.Should().Be(PostStatus.Published);
        published.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SchedulePost_WithFutureDate_ShouldSetStatusScheduled()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-schedule", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new SchedulePostCommandHandler(context);
        var publishAt = DateTime.UtcNow.AddDays(1);
        await handler.Handle(new SchedulePostCommand { Id = post.Id, PublishAt = publishAt }, CancellationToken.None);

        var scheduled = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        scheduled.Status.Should().Be(PostStatus.Scheduled);
        scheduled.ScheduledAt.Should().BeCloseTo(publishAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UnpublishPost_ShouldRevertToDraft()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-unpub", "summary", "content", null, null, null);
        post.Publish();
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UnpublishPostCommandHandler(context);
        await handler.Handle(new UnpublishPostCommand { Id = post.Id }, CancellationToken.None);

        var reverted = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        reverted.Status.Should().Be(PostStatus.Draft);
        reverted.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task ArchivePost_ShouldSetStatusArchived()
    {
        await using var context = new TestApplicationDbContext(CreateOptions());
        var post = Post.Create(Guid.NewGuid(), "Title", "slug-archive", "summary", "content", null, null, null);
        context.Set<Post>().Add(post);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ArchivePostCommandHandler(context);
        await handler.Handle(new ArchivePostCommand { Id = post.Id }, CancellationToken.None);

        var archived = await context.Set<Post>().FirstAsync(p => p.Id == post.Id, CancellationToken.None);
        archived.Status.Should().Be(PostStatus.Archived);
    }
}
