using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Guards;
using StangaNetLib.FeatureFlags.Abstractions;
using StangaNetLib.FeatureFlags.Errors;
using StangaNetLib.FeatureFlags.Models;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Implementations;

/// <summary>
/// Default <see cref="IFeatureFlagService"/> implementation.
/// Evaluates flag state by consulting the registered <see cref="IFeatureFlagStore"/>,
/// applying rollout-percentage logic and optional tenant filtering.
/// </summary>
internal sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagStore _store;
    private readonly IOptions<FeatureFlagsSettings> _settings;

    public FeatureFlagService(IFeatureFlagStore store, IOptions<FeatureFlagsSettings> settings)
    {
        _store = Guard.Against.Null(store, nameof(store));
        _settings = Guard.Against.Null(settings, nameof(settings));
    }

    public async Task<Result<bool>> IsEnabledAsync(
        string flagName,
        string? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            return Result<bool>.Failure(FeatureFlagErrors.InvalidFlagName());

        var flag = await _store.GetAsync(flagName, cancellationToken).ConfigureAwait(false);

        if (flag is null)
            return Result<bool>.Success(_settings.Value.DefaultValue);

        if (!flag.IsEnabled)
            return Result<bool>.Success(false);

        if (!IsTenantAllowed(flag, tenantId))
            return Result<bool>.Success(false);

        if (!IsInRollout(flag, userId))
            return Result<bool>.Success(false);

        return Result<bool>.Success(true);
    }

    public async Task<Result<FeatureFlagDefinition>> GetFlagAsync(
        string flagName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            return Result<FeatureFlagDefinition>.Failure(FeatureFlagErrors.InvalidFlagName());

        var flag = await _store.GetAsync(flagName, cancellationToken).ConfigureAwait(false);

        return flag is not null
            ? Result<FeatureFlagDefinition>.Success(flag)
            : Result<FeatureFlagDefinition>.Failure(FeatureFlagErrors.FlagNotFound(flagName));
    }

    public async Task<Result<IReadOnlyList<FeatureFlagDefinition>>> GetAllFlagsAsync(
        CancellationToken cancellationToken = default)
    {
        var flags = await _store.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Result<IReadOnlyList<FeatureFlagDefinition>>.Success(flags);
    }

    private static bool IsTenantAllowed(FeatureFlagDefinition flag, string? tenantId)
    {
        if (flag.AllowedTenantIds is null)
            return true;

        if (tenantId is null)
            return false;

        return flag.AllowedTenantIds.Contains(tenantId, StringComparer.Ordinal);
    }

    private static bool IsInRollout(FeatureFlagDefinition flag, string? userId)
    {
        if (!flag.RolloutPercentage.HasValue || flag.RolloutPercentage.Value >= 100)
            return true;

        if (flag.RolloutPercentage.Value <= 0)
            return false;

        var bucket = ComputeBucket(userId ?? string.Empty, flag.Name);
        return bucket < flag.RolloutPercentage.Value;
    }

    private static int ComputeBucket(string userId, string flagName)
    {
        var input = $"{userId}:{flagName}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = MD5.HashData(bytes);
        var value = BitConverter.ToUInt32(hash, 0);
        return (int)(value % 100);
    }
}
