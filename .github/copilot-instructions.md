# StangaNetLib.FeatureFlags — Copilot instructions

- Feature flags: in-memory and configuration-backed stores, percentage rollout, tenant allow-list.
- Prefer `Result` / `Result<T>` and `Guard` from `StangaNetLib.Core` — do not reintroduce local Result/Guard types.
- Keep public APIs documented (`///`) and options-driven via `FeatureFlagsSettings`.
- DI: `AddStangaNetLibFeatureFlags` / `AddStangaNetLibFeatureFlagsFromConfiguration`.
- Match existing naming, file-scoped namespaces, and EditorConfig / Roslyn conventions.
- Tests use xUnit v3 + FluentAssertions; cover both `net8.0` and `net9.0` when changing shared behavior.
- Rollout buckets must stay deterministic (`userId` + flag name); do not switch hash algorithm without an explicit review.
