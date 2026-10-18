using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using StangaNetLib.Core.Guards;
using StangaNetLib.FeatureFlags.Abstractions;
using StangaNetLib.FeatureFlags.Models;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Implementations;

/// <summary>
/// Thread-safe in-memory <see cref="IFeatureFlagStore"/>.
/// Seeded from <see cref="FeatureFlagsSettings"/> at construction and supports runtime mutations
/// via <see cref="SetAsync"/> and <see cref="RemoveAsync"/>.
/// </summary>
internal sealed class InMemoryFeatureFlagStore : IFeatureFlagStore
{
    private readonly ConcurrentDictionary<string, FeatureFlagDefinition> _flags;

    public InMemoryFeatureFlagStore(IOptions<FeatureFlagsSettings> options)
    {
        Guard.Against.Null(options, nameof(options));

        _flags = new ConcurrentDictionary<string, FeatureFlagDefinition>(StringComparer.Ordinal);

        foreach (var (name, entry) in options.Value.Flags)
        {
            _flags[name] = MapEntry(name, entry);
        }
    }

    public Task<FeatureFlagDefinition?> GetAsync(string flagName, CancellationToken cancellationToken = default)
    {
        _flags.TryGetValue(flagName, out var flag);
        return Task.FromResult(flag);
    }

    public Task SetAsync(FeatureFlagDefinition flag, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(flag, nameof(flag));
        _flags[flag.Name] = flag;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string flagName, CancellationToken cancellationToken = default)
    {
        _flags.TryRemove(flagName, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FeatureFlagDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FeatureFlagDefinition> result = _flags.Values.ToList().AsReadOnly();
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
