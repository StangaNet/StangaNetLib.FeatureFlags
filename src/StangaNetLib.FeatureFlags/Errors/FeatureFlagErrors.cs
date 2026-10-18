using StangaNetLib.Core.Common;

namespace StangaNetLib.FeatureFlags.Errors;

internal static class FeatureFlagErrors
{
    internal static Error FlagNotFound(string flagName)
        => Error.NotFound("FeatureFlag", flagName);

    internal static Error InvalidFlagName()
        => Error.Validation("FlagName", "Flag name must not be null or whitespace.");
}
