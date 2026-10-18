using StangaNetLib.FeatureFlags.Implementations;
using StangaNetLib.FeatureFlags.Models;

namespace StangaNetLib.FeatureFlags.Tests;

public sealed class InMemoryFeatureFlagStoreTests
{
    private static InMemoryFeatureFlagStore EmptyStore() =>
        new(SettingsFactory.Options());

    [Fact]
    public async Task SetAsync_StoresFlag_GetAsyncReturnsSameFlag()
    {
        var store = EmptyStore();
        var flag = new FeatureFlagDefinition { Name = "flag-a", IsEnabled = true };

        await store.SetAsync(flag);
        var result = await store.GetAsync("flag-a");

        result.Should().NotBeNull();
        result!.Name.Should().Be("flag-a");
        result.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_UnknownFlag_ReturnsNull()
    {
        var store = EmptyStore();

        var result = await store.GetAsync("unknown");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingFlag()
    {
        var store = EmptyStore();
        await store.SetAsync(new FeatureFlagDefinition { Name = "flag-a", IsEnabled = true });
        await store.SetAsync(new FeatureFlagDefinition { Name = "flag-a", IsEnabled = false });

        var result = await store.GetAsync("flag-a");

        result!.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_RemovesExistingFlag()
    {
        var store = EmptyStore();
        await store.SetAsync(new FeatureFlagDefinition { Name = "flag-a", IsEnabled = true });

        await store.RemoveAsync("flag-a");
        var result = await store.GetAsync("flag-a");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_NoOp_WhenFlagDoesNotExist()
    {
        var store = EmptyStore();
        var act = async () => await store.RemoveAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredFlags()
    {
        var store = EmptyStore();
        await store.SetAsync(new FeatureFlagDefinition { Name = "flag-a", IsEnabled = true });
        await store.SetAsync(new FeatureFlagDefinition { Name = "flag-b", IsEnabled = false });

        var all = await store.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(f => f.Name).Should().Contain(["flag-a", "flag-b"]);
    }

    [Fact]
    public async Task GetAllAsync_EmptyStore_ReturnsEmptyList()
    {
        var store = EmptyStore();

        var all = await store.GetAllAsync();

        all.Should().BeEmpty();
    }

    [Fact]
    public async Task Constructor_SeedsFromSettings()
    {
        var store = new InMemoryFeatureFlagStore(SettingsFactory.Options(s =>
        {
            s.Flags["seeded-flag"] = new Settings.FeatureFlagEntry
            {
                IsEnabled = true,
                RolloutPercentage = 50,
                Description = "seeded"
            };
        }));

        var flag = await store.GetAsync("seeded-flag");

        flag.Should().NotBeNull();
        flag!.IsEnabled.Should().BeTrue();
        flag.RolloutPercentage.Should().Be(50);
        flag.Description.Should().Be("seeded");
    }

    [Fact]
    public async Task SetAsync_ThrowsArgumentNullException_WhenFlagIsNull()
    {
        var store = EmptyStore();
        var act = async () => await store.SetAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConcurrentSet_AllFlagsStoredCorrectly()
    {
        var store = EmptyStore();
        var tasks = Enumerable.Range(0, 50)
            .Select(i => store.SetAsync(new FeatureFlagDefinition { Name = $"flag-{i}", IsEnabled = true }));

        await Task.WhenAll(tasks);

        var all = await store.GetAllAsync();
        all.Should().HaveCount(50);
    }
}
