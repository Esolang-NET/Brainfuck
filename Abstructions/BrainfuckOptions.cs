using System.Runtime.Serialization;

namespace Brainfuck;
/// <summary>
/// brainfuck options
/// </summary>
/// <param name="IncrementPointer">Increment the data pointer by one (to point to the next cell to the right).</param>
/// <param name="DecrementPointer">Decrement the data pointer by one (to point to the next cell to the left).</param>
/// <param name="IncrementCurrent">Increment the byte at the data pointer by one.</param>
/// <param name="DecrementCurrent">Decrement the byte at the data pointer by one.</param>
/// <param name="Output">Output the byte at the data pointer.</param>
/// <param name="Input">Accept one byte of input, storing its value in the byte at the data pointer.</param>
/// <param name="Begin">If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.</param>
/// <param name="End">If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.</param>
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
    ) : ISerializable
{
    /// <summary>
    /// brainfuck options
    /// </summary>
    public BrainfuckOptions() : this(IncrementPointer: BrainfuckOptionsDefault.IncrementPointer) { }
    public BrainfuckOptions(SerializationInfo info, StreamingContext context)
        : this(
             IncrementPointer: info.GetString(nameof(IncrementPointer))!,
             DecrementPointer: info.GetString(nameof(DecrementPointer))!,
             IncrementCurrent: info.GetString(nameof(IncrementCurrent))!,
             DecrementCurrent: info.GetString(nameof(DecrementCurrent))!,
             Output: info.GetString(nameof(Output))!,
             Input: info.GetString(nameof(Input))!,
             Begin: info.GetString(nameof(Begin))!,
             End: info.GetString(nameof(End))!
        )
    { }

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
