# Esolang.Brainfuck.Generator.VisualBasic

A Roslyn source generator that provides compile-time Brainfuck code generation for Visual Basic.

## Architecture

This project is implemented as a **C#** Roslyn Source Generator. Its purpose is to analyze methods decorated with the `GenerateBrainfuckMethod` attribute in a Visual Basic project and emit the corresponding implementation in **Visual Basic** code.

- **Implementation Language**: C# (Roslyn Source Generator)
- **Target Language**: Visual Basic

It leverages the shared logic from the `Parser` and `Processor` projects to parse Brainfuck source code into intermediate representations and then emits valid Visual Basic syntax as a partial method implementation.
