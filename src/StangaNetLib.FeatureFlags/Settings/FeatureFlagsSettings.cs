namespace StangaNetLib.FeatureFlags.Settings;

/// <summary>Configuration for StangaNetLib.FeatureFlags services.</summary>
public sealed class FeatureFlagsSettings
{
    /// <summary>The <c>appsettings.json</c> section key. Value: <c>"FeatureFlagsSettings"</c>.</summary>
    public const string SectionName = "FeatureFlagsSettings";

    /// <summary>
    /// Named flag definitions. Dictionary key is the flag name.
    /// </summary>
    public Dictionary<string, FeatureFlagEntry> Flags { get; set; } = [];

    /// <summary>
    /// Value returned by <c>IsEnabledAsync</c> when the requested flag is not found in the store.
    /// Defaults to <c>false</c> (safe-off by default).
    /// </summary>
    public bool DefaultValue { get; set; } = false;
}
