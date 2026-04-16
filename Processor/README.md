# Brainfuck.Processor

brainfuck processor.

## usage

```cs
using System;
using Esolang.Brainfuck.Processor;

var source = "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.";
var processor = new BrainfuckProcessor(source);
var result = await processor.RunAndOutputStringAsync();

Console.WriteLine(result);
// output:
// Hello, world!
```
