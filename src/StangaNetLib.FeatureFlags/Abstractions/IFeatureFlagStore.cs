using StangaNetLib.FeatureFlags.Models;

namespace StangaNetLib.FeatureFlags.Abstractions;

/// <summary>
/// Persistence abstraction for feature flag definitions.
/// Implementations provide storage backends such as in-memory or configuration-backed stores.
/// </summary>
public interface IFeatureFlagStore
{
    /// <summary>
    /// Retrieves a flag definition by name, or <c>null</c> when not found.
    /// </summary>
    /// <param name="flagName">The exact flag name to look up. Case-sensitive.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="FeatureFlagDefinition"/>, or <c>null</c> if not found.</returns>
    Task<FeatureFlagDefinition?> GetAsync(string flagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or replaces a flag definition. Overwrites any existing flag with the same name.
    /// </summary>
    /// <param name="flag">The flag definition to store. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync(FeatureFlagDefinition flag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a flag by name. No-ops when the flag does not exist.
    /// </summary>
    /// <param name="flagName">The exact flag name to remove. Case-sensitive.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string flagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all known flag definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only snapshot of all registered flags. Returns an empty list when empty.</returns>
    Task<IReadOnlyList<FeatureFlagDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
