# Development Guide

## Testing

This project uses the Microsoft Testing Platform (MTP).

### Running Tests

To run all tests in the solution:

```bash
dotnet test
```

### Collecting Code Coverage

To run tests and collect code coverage:

```bash
dotnet test --coverage --coverage-output-format cobertura
```

To generate an HTML coverage report using ReportGenerator:

```bash
dotnet reportgenerator "-reports:**/*.cobertura.xml" "-targetdir:coveragereport" -reporttypes:Html
```
The report will be generated in the `coveragereport` directory.

---

## Visual Basic Generator Implementation Plan

1.  **Project Setup**: Initialize `Esolang.Brainfuck.Generator.VisualBasic` and `Esolang.Brainfuck.Generator.VisualBasic.Tests` projects. (Completed)
2.  **Infrastructure**: Define `BrainfuckMethodAttribute` and basic `BrainfuckGenerator` registration. (Completed)
3.  **Testing**: Resolve MTP-based test discovery issues for VB.NET.
4.  **Core Logic**: 
    - Implement `MethodGenerator` logic (VB.NET syntax emitter).
    - Port or adapt `Parser` and `Processor` logic where necessary.
5.  **Verification**: Implement functional tests for attribute detection and code generation correctness.
