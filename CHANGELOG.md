# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [Unreleased]
### Added
- Comprehensive benchmark suite using BenchmarkDotNet covering:
  - CreateEntry benchmarks for multiple key types (int, string, Guid, complex objects)
  - TryGetValue benchmarks with hit/miss scenarios and access patterns
  - GetAllValues benchmarks with various data sizes and expiration scenarios
  - Remove benchmarks with different removal patterns
- Mixed-operation benchmarks simulating realistic workloads
  - Memory allocation benchmarks testing GC pressure and object sizes
- Benchmark CI/CD integration: runs on main branch pushes and releases
- Benchmark results exported as HTML, Markdown, CSV, and plots
- Detailed benchmark documentation in StrongTypedCache.Benchmarks/README.md

## [1.0.1] - 2025-10-27
### Changed
- CI: Pack and publish all three NuGet packages (Abstractions, InMemory, Extensions) instead of only Extensions.
- `StrongTypedCache.Extensions` no longer bundles referenced projects; it depends on `StrongTypedCache.Abstractions` and `StrongTypedCache.InMemory` as NuGet dependencies.

## [1.0.0] - 2025-10-26
### Changed
- Promoted to stable 1.0.0. API surface unchanged from 0.3.0.

### Fixed
- Integration tests added for DI registration (options and configuration binding) to guard behavior.

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
