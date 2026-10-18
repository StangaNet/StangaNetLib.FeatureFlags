using StangaNetLib.Core.Validators;

namespace StangaNetLib.FeatureFlags.Settings;

internal sealed class FeatureFlagsSettingsValidator : SettingsValidatorBase<FeatureFlagsSettings>
{
    protected override void ValidateCore(FeatureFlagsSettings options, List<string> errors)
    {
        foreach (var (name, entry) in options.Flags)
        {
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Flag name must not be null or whitespace.");

            if (entry.RolloutPercentage.HasValue &&
                (entry.RolloutPercentage.Value < 0 || entry.RolloutPercentage.Value > 100))
                errors.Add($"Flag '{name}': {nameof(entry.RolloutPercentage)} must be between 0 and 100.");
        }
    }
}
