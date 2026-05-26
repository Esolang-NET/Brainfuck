# Changelog

All notable changes to this repository are documented in this file.

The format is based on Keep a Changelog.

## [Unreleased]

### Added
- `Esolang.Brainfuck.Generator`: Added BF0011 diagnostic for enforcing partial method declaration.

## [1.1.3] - 2026-05-25

### Added
- `Esolang.Brainfuck.Generator`: Implement logging support for runtime instructions and events (instruction execution).
- `Esolang.Brainfuck.Generator.Tests`: Standardized test utility methods and improved diagnostic logging for test failures.

### Improved
- **Generator**: Refactored the Brainfuck source generator to simplify and unify type analysis logic.
  - Introduced `KnownTypes` struct to cache and centralize type symbol resolution during compilation.
  - Replaced fragile string-based type checks with robust `INamedTypeSymbol` comparisons using `SymbolEqualityComparer`.
  - Restored explicit handling of Nullable reference types (e.g., `string?`) in return type analysis to ensure correct code generation.
- Refactored `MethodGenerator.Initialize` to use tuple deconstruction for cleaner input processing.
- Increased robustness of diagnostic reporting by ensuring proper parameter and return type validation.


## [1.1.2] - 2026-05-09

### Changed

- `Esolang.Brainfuck.Generator` now supports `string`, `Task<string>`, and `ValueTask<string>` as return types.

## [1.1.1]

### Changed

- Package metadata: added NuGet `PackageTags` for packable Brainfuck packages (`Generator`, `Parser`, `Processor`, `dotnet-brainfuck`) to improve search/discovery.
- Fixed a Source Generator load failure where the Generator could not resolve `Esolang.Brainfuck.Parser` at build time; added a `buildTransitive` `.targets` file to ensure Parser assemblies are included as analyzers.
- `Esolang.Brainfuck.Generator`: `PT0008` / `PT0007` Severity `Error`→ `Hidden`

## [1.1.0] - 2026-05-08

### Added

- `Esolang.Processor.Abstractions` (`Esolang.Processor` namespace): shared execution abstractions package (`IProcessor<TProgram>`, `ITextProcessor<TProgram>`, `IPipeProcessor<TProgram>`).
- `Esolang.Brainfuck.Processor/BrainfuckProcessor.IProcessor.cs`: partial implementation for unified execution interfaces, including text/pipe adapter paths.
- `Esolang.Brainfuck.Processor.Tests`: coverage for `RunToEnd(...)` text I/O and `RunToEndAsync(...)` pipe I/O.

### Changed

- `Esolang.Brainfuck.Processor`: `BrainfuckProcessor` now implements `ITextProcessor<ReadOnlyMemory<BrainfuckSequence>>` and `IPipeProcessor<ReadOnlyMemory<BrainfuckSequence>>`.
- `Esolang.Brainfuck.Processor`: switched abstraction source from local `Processor/IProcessor.cs` to `Esolang.Processor.Abstractions` package.
- `Esolang.Brainfuck.Generator`: added return-type support for `int`, `Task<int>`, and `ValueTask<int>` (returns `0` on normal completion).

## [1.0.0] - 2026-05-06

### Added

- Generator: Added C# language version check (BF0010) to warn if below C# 8.0.
- Generator: Added support for `System.IO.TextReader`/`TextWriter` input/output patterns.
- Generator: Added BF0009 (Hidden) diagnostic for unused input parameters.
- Generator: Significantly expanded samples and test coverage (UseConsole sample, more comprehensive tests).
- CI: Added release workflow to GitHub Actions.

### Changed

- Generator: Diagnostic messages and ID structure unified with the Piet project.
- Generator: Clarified signature validation and combination rules.
- All READMEs: Rewritten in English, reorganized, and expanded with install, usage, and API details.
- Build/package baseline: unified repository `Version` to `1.0.0`, unified `LangVersion` to C# 14, and set `AssemblyVersion` / `FileVersion` to `1.0.0.102`.

### Fixed

- Generator: Improved accuracy of duplicate/invalid input/output parameter detection.
- Tests: Target frameworks now auto-switch for Windows/non-Windows environments.

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

[Unreleased]: https://github.com/Esolang-NET/Brainfuck/compare/v1.1.3...HEAD
[1.1.3]: https://github.com/Esolang-NET/Brainfuck/tree/v1.1.3
[1.1.2]: https://github.com/Esolang-NET/Brainfuck/tree/v1.1.2
[1.1.1]: https://github.com/Esolang-NET/Brainfuck/tree/v1.1.1
[1.1.0]: https://github.com/Esolang-NET/Brainfuck/tree/v1.1.0
[1.0.0]: https://github.com/Esolang-NET/Brainfuck/tree/v1.0.0
[0.1.1-preview-1]: https://github.com/Esolang-NET/Brainfuck/tree/v0.1.1-preview-1
