using StangaNetLib.Core.Common;
using StangaNetLib.FeatureFlags.Models;

namespace StangaNetLib.FeatureFlags.Abstractions;

/// <summary>
/// Evaluates feature flags for conditional feature activation.
/// Supports per-user rollout percentages and per-tenant activation.
/// </summary>
/// <remarks>
/// When using <b>StangaNetLib.Tenancy</b>, pass the current tenant identifier via the
/// <c>tenantId</c> parameter: <c>service.IsEnabledAsync("flag", tenantId: ctx.CurrentTenant?.TenantId)</c>.
/// </remarks>
public interface IFeatureFlagService
{
    /// <summary>
    /// Evaluates whether a feature flag is active for the given context.
    /// </summary>
    /// <param name="flagName">The unique name of the flag to evaluate. Must not be null or whitespace.</param>
    /// <param name="userId">
    /// Optional caller identity used for deterministic percentage-based rollout.
    /// When <c>null</c>, rollout evaluation treats the caller as an anonymous user.
    /// </param>
    /// <param name="tenantId">
    /// Optional tenant identifier. When the flag defines <c>AllowedTenantIds</c> and this is
    /// non-null, evaluation returns <c>false</c> unless the tenant is in the allowed list.
    /// When <c>null</c>, tenant restrictions are not enforced.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the flag is active for this context; <c>false</c> otherwise.
    /// Returns the configured <c>DefaultValue</c> when the flag is not found.
    /// Returns a <see cref="ErrorType.Validation"/> failure when <paramref name="flagName"/> is invalid.
    /// </returns>
    Task<Result<bool>> IsEnabledAsync(
        string flagName,
        string? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full definition of a named flag.
    /// </summary>
    /// <param name="flagName">The unique name of the flag to retrieve. Must not be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The <see cref="FeatureFlagDefinition"/> on success.
    /// Returns a <see cref="ErrorType.NotFound"/> failure when no flag with that name exists.
    /// Returns a <see cref="ErrorType.Validation"/> failure when <paramref name="flagName"/> is invalid.
    /// </returns>
    Task<Result<FeatureFlagDefinition>> GetFlagAsync(
        string flagName,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all registered feature flag definitions.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all known flags. Returns an empty list when no flags are registered.</returns>
    Task<Result<IReadOnlyList<FeatureFlagDefinition>>> GetAllFlagsAsync(
        CancellationToken cancellationToken = default);
}
