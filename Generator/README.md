# Brainfuck.Generator

brainfuck method generator.

## usage

use `Esolang.Brainfuck.GenerateBrainfuckMethodAttribute` attribute.

first argument is brainfuck source code.

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

## feature

- Determine whether input or output is possible based on the source.
- Both synchronous and asynchronous usage are supported.
- Both binary (UTF8) input and output and string input and output are supported.

## support type

- input
  - parameter type
    - `System.IO.Pipelines.PipeReader`
    - `string`
- output
  - parameter type
    - `System.IO.Pipelines.PipeWriter`
  - return type
    - `string`
    - `System.Threading.Tasks.Task<string>`
    - `System.Threading.Tasks.ValueTask<string>`
    - `System.Collections.Generic.IEnumerable<byte>`
    - `System.Collections.Generic.IAsyncEnumerable<byte>`
- other support type
  - parameter type
    - `System.Threading.CancellationToken`
  - return type
    - `void`
