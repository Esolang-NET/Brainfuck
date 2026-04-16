# Changelog

All notable changes to this repository are documented in this file.

The format is based on Keep a Changelog.

## [Unreleased]

## [0.1.1-preview-1] - 2026-04-16

### Added
- Added Interpreter test project with option binding and command registration tests.
- Added dotnet tool E2E checks in CI for pack/install/run/parse flow (PR workflow).
- Added XML documentation enforcement for packable projects.

### Changed
- Replaced file-based sample app with a csproj-based sample that supports net8.0, net9.0, and net10.0.
- Aggregated generated methods into a single generated source file.
- Updated README files for pre-release consistency and usage guidance.

### Fixed
- Fixed duplicate generated source headers in aggregated generator output.
- Removed unnecessary System.Memory package references.
- Fixed `--syntax-increment-current` option registration in Interpreter CLI.

### Package Notes
- Generator: aggregation output and header deduplication updates.
- Interpreter: new tests, CLI option registration fix, and tool E2E CI coverage.
- Parser: README output text typo corrections.
- Processor: README sample updated to use `BrainfuckProcessor`.
