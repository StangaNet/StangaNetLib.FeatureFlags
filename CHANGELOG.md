# Changelog

All notable changes to **StangaNetLib.FeatureFlags** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-10-18

### Added
- `IFeatureFlagService` — evaluate flags with optional user rollout and tenant allow-list.
- `IFeatureFlagStore` — persistence abstraction with in-memory and configuration-backed stores.
- Percentage rollout via deterministic MD5 bucket on `userId` + flag name.
- `FeatureFlagsSettings` / `FeatureFlagEntry` with options validation.
- DI: `AddStangaNetLibFeatureFlags` (in-memory, optional config seed) and `AddStangaNetLibFeatureFlagsFromConfiguration` (live reload).
- Dual targeting `net8.0` / `net9.0`, XML docs, symbols (`snupkg`), package icon, and MSBuild consumer metadata stamp.
- Depends on `StangaNetLib.Core` 1.0.0; Microsoft.Extensions.* pinned to 8.x.
- CI via shared StangaNet reusable workflow (push / PR / manual; publish outside PRs).

[Unreleased]: https://github.com/StangaNet/StangaNetLib.FeatureFlags/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/StangaNet/StangaNetLib.FeatureFlags/releases/tag/v1.0.0
