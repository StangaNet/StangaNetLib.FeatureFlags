using StangaNetLib.FeatureFlags.Implementations;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Tests;

public sealed class ConfigurationFeatureFlagStoreTests
{
    private static ConfigurationFeatureFlagStore StoreWith(Action<FeatureFlagsSettings>? configure = null) =>
        new(SettingsFactory.Monitor(configure));

    [Fact]
    public async Task GetAsync_ReturnsConfiguredFlag()
    {
        var store = StoreWith(s =>
            s.Flags["my-flag"] = new FeatureFlagEntry { IsEnabled = true });

        var flag = await store.GetAsync("my-flag");

        flag.Should().NotBeNull();
        flag!.Name.Should().Be("my-flag");
        flag.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_UnknownFlag_ReturnsNull()
    {
        var store = StoreWith();

        var flag = await store.GetAsync("unknown");

        flag.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_MapsRolloutPercentage()
    {
        var store = StoreWith(s =>
            s.Flags["rollout-flag"] = new FeatureFlagEntry { IsEnabled = true, RolloutPercentage = 30 });

        var flag = await store.GetAsync("rollout-flag");

        flag!.RolloutPercentage.Should().Be(30);
    }

    [Fact]
    public async Task GetAsync_MapsAllowedTenantIds()
    {
        var store = StoreWith(s =>
            s.Flags["tenant-flag"] = new FeatureFlagEntry
            {
                IsEnabled = true,
                AllowedTenantIds = ["tenant-a", "tenant-b"]
            });

        var flag = await store.GetAsync("tenant-flag");

        flag!.AllowedTenantIds.Should().BeEquivalentTo(["tenant-a", "tenant-b"]);
    }

    [Fact]
    public async Task GetAsync_NullAllowedTenantIds_MapToNull()
    {
        var store = StoreWith(s =>
            s.Flags["open-flag"] = new FeatureFlagEntry { IsEnabled = true, AllowedTenantIds = null });

        var flag = await store.GetAsync("open-flag");

        flag!.AllowedTenantIds.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllConfiguredFlags()
    {
        var store = StoreWith(s =>
        {
            s.Flags["flag-1"] = new FeatureFlagEntry { IsEnabled = true };
            s.Flags["flag-2"] = new FeatureFlagEntry { IsEnabled = false };
        });

        var all = await store.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(f => f.Name).Should().Contain(["flag-1", "flag-2"]);
    }

    [Fact]
    public async Task GetAllAsync_EmptySettings_ReturnsEmptyList()
    {
        var store = StoreWith();

        var all = await store.GetAllAsync();

        all.Should().BeEmpty();
    }

    [Fact]
    public async Task SetAsync_IsNoOp()
    {
        var store = StoreWith();
        var act = async () => await store.SetAsync(new Models.FeatureFlagDefinition { Name = "x", IsEnabled = true });

        await act.Should().NotThrowAsync();
        (await store.GetAsync("x")).Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_IsNoOp()
    {
        var store = StoreWith(s =>
            s.Flags["flag"] = new FeatureFlagEntry { IsEnabled = true });

        await store.RemoveAsync("flag");

        (await store.GetAsync("flag")).Should().NotBeNull();
    }
}
