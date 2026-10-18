using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StangaNetLib.FeatureFlags.Abstractions;
using StangaNetLib.FeatureFlags.Implementations;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Extensions;

/// <summary>
/// Extension methods for registering StangaNetLib.FeatureFlags services.
/// </summary>
/// <example>
/// appsettings.json:
/// <code>
/// {
///   "FeatureFlagsSettings": {
///     "DefaultValue": false,
///     "Flags": {
///       "dark-mode": { "IsEnabled": true },
///       "new-checkout": { "IsEnabled": true, "RolloutPercentage": 20 },
///       "beta-feature": {
///         "IsEnabled": true,
///         "AllowedTenantIds": ["tenant-a", "tenant-b"]
///       }
///     }
///   }
/// }
/// </code>
/// Program.cs (in-memory, seeded from configuration):
/// <code>
/// builder.Services.AddStangaNetLibFeatureFlags(builder.Configuration);
/// </code>
/// Program.cs (configuration-backed, live reload):
/// <code>
/// builder.Services.AddStangaNetLibFeatureFlagsFromConfiguration(builder.Configuration);
/// </code>
/// Inject and use:
/// <code>
/// public class MyService(IFeatureFlagService flags)
/// {
///     public async Task DoWorkAsync(string userId, string tenantId)
///     {
///         var result = await flags.IsEnabledAsync("new-checkout", userId, tenantId);
///         if (result.IsSuccess &amp;&amp; result.Value)
///             // new checkout flow
///     }
/// }
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers StangaNetLib.FeatureFlags with the <b>in-memory</b> backend seeded from
    /// the <c>FeatureFlagsSettings</c> section of <paramref name="configuration"/>.
    /// Flags can be overridden at runtime via <see cref="IFeatureFlagStore"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddStangaNetLibFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FeatureFlagsSettings>(configuration.GetSection(FeatureFlagsSettings.SectionName));
        return RegisterInMemory(services);
    }

    /// <summary>
    /// Registers StangaNetLib.FeatureFlags with an empty <b>in-memory</b> backend using default settings.
    /// Flags must be added programmatically via <see cref="IFeatureFlagStore"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddStangaNetLibFeatureFlags(this IServiceCollection services)
    {
        services.Configure<FeatureFlagsSettings>(_ => { });
        return RegisterInMemory(services);
    }

    /// <summary>
    /// Registers StangaNetLib.FeatureFlags with the <b>configuration-backed</b> backend.
    /// Reads flags from <c>FeatureFlagsSettings</c> section of <paramref name="configuration"/>
    /// and reflects live appsettings changes via <see cref="IOptionsMonitor{TOptions}"/>.
    /// <see cref="IFeatureFlagStore.SetAsync"/> and <see cref="IFeatureFlagStore.RemoveAsync"/>
    /// are no-ops on this backend.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddStangaNetLibFeatureFlagsFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FeatureFlagsSettings>(configuration.GetSection(FeatureFlagsSettings.SectionName));
        return RegisterConfiguration(services);
    }

    private static IServiceCollection RegisterInMemory(IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<FeatureFlagsSettings>, FeatureFlagsSettingsValidator>();
        services.TryAddSingleton<IFeatureFlagStore, InMemoryFeatureFlagStore>();
        services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();
        return services;
    }

    private static IServiceCollection RegisterConfiguration(IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<FeatureFlagsSettings>, FeatureFlagsSettingsValidator>();
        services.TryAddSingleton<IFeatureFlagStore, ConfigurationFeatureFlagStore>();
        services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();
        return services;
    }
}
