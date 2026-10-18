namespace StangaNetLib.FeatureFlags.Settings;

/// <summary>A single feature flag entry as read from <c>appsettings.json</c>.</summary>
public sealed class FeatureFlagEntry
{
    /// <summary>Master switch for this flag. Defaults to <c>false</c>.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Percentage of callers (0–100) who receive this feature.
    /// <c>null</c> applies the flag to all callers when enabled.
    /// </summary>
    public int? RolloutPercentage { get; set; }

    /// <summary>
    /// Tenant IDs allowed to activate this flag. <c>null</c> allows all tenants.
    /// </summary>
    public List<string>? AllowedTenantIds { get; set; }

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; set; }
}
