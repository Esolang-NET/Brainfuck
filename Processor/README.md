# Brainfuck.Processor

brainfuck processor.

## usage

```cs
using System;
using Esolang.Brainfuck.Processor;

var source = "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.";
var runner = new BrainfuckRunner(source);
var result = await runner.RunAndOutputStringAsync();

Console.WriteLine(result);
// output:
// Hello, world!
```
