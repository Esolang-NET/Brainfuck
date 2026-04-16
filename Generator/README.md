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
