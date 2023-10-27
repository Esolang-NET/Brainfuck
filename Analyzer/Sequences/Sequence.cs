namespace Brainfuck.Analyzer.Sequences;

internal sealed record Sequence(int Index, BrainfuckSequence Value, ReadOnlyMemory<char> Syntax) : INestableSequence;
