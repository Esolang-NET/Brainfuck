using Esolang.Brainfuck;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using OriginalBrainfuckContext = Esolang.Brainfuck.Processor.BrainfuckContext;

namespace TestShared;

[Serializable]
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public readonly record struct BrainfuckContext(ReadOnlyMemory<BrainfuckSequence> Sequences, ImmutableArray<byte> Stack, int SequencesIndex = default, int StackIndex = default) : ISerializable, IEquatable<BrainfuckContext>
{
    public BrainfuckContext(SerializationInfo info, StreamingContext context) : this(Sequences: default, Stack: default!)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(info);
#else
        if (info is null) throw new ArgumentNullException(nameof(info));
#endif
        Sequences = (info.GetValue(nameof(Sequences), typeof(BrainfuckSequence[])) as BrainfuckSequence[] ?? []).AsMemory();
        SequencesIndex = info.GetInt32(nameof(SequencesIndex));
        Stack = ImmutableArray.Create(info.GetValue(nameof(Stack), typeof(byte[])) as byte[] ?? []);
        StackIndex = info.GetInt32(nameof(StackIndex));
    }
    public bool IsEmpty
        => ReadOnlyMemory<BrainfuckSequence>.Empty.Equals(Sequences)
        && SequencesIndex == 0
        && Stack.IsEmpty
        && StackIndex == 0;
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
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Sequences), Sequences.ToArray(), typeof(BrainfuckSequence[]));
        info.AddValue(nameof(SequencesIndex), SequencesIndex);
        info.AddValue(nameof(Stack), Stack.ToArray(), typeof(byte[]));
        info.AddValue(nameof(StackIndex), StackIndex);
    }
    public bool Equals(BrainfuckContext other)
        => MemoryMarshal.Cast<BrainfuckSequence, int>(Sequences.Span).SequenceEqual(MemoryMarshal.Cast<BrainfuckSequence, int>(other.Sequences.Span))
        && SequencesIndex == other.SequencesIndex
        && Stack.SequenceEqual(other.Stack)
        && StackIndex == other.StackIndex;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Sequences);
        hash.Add(SequencesIndex);
        hash.Add(Stack);
        hash.Add(StackIndex);
        return hash.ToHashCode();
    }
    public static implicit operator OriginalBrainfuckContext(BrainfuckContext context) => new(Sequences: context.Sequences, Stack: context.Stack, SequencesIndex: context.SequencesIndex, StackIndex: context.StackIndex);
    public static implicit operator BrainfuckContext(OriginalBrainfuckContext context) => new(Sequences: context.Sequences, Stack: context.Stack, SequencesIndex: context.SequencesIndex, StackIndex: context.StackIndex);
}
