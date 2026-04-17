# Esolang.Brainfuck.Generator

Brainfuck source generator for .NET.

## Changelog

- [Repository changelog](../CHANGELOG.md)

## Install

```bash
dotnet add package Esolang.Brainfuck.Generator
```

## Usage

Use `GenerateBrainfuckMethodAttribute` on a `partial` method.

```cs
using Esolang.Brainfuck;

Console.WriteLine(BrainfuckSample.SampleMethod1());

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial string? SampleMethod1();
}

// output:
// 0123456789
```

## Features

- Determines whether input/output interfaces are required from the source.
- Supports synchronous and asynchronous method signatures.
- Supports string and `System.IO.Pipelines` based input/output patterns.

## Supported Method Signatures

| Category | Supported types |
| --- | --- |
| Input parameter | `string`, `System.IO.Pipelines.PipeReader` |
| Output parameter | `System.IO.Pipelines.PipeWriter` |
| Return type | `void`, `string`, `System.Threading.Tasks.Task<string>`, `System.Threading.Tasks.ValueTask<string>`, `System.Collections.Generic.IEnumerable<byte>`, `System.Collections.Generic.IAsyncEnumerable<byte>` |
| Other parameter | `System.Threading.CancellationToken` |

Output-related signatures are allowed even when the source does not contain `.`. In that case string returns produce `null`, byte-sequence returns complete without values, and `PipeWriter` parameters are simply left unused.

## Signature Patterns

The generator accepts one input source, one output destination, and an optional `CancellationToken`.

### 1. Return string directly

```cs
using Esolang.Brainfuck;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial string? Digits();
}
```

Use when the Brainfuck source outputs bytes and you want the result as a string.

### 2. Async return (`Task<string?>` / `ValueTask<string?>`)

```cs
using Esolang.Brainfuck;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("1+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.")]
    public static partial Task<string?> HelloWorldAsync();

    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial ValueTask<string?> DigitsValueTaskAsync();
}
```

Use when the caller wants async-compatible signatures.

### 3. Input from `string`

```cs
using Esolang.Brainfuck;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod(",.")]
    public static partial string? ReadOneChar(string input);
}
```

Use when input is already in memory as text.

### 4. Input from `PipeReader`

```cs
using Esolang.Brainfuck;
using System.IO.Pipelines;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod(",.")]
    public static partial Task<string?> ReadOneCharFromPipeAsync(PipeReader input, CancellationToken cancellationToken = default);
}
```

Use when input comes from a stream/pipeline (network, process, etc.).

### 5. Output to `PipeWriter`

```cs
using Esolang.Brainfuck;
using System.IO.Pipelines;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial Task WriteDigitsToPipeAsync(PipeWriter output, CancellationToken cancellationToken = default);
}
```

Use when you want to push output bytes to a pipeline sink instead of returning a string.

### 6. Byte sequence return

```cs
using Esolang.Brainfuck;

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("+++++.")]
    public static partial System.Collections.Generic.IEnumerable<byte> GetBytes();

    [GenerateBrainfuckMethod("+++++.")]
    public static partial System.Collections.Generic.IAsyncEnumerable<byte> GetBytesAsync();
}
```

Use when the consumer needs raw bytes and controls text decoding.

## Combination Rules

- Do not combine `string` return (`string` / `Task<string>` / `ValueTask<string>`) with a `PipeWriter` parameter.
- Do not combine `string` input and `PipeReader` input in the same method.
- Use at most one of each special parameter kind (`string`, `PipeReader`, `PipeWriter`, `CancellationToken`).
- If source contains `,`, one input parameter (`string` or `PipeReader`) is required.
- If source contains `.`, one output target is required: `string`, `Task<string>`, `ValueTask<string>`, `IEnumerable<byte>`, `IAsyncEnumerable<byte>`, or `PipeWriter`.
- If source does not contain `.`, output-related signatures are still valid and produce no output.
- If source does not contain `,`, input parameters are still accepted but produce BF0009 (Hidden).

## UseConsole Sample Run

You can run the sample project that includes all main signature patterns:

```bash
dotnet run --project samples/Generator.UseConsole/Esolang.Brainfuck.Generator.UseConsole.csproj --framework net8.0
```

Current sample methods in `samples/Generator.UseConsole/Esolang.Brainfuck.Generator.UseConsole.cs` include:

- `Task<string?>` return pattern.
- `string` input pattern.
- `PipeReader` input pattern.
- `PipeWriter` output pattern.
- Custom Brainfuck token mapping pattern.

Example output:

```text
SampleMethod1: 0123456789
SampleMethod2: Hello, world!
SampleMethod3: A
SampleMethod4: Z
SampleMethod5: 0123456789
```

## Diagnostics

| ID | Meaning |
| --- | --- |
| BF0001 | Invalid value parameter on attribute. |
| BF0002 | Unsupported return type. |
| BF0003 | Unsupported parameter type. |
| BF0004 | Duplicate unsupported parameter pattern. |
| BF0005 | Unsupported input parameter combination (`PipeReader` and `string`). |
| BF0006 | Unsupported parameter and return type combination. |
| BF0007 | Source requires output interface (`string`/`Task<string>`/`ValueTask<string>` return or `PipeWriter` parameter). |
| BF0008 | Source requires input interface (`string` or `PipeReader` parameter). |
| BF0009 | Input parameter provided but source does not contain the input command (Hidden). |
