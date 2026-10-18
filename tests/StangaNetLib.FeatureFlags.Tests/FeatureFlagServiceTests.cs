using StangaNetLib.Core.Common;
using StangaNetLib.FeatureFlags.Implementations;
using StangaNetLib.FeatureFlags.Models;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Tests;

public sealed class FeatureFlagServiceTests
{
    private static FeatureFlagService BuildService(
        Action<FeatureFlagsSettings>? settingsConfigure = null,
        Action<InMemoryFeatureFlagStore>? storeConfigure = null)
    {
        var options = SettingsFactory.Options(settingsConfigure);
        var store = new InMemoryFeatureFlagStore(options);
        storeConfigure?.Invoke(store);
        return new FeatureFlagService(store, options);
    }

    [Fact]
    public async Task IsEnabledAsync_EnabledFlag_ReturnsTrue()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = true }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_DisabledFlag_ReturnsFalse()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = false }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownFlag_ReturnsDefaultValueFalse()
    {
        var svc = BuildService();

        var result = await svc.IsEnabledAsync("unknown");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownFlag_ReturnsDefaultValueTrue_WhenConfigured()
    {
        var svc = BuildService(s => s.DefaultValue = true);

        var result = await svc.IsEnabledAsync("unknown");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsEnabledAsync_InvalidFlagName_ReturnsValidationFailure(string? flagName)
    {
        var svc = BuildService();

        var result = await svc.IsEnabledAsync(flagName!);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GetFlagAsync_KnownFlag_ReturnsDefinition()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = true, Description = "test" })
                 .GetAwaiter().GetResult());

        var result = await svc.GetFlagAsync("feat");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("test");
    }

    [Fact]
    public async Task GetFlagAsync_UnknownFlag_ReturnsNotFoundFailure()
    {
        var svc = BuildService();

        var result = await svc.GetFlagAsync("no-such-flag");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetFlagAsync_InvalidFlagName_ReturnsValidationFailure(string? flagName)
    {
        var svc = BuildService();

        var result = await svc.GetFlagAsync(flagName!);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GetAllFlagsAsync_ReturnsAllFlags()
    {
        var svc = BuildService(storeConfigure: store =>
        {
            store.SetAsync(new FeatureFlagDefinition { Name = "a", IsEnabled = true }).GetAwaiter().GetResult();
            store.SetAsync(new FeatureFlagDefinition { Name = "b", IsEnabled = false }).GetAwaiter().GetResult();
        });

        var result = await svc.GetAllFlagsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllFlagsAsync_EmptyStore_ReturnsEmptyList()
    {
        var svc = BuildService();

        var result = await svc.GetAllFlagsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task IsEnabledAsync_RolloutPercentage100_AlwaysTrue()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = true, RolloutPercentage = 100 })
                 .GetAwaiter().GetResult());

        for (var i = 0; i < 20; i++)
        {
            var result = await svc.IsEnabledAsync("feat", userId: $"user-{i}");
            result.Value.Should().BeTrue($"user-{i} should be in 100% rollout");
        }
    }

    [Fact]
    public async Task IsEnabledAsync_RolloutPercentage0_AlwaysFalse()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = true, RolloutPercentage = 0 })
                 .GetAwaiter().GetResult());

        for (var i = 0; i < 20; i++)
        {
            var result = await svc.IsEnabledAsync("feat", userId: $"user-{i}");
            result.Value.Should().BeFalse($"user-{i} should be outside 0% rollout");
        }
    }

    [Fact]
    public async Task IsEnabledAsync_Rollout_IsDeterministic_ForSameUserId()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition { Name = "feat", IsEnabled = true, RolloutPercentage = 50 })
                 .GetAwaiter().GetResult());

        var first = await svc.IsEnabledAsync("feat", userId: "stable-user");
        var second = await svc.IsEnabledAsync("feat", userId: "stable-user");

        first.Value.Should().Be(second.Value);
    }

    [Fact]
    public async Task IsEnabledAsync_TenantFilter_MatchingTenant_ReturnsTrue()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition
            {
                Name = "feat",
                IsEnabled = true,
                AllowedTenantIds = ["tenant-a"]
            }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat", tenantId: "tenant-a");

        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_TenantFilter_NonMatchingTenant_ReturnsFalse()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition
            {
                Name = "feat",
                IsEnabled = true,
                AllowedTenantIds = ["tenant-a"]
            }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat", tenantId: "tenant-b");

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_TenantFilter_NullTenantId_ReturnsFalse()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition
            {
                Name = "feat",
                IsEnabled = true,
                AllowedTenantIds = ["tenant-a"]
            }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat", tenantId: null);

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_NullAllowedTenantIds_AllowsAllTenants()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition
            {
                Name = "feat",
                IsEnabled = true,
                AllowedTenantIds = null
            }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat", tenantId: "any-tenant");

        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_EmptyAllowedTenantIds_AlwaysReturnsFalse()
    {
        var svc = BuildService(storeConfigure: store =>
            store.SetAsync(new FeatureFlagDefinition
            {
                Name = "feat",
                IsEnabled = true,
                AllowedTenantIds = new List<string>().AsReadOnly()
            }).GetAwaiter().GetResult());

        var result = await svc.IsEnabledAsync("feat", tenantId: "any-tenant");

        result.Value.Should().BeFalse();
    }
}
