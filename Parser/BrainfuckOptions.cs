namespace Esolang.Brainfuck;
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
    ) : IBrainfuckOptions, IEquatable<IBrainfuckOptions>
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
}
