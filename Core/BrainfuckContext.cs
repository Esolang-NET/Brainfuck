using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Brainfuck;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct BrainfuckContext : ISerializable, IEquatable<BrainfuckContext>
{
    public readonly ReadOnlyMemory<BrainfuckSequence> Sequences { get; }
    public readonly int SequencesIndex { get; }
    public readonly ImmutableList<byte> Stack { get; }
    public readonly int StackIndex { get; }
    public readonly PipeReader? Input { get; }
    public readonly PipeWriter? Output { get; }
    public BrainfuckContext(ReadOnlyMemory<BrainfuckSequence> sequences, ImmutableList<byte> stack, int sequencesIndex = default, int stackIndex = default, PipeReader? input = default, PipeWriter? output = default)
        => (Sequences, Stack, SequencesIndex, StackIndex, Input, Output) = (sequences, stack, sequencesIndex, stackIndex, input, output);
    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences, out ImmutableList<byte> stack, out int sequencesIndex, out int stackIndex, out PipeReader? input, out PipeWriter? output)
        => (sequences, stack, sequencesIndex, stackIndex, input, output) = (Sequences, Stack, SequencesIndex, StackIndex, Input, Output);
    public BrainfuckContext(SerializationInfo info, StreamingContext context)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        Sequences = (info.GetValue(nameof(Sequences), typeof(BrainfuckSequence[])) as BrainfuckSequence[] ?? Array.Empty<BrainfuckSequence>()).AsMemory();
        SequencesIndex = info.GetInt32(nameof(SequencesIndex));
        Stack = ImmutableList.Create(info.GetValue(nameof(Stack), typeof(byte[])) as byte[] ?? Array.Empty<byte>());
        StackIndex = info.GetInt32(nameof(StackIndex));
    }
    public bool IsEmpty
        => ReadOnlyMemory<BrainfuckSequence>.Empty.Equals(Sequences)
        && SequencesIndex == 0
        && Stack.IsEmpty
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

    private string GetDebuggerDisplay() => ToString();
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Sequences), Sequences.ToArray(), typeof(BrainfuckSequence[]));
        info.AddValue(nameof(SequencesIndex), SequencesIndex);
        info.AddValue(nameof(Stack), Stack.ToArray(), typeof(byte[]));
        info.AddValue(nameof(StackIndex), StackIndex);
    }
    public bool Equals(BrainfuckContext other)
        => MemoryMarshal.Cast<BrainfuckSequence, int>(Sequences.Span).SequenceEqual(MemoryMarshal.Cast<BrainfuckSequence, int>( other.Sequences.Span))
        && SequencesIndex == other.SequencesIndex
        && Stack.SequenceEqual(other.Stack)
        && StackIndex == other.StackIndex
        && Equals(Input, other.Input)
        && Equals(Output, other.Output);

    public override bool Equals(object? obj) => obj is BrainfuckContext context && Equals(context);

    public static bool operator ==(BrainfuckContext left, BrainfuckContext right) => left.Equals(right);

    public static bool operator !=(BrainfuckContext left, BrainfuckContext right) => !(left == right);

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