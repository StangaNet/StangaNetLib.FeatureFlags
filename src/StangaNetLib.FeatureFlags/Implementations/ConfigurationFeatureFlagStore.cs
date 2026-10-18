using Microsoft.Extensions.Options;
using StangaNetLib.Core.Guards;
using StangaNetLib.FeatureFlags.Abstractions;
using StangaNetLib.FeatureFlags.Models;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Implementations;

/// <summary>
/// Read-only <see cref="IFeatureFlagStore"/> backed by <see cref="FeatureFlagsSettings"/>.
/// Reflects live configuration changes when <see cref="IOptionsMonitor{TOptions}"/> is used.
/// <see cref="SetAsync"/> and <see cref="RemoveAsync"/> are no-ops on this backend.
/// </summary>
internal sealed class ConfigurationFeatureFlagStore : IFeatureFlagStore
{
    private readonly IOptionsMonitor<FeatureFlagsSettings> _monitor;

    public ConfigurationFeatureFlagStore(IOptionsMonitor<FeatureFlagsSettings> monitor)
    {
        _monitor = Guard.Against.Null(monitor, nameof(monitor));
    }

    public Task<FeatureFlagDefinition?> GetAsync(string flagName, CancellationToken cancellationToken = default)
    {
        _monitor.CurrentValue.Flags.TryGetValue(flagName, out var entry);
        var result = entry is not null ? MapEntry(flagName, entry) : null;
        return Task.FromResult(result);
    }

    public Task SetAsync(FeatureFlagDefinition flag, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string flagName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<FeatureFlagDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FeatureFlagDefinition> result = _monitor.CurrentValue.Flags
            .Select(kv => MapEntry(kv.Key, kv.Value))
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    private static FeatureFlagDefinition MapEntry(string name, FeatureFlagEntry entry) =>
        new()
        {
            Name = name,
            IsEnabled = entry.IsEnabled,
            RolloutPercentage = entry.RolloutPercentage,
            AllowedTenantIds = entry.AllowedTenantIds?.AsReadOnly(),
            Description = entry.Description
        };
}
