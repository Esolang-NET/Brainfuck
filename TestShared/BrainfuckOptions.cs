using System.Runtime.Serialization;

namespace Brainfuck.TestShared;
[Serializable]
public readonly record struct BrainfuckOptions(
        string IncrementPointer = BrainfuckOptionsDefault.IncrementPointer,
        string DecrementPointer = BrainfuckOptionsDefault.DecrementPointer,
        string IncrementCurrent = BrainfuckOptionsDefault.IncrementCurrent,
        string DecrementCurrent = BrainfuckOptionsDefault.DecrementCurrent,
        string Output = BrainfuckOptionsDefault.Output,
        string Input = BrainfuckOptionsDefault.Input,
        string Begin = BrainfuckOptionsDefault.Begin,
        string End = BrainfuckOptionsDefault.End
    ) : IBrainfuckOptions, ISerializable, IEquatable<IBrainfuckOptions>
{
    /// <summary>
    /// brainfuck options
    /// </summary>
    public BrainfuckOptions() : this(IncrementPointer: BrainfuckOptionsDefault.IncrementPointer) { }

    /// <summary>
    /// brainfuck options
    /// </summary>
    /// <param name="options"></param>
    public BrainfuckOptions(IBrainfuckOptions options) : this(
             IncrementPointer: options.IncrementPointer,
             DecrementPointer: options.DecrementPointer,
             IncrementCurrent: options.IncrementCurrent,
             DecrementCurrent: options.DecrementCurrent,
             Output: options.Output,
             Input: options.Input,
             Begin: options.Begin,
             End: options.End
        )
    { }

    /// <summary>
    /// brainfuck options
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public BrainfuckOptions(SerializationInfo info, StreamingContext context)
        : this(
             IncrementPointer: info.GetString(nameof(IncrementPointer)) ?? string.Empty,
             DecrementPointer: info.GetString(nameof(DecrementPointer)) ?? string.Empty,
             IncrementCurrent: info.GetString(nameof(IncrementCurrent)) ?? string.Empty,
             DecrementCurrent: info.GetString(nameof(DecrementCurrent)) ?? string.Empty,
             Output: info.GetString(nameof(Output)) ?? string.Empty,
             Input: info.GetString(nameof(Input)) ?? string.Empty,
             Begin: info.GetString(nameof(Begin)) ?? string.Empty,
             End: info.GetString(nameof(End)) ?? string.Empty
        )
    { }

    bool IEquatable<IBrainfuckOptions>.Equals(IBrainfuckOptions? other)
        => other is not null
        && IncrementPointer == other.IncrementPointer
        && DecrementPointer == other.DecrementPointer
        && IncrementCurrent == other.IncrementCurrent
        && DecrementCurrent == other.DecrementCurrent
        && Output == other.Output
        && Input == other.Input
        && Begin == other.Begin
        && End == other.End;

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(IncrementPointer), IncrementPointer);
        info.AddValue(nameof(DecrementPointer), DecrementPointer);
        info.AddValue(nameof(IncrementCurrent), IncrementCurrent);
        info.AddValue(nameof(DecrementCurrent), DecrementCurrent);
        info.AddValue(nameof(Output), Output);
        info.AddValue(nameof(Input), Input);
        info.AddValue(nameof(Begin), Begin);
        info.AddValue(nameof(End), End);
    }
}
