# Project Status: Brainfuck Processor IEventProcessor Migration

## Status
- **Phase**: BrainfuckProcessor refactoring to `IEventProcessor`.
- **Commit**: `a4adc30` (Last known stable build).
- **Current State**: Clean build, tests passing on stable code.

## Completed Tasks
- Implemented `Esolang.Generator.Abstractions` with `KnownTypes` and `TypeResolutionExtensions` for standardized type resolution in source generators.
- Integrated `KnownTypes` into `Esolang.Brainfuck.Generator` by copying sources to ensure repository independence.
- Established `BrainfuckProcessor` as a partial class implementing `IEventProcessor`.

## Known Issues & Next Steps
1. **RunAsyncEnumerable Implementation**:
   - The implementation of `RunAsyncEnumerable` in `BrainfuckProcessor.IEventProcessor.cs` is incomplete and currently reverted to a stable stub/buildable state.
   - The challenge is correctly mapping sequential Brainfuck commands to `IOEvent` streams while maintaining `BrainfuckContext` (specifically `Input` and `Output` streams) across `yield return` points.
   - Current implementation attempts cause `InvalidOperationException: required context.Output.` during testing.

2. **Refactoring Strategy**:
   - Refactor `RunAsyncEnumerable` to properly manage `BrainfuckContext` when yielding events.
   - Ensure that `InputCommand` and `OutputCommand` execution preserves the `Input`/`Output` streams in the updated context.

3. **Testing**:
   - Once implementation is stable, fix and pass `BrainfuckProcessorEventTests` (`RunAsyncEnumerable_ProducesOutputEvents`, `RunAsyncEnumerable_HandlesInputEvent`).
   - Afterward, adapt `Interpreter` and remaining tests to the new `IEventProcessor` model.
