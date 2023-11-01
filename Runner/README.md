# Brainfuck.Runner

brainfuck runner.

## usage

```cs
using System;
using Brainfuck.Runner;

var source = "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.";
var runner = new BrainfuckRunner(source);
var result = await runner.RunAndOutputStringAsync();

Console.WriteLine(result);
// output:
// Hello, world!
```
