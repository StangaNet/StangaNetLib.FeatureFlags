using Microsoft.Extensions.Options;
using StangaNetLib.FeatureFlags.Settings;

namespace StangaNetLib.FeatureFlags.Tests;

internal sealed class FakeOptionsMonitor<T> : IOptionsMonitor<T>
{
    private T _value;

    public FakeOptionsMonitor(T value) => _value = value;

    public T CurrentValue => _value;

    public void UpdateValue(T value) => _value = value;

    public T Get(string? name) => _value;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}

internal static class SettingsFactory
{
    internal static IOptions<FeatureFlagsSettings> Options(
        Action<FeatureFlagsSettings>? configure = null)
    {
        var settings = new FeatureFlagsSettings();
        configure?.Invoke(settings);
        return Microsoft.Extensions.Options.Options.Create(settings);
    }

    internal static FakeOptionsMonitor<FeatureFlagsSettings> Monitor(
        Action<FeatureFlagsSettings>? configure = null)
    {
        var settings = new FeatureFlagsSettings();
        configure?.Invoke(settings);
        return new FakeOptionsMonitor<FeatureFlagsSettings>(settings);
    }
}
