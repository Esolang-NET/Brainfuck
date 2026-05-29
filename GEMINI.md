# Project Status: Brainfuck Processor I/O Model Refactoring

## Status
- **Phase**: Completed event-based I/O refactoring.
- **Commit**: `HEAD`
- **Current State**: Clean build, tests passing.

## Completed Tasks
- Implemented `Esolang.Generator.Abstractions` with `KnownTypes` and `TypeResolutionExtensions` for standardized type resolution in source generators.
- Integrated `KnownTypes` into `Esolang.Brainfuck.Generator` by copying sources to ensure repository independence.
- Established `BrainfuckProcessor` as a partial class implementing `IEventProcessor`.
- Refactored `BrainfuckProcessor` and `BrainfuckContext` to remove `System.IO.Pipelines` (`PipeReader`/`PipeWriter`) dependencies.
- Transitioned all `SequenceCommands` (`Input`, `Output`) to a pure event-based I/O model using `IOEvent`.
- Optimized the `BrainfuckProcessor` execution loop for event-based state transitions.
- Updated and verified the `Processor.Tests` suite to match the new architecture.

## Future Design Considerations

### Generator I/O Extensibility (Cross-Project)
- **Current Approach**: Simple delegate-based I/O (`Func<char>`, `Action<char>`) is sufficient for basic requirements in individual generators.
- **Future Consideration**: As requirements grow across the `Esolang.*.Generator` projects, we will need to ensure a consistent approach to I/O extensibility. This may involve introducing attribute-based I/O definition (e.g., `[EsolangInput]`, `[EsolangOutput]`) to explicitly specify delegate roles, support different behaviors, or handle multiple I/O streams across different Esolang languages. Architectural decisions here must be coordinated to maintain consistency across all generators.

