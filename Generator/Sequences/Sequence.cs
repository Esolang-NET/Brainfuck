namespace Esolang.Brainfuck.Generator.Sequences;

sealed record Sequence(int Index, BrainfuckSequence Value, ReadOnlyMemory<char> Syntax) : INestableSequence;
