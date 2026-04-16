# Brainfuck.Parser

brainfuck parser.

## changelog

- [Repository changelog](./CHANGELOG.md)

## usage

```cs
using System;
using Esolang.Brainfuck;

var source = "[,+++++.]";
var sequences = new BrainfuckSequenceEnumerable(source);
foreach(var (sequence, syntax) in sequences)
{
    Console.WriteLine($"syntax:{syntax} sequence:{sequence}");
}
// output:
// syntax:[ sequence:Begin
// syntax:, sequence:Input
// syntax:+ sequence:IncrementCurrent
// syntax:+ sequence:IncrementCurrent
// syntax:+ sequence:IncrementCurrent
// syntax:+ sequence:IncrementCurrent
// syntax:+ sequence:IncrementCurrent
// syntax:. sequence:Output
// syntax:] sequence:End
```
