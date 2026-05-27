# Project Status: Brainfuck Processor IEventProcessor Migration

## Status
- **Phase**: BrainfuckProcessor refactoring to `IEventProcessor`.
- **Commit**: `HEAD`
- **Current State**: Clean build, tests passing.

## Completed Tasks
- Implemented `Esolang.Generator.Abstractions` with `KnownTypes` and `TypeResolutionExtensions` for standardized type resolution in source generators.
- Integrated `KnownTypes` into `Esolang.Brainfuck.Generator` by copying sources to ensure repository independence.
- Established `BrainfuckProcessor` as a partial class implementing `IEventProcessor`.
- Refactored `RunAsyncEnumerable` to correctly handle I/O events without requiring `PipeReader`/`PipeWriter` in the context, resolving `InvalidOperationException`.

## Next Steps
1. **Refactoring Strategy**:
   - Afterward, adapt `Interpreter` and remaining tests to the new `IEventProcessor` model if needed (currently all tests pass).
