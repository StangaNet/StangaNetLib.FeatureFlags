# StangaNetLib.FeatureFlags

[![.NET CI](https://github.com/StangaNet/StangaNetLib.FeatureFlags/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.FeatureFlags/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

Feature flags with percentage rollout, tenant allow-lists, runtime overrides, and live configuration reload. Failures and lookups surface as `Result` / `Result<T>` from [`StangaNetLib.Core`](https://github.com/StangaNet/StangaNetLib.Core).

## Installation

The package is hosted on **GitHub Packages** (pulls Core from the same feed).

```xml
<!-- NuGet.config — source key must match your packageSourceCredentials -->
<configuration>
  <packageSources>
    <add key="StangaNet" value="https://nuget.pkg.github.com/StangaNet/index.json" />
  </packageSources>
</configuration>
```

```xml
<!-- .csproj -->
<PackageReference Include="StangaNetLib.FeatureFlags" Version="1.0.0" />
```

See [CHANGELOG.md](./CHANGELOG.md) for release history.

## Core Components

| Namespace | Components | Purpose |
|---|---|---|
| `StangaNetLib.FeatureFlags.Abstractions` | `IFeatureFlagService`, `IFeatureFlagStore` | Evaluation and storage contracts |
| `StangaNetLib.FeatureFlags.Models` | `FeatureFlagDefinition` | Immutable flag descriptor |
| `StangaNetLib.FeatureFlags.Settings` | `FeatureFlagsSettings`, `FeatureFlagEntry` | Configuration model |
| `StangaNetLib.FeatureFlags.Extensions` | `AddStangaNetLibFeatureFlags`, `AddStangaNetLibFeatureFlagsFromConfiguration` | DI |

## Quick Start

```csharp
// In-memory store seeded from configuration (runtime-mutable via IFeatureFlagStore)
builder.Services.AddStangaNetLibFeatureFlags(builder.Configuration);

// Or configuration-backed with live reload (SetAsync/RemoveAsync are no-ops)
builder.Services.AddStangaNetLibFeatureFlagsFromConfiguration(builder.Configuration);
```

Optional `appsettings.json`:

```json
{
  "FeatureFlagsSettings": {
    "DefaultValue": false,
    "Flags": {
      "dark-mode": { "IsEnabled": true },
      "new-checkout": { "IsEnabled": true, "RolloutPercentage": 20 },
      "beta-api": {
        "IsEnabled": true,
        "AllowedTenantIds": ["tenant-a", "tenant-b"]
      }
    }
  }
}
```

```csharp
var result = await flags.IsEnabledAsync("new-checkout", userId: user.Id, tenantId: tenant?.Id);
if (result.IsSuccess && result.Value)
{
    // new path
}
```

## Evaluation order

1. Flag not found → `DefaultValue` (default `false`)
2. `IsEnabled = false` → `false`
3. `AllowedTenantIds` set and `tenantId` not listed → `false`
4. `RolloutPercentage` set → deterministic bucket on `userId` + flag name
5. Otherwise → `true`

## License

Apache-2.0 — see [LICENSE](./LICENSE) and [NOTICE](./NOTICE).
