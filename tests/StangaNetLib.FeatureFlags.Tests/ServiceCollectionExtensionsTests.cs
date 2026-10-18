using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.FeatureFlags.Abstractions;
using StangaNetLib.FeatureFlags.Extensions;
using StangaNetLib.FeatureFlags.Implementations;

namespace StangaNetLib.FeatureFlags.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    private static IConfiguration EmptyConfiguration() =>
        new ConfigurationBuilder().Build();

    [Fact]
    public void AddStangaNetLibFeatureFlags_RegistersIFeatureFlagService()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlags();

        var sp = services.BuildServiceProvider();
        sp.GetService<IFeatureFlagService>().Should().NotBeNull()
            .And.BeOfType<FeatureFlagService>();
    }

    [Fact]
    public void AddStangaNetLibFeatureFlags_RegistersInMemoryStore()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlags();

        var sp = services.BuildServiceProvider();
        sp.GetService<IFeatureFlagStore>().Should().NotBeNull()
            .And.BeOfType<InMemoryFeatureFlagStore>();
    }

    [Fact]
    public void AddStangaNetLibFeatureFlags_WithConfiguration_RegistersIFeatureFlagService()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlags(EmptyConfiguration());

        var sp = services.BuildServiceProvider();
        sp.GetService<IFeatureFlagService>().Should().NotBeNull();
    }

    [Fact]
    public void AddStangaNetLibFeatureFlagsFromConfiguration_RegistersIFeatureFlagService()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlagsFromConfiguration(EmptyConfiguration());

        var sp = services.BuildServiceProvider();
        sp.GetService<IFeatureFlagService>().Should().NotBeNull()
            .And.BeOfType<FeatureFlagService>();
    }

    [Fact]
    public void AddStangaNetLibFeatureFlagsFromConfiguration_RegistersConfigurationStore()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlagsFromConfiguration(EmptyConfiguration());

        var sp = services.BuildServiceProvider();
        sp.GetService<IFeatureFlagStore>().Should().NotBeNull()
            .And.BeOfType<ConfigurationFeatureFlagStore>();
    }

    [Fact]
    public void AddStangaNetLibFeatureFlags_CalledTwice_ReturnsSameSingletonInstance()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibFeatureFlags();
        services.AddStangaNetLibFeatureFlags();

        var sp = services.BuildServiceProvider();
        var first = sp.GetService<IFeatureFlagService>();
        var second = sp.GetService<IFeatureFlagService>();

        first.Should().BeSameAs(second);
    }
}
