namespace Esolang.Brainfuck.Generator.Sequences;

internal sealed record Sequence(int Index, BrainfuckSequence Value, ReadOnlyMemory<char> Syntax) : INestableSequence;
