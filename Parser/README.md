# Brainfuck.Parser

brainfuck parser.

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
// syntax:, sequnece:Input
// syntax:+ sequnece:IncrementCurrent
// syntax:+ sequnece:IncrementCurrent
// syntax:+ sequnece:IncrementCurrent
// syntax:+ sequnece:IncrementCurrent
// syntax:+ sequnece:IncrementCurrent
// syntax:. sequence:Output
// syntax:] sequence:End
```
