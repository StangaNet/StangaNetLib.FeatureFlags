namespace StangaNetLib.FeatureFlags.Models;

/// <summary>
/// Describes a feature flag and its activation rules.
/// </summary>
public sealed class FeatureFlagDefinition
{
    /// <summary>Unique name of the flag. Used as the lookup key.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Master switch. When <c>false</c>, the flag is always inactive regardless of other settings.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Percentage of callers (0–100) who receive this feature when it is enabled.
    /// <c>null</c> means the flag applies to 100% of callers (same as 100).
    /// Uses a deterministic hash of <c>userId</c> + flag name so the same user always
    /// gets the same outcome for a given flag.
    /// </summary>
    public int? RolloutPercentage { get; init; }

    /// <summary>
    /// When non-null, restricts activation to callers whose <c>tenantId</c> matches one of these values.
    /// An empty list means no tenant is allowed. <c>null</c> means all tenants are allowed.
    /// </summary>
    public IReadOnlyList<string>? AllowedTenantIds { get; init; }

    /// <summary>Optional human-readable description of the flag's purpose.</summary>
    public string? Description { get; init; }
}
