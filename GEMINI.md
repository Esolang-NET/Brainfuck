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

## Coding Standards

- **Raw String Literals**: Always use C# 11+ raw string literals (`""" ... """`) for multi-line string definitions, including source code snippets in tests and generated code templates. This improves readability by eliminating the need to escape double quotes and improves handling of indentation.

---

## Remaining Tasks for Visual Basic Generator

1.  **Parameter Validation Logic**: Implement the full suite of diagnostic validations (BF0001, BF0003-BF0010) in `BrainfuckGenerator.cs` to match the C# generator's behavior.
2.  **Core Code Emission**: Complete the `VisualBasicEmitter` implementation for all Brainfuck instructions (`Output`, `Input`, `Begin`, `End`), translating them into valid VB.NET syntax.
3.  **Functional Testing**: Add functional integration tests for the generated VB.NET code, verifying output with `TextWriter`/`TextReader` and execution behavior.
4.  **Style Compliance**: Ensure generated code follows VB.NET coding standards and `dotnet format`.
