# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [0.3.0] - 2025-10-26
### Fixed
- `InMemoryCache` now correctly commits entries by disposing `ICacheEntry` during `CreateEntry` (previously entries might not be persisted, causing `TryGetValue` to fail).
- Eliminated `NullReferenceException` in `GetAllValues()` by removing brittle reflection over `MemoryCache` internals.

### Changed
- Reworked internal storage to track keys via `ConcurrentDictionary` and clean up expired keys during enumeration.
- `GetAllValues()` enumerates tracked keys and returns only currently valid values.

### Tests
- All unit tests now pass (14/14).

## [0.2.0] - 2025-10-26
### Added
- Added `CacheOptions` with `AbsoluteExpirationTimeSec` for DI/config binding.
- New overloads for `AddStrongTypedInMemoryCache` accepting `CacheOptions` and `IConfiguration`.
- GitHub Actions workflow to pack and publish to NuGet.org.

### Changed
- Bumped package versions to 0.2.0 across all packable projects.

## [0.1.0] - 2025-10-26
### Added
- Initial release: `ICache` abstractions, `InMemoryCache` implementation, DI extensions.
- Unit tests for cache and DI registration.
