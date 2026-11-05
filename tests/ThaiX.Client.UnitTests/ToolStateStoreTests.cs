using Microsoft.JSInterop;
using ThaiX.Client.Services.Tools;
using Xunit;

namespace ThaiX.Client.UnitTests;

public sealed class ToolStateStoreTests
{
    [Fact]
    public async Task ToggleFavoriteAsync_WhenNotFavorite_ShouldAddKnownTool()
    {
        // Arrange
        var js = new InMemoryJsRuntime();
        var store = new ToolStateStore(js, null!, null!, null!);
        var tool = ToolCatalog.Find("json-formatter")!;

        // Act
        var isFavorite = await store.ToggleFavoriteAsync(tool);
        var favorites = await store.GetFavoriteToolsAsync();

        // Assert
        Assert.True(isFavorite);
        Assert.Single(favorites);
        Assert.Equal(tool.Slug, favorites[0].Slug);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_WhenAlreadyFavorite_ShouldRemoveTool()
    {
        // Arrange
        var js = new InMemoryJsRuntime();
        var store = new ToolStateStore(js, null!, null!, null!);
        var tool = ToolCatalog.Find("base64")!;
        await store.ToggleFavoriteAsync(tool);

        // Act
        var isFavorite = await store.ToggleFavoriteAsync(tool);
        var favorites = await store.GetFavoriteToolsAsync();

        // Assert
        Assert.False(isFavorite);
        Assert.Empty(favorites);
    }

    [Fact]
    public async Task ClearLastUsedAsync_ShouldRemoveRecentHistory()
    {
        // Arrange
        var js = new InMemoryJsRuntime();
        var store = new ToolStateStore(js, null!, null!, null!);
        await store.RecordLastUsedAsync(ToolCatalog.Find("json-formatter")!);

        // Act
        await store.ClearLastUsedAsync();

        // Assert
        Assert.Empty(await store.GetLastUsedAsync());
    }

    [Fact]
    public async Task GetFavoriteToolsAsync_WithDuplicateAndUnknownSlugs_ShouldReturnUniqueKnownTools()
    {
        // Arrange
        var js = new InMemoryJsRuntime();
        js.Set("ThaiX.tools.favorites",
            "[\"base64\",\"BASE64\",\"missing-tool\",\"json-formatter\"]");
        var store = new ToolStateStore(js, null!, null!, null!);

        // Act
        var favorites = await store.GetFavoriteToolsAsync();

        // Assert
        Assert.Equal(2, favorites.Count);
        Assert.Equal("base64", favorites[0].Slug);
        Assert.Equal("json-formatter", favorites[1].Slug);
    }

    private sealed class InMemoryJsRuntime : IJSRuntime
    {
        private readonly Dictionary<string, string> _storage = new(StringComparer.Ordinal);

        public void Set(string key, string value) => _storage[key] = value;

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            args ??= [];
            switch (identifier)
            {
                case "localStorage.getItem":
                    return ValueTask.FromResult(_storage.TryGetValue((string)args[0]!, out var value)
                        ? (TValue)(object)value
                        : default!);
                case "localStorage.setItem":
                    _storage[(string)args[0]!] = (string)args[1]!;
                    return ValueTask.FromResult(default(TValue)!);
                case "localStorage.removeItem":
                    _storage.Remove((string)args[0]!);
                    return ValueTask.FromResult(default(TValue)!);
                default:
                    throw new InvalidOperationException($"Unexpected JS invocation: {identifier}");
            }
        }
    }
}
