using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;

namespace Esolang.Brainfuck.Runner;

/// <summary>
/// brainfuck runner context
/// </summary>
/// <param name="Sequences"></param>
/// <param name="Stack"></param>
/// <param name="SequencesIndex"></param>
/// <param name="StackIndex"></param>
/// <param name="Input"></param>
/// <param name="Output"></param>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public readonly record struct BrainfuckContext(ReadOnlyMemory<BrainfuckSequence> Sequences, ImmutableArray<byte> Stack, int SequencesIndex = default, int StackIndex = default, PipeReader? Input = default, PipeWriter? Output = default) : IEquatable<BrainfuckContext>
{
    public readonly bool IsEmpty
        => ReadOnlyMemory<BrainfuckSequence>.Empty.Equals(Sequences)
        && SequencesIndex == 0
        && (Stack == default || Stack.IsEmpty)
        && StackIndex == 0
        && Input == null
        && Output == null;
    bool PrintMembers(StringBuilder builder)
    {
        if (IsEmpty)
            return false;
        builder.Append(nameof(Sequences) + "= [");
        builder.Append(string.Join(", ", Sequences.ToArray()));
        builder.Append("], " + nameof(SequencesIndex) + "=");
        builder.Append(SequencesIndex);
        builder.Append(", " + nameof(Stack) + "= [");
        builder.Append(string.Join(", ", Stack));
        builder.Append("], " + nameof(StackIndex) + "= ");
        builder.Append(StackIndex);
        builder.Append(", " + nameof(Input) + "=");
        builder.Append(Input);
        builder.Append(", " + nameof(Output) + "=");
        builder.Append(Output);
        return true;
    }
    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(BrainfuckContext) + " { ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }

    private string DebuggerDisplay => ToString();
    public bool Equals(BrainfuckContext other)
    {
        {
            var s1IsEmpty = Sequences.Equals(default);
            var s2IsEmpty = other.Sequences.Equals(default);
            if (s1IsEmpty != s2IsEmpty) return false;
            if (!s1IsEmpty && !s2IsEmpty && !MemoryMarshal.Cast<BrainfuckSequence, int>(Sequences.Span).SequenceEqual(MemoryMarshal.Cast<BrainfuckSequence, int>(other.Sequences.Span))) return false;
        }
        {
            var s1IsEmpty = Stack == default || Stack.IsEmpty;
            var s2IsEmpty = other.Stack == default || other.Stack.IsEmpty;
            if (s1IsEmpty != s2IsEmpty) return false;
            if (!s1IsEmpty && !s2IsEmpty && !Stack.SequenceEqual(other.Stack)) return false;
        }
        return
            SequencesIndex == other.SequencesIndex
            && StackIndex == other.StackIndex
            && Equals(Input, other.Input)
            && Equals(Output, other.Output);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Sequences);
        hash.Add(SequencesIndex);
        hash.Add(Stack);
        hash.Add(StackIndex);
        hash.Add(Input);
        hash.Add(Output);
        return hash.ToHashCode();
    }
}
