namespace Brainfuck;
/// <summary>
/// brainfuck options
/// </summary>
public class BrainfuckOptions
{
    /// <summary>
    /// brainfuck options
    /// </summary>
    public BrainfuckOptions() { }
    /// <summary>
    /// brainfuck options create from options.
    /// </summary>
    /// <param name="options">origin</param>
    public BrainfuckOptions(BrainfuckOptions options)
    {
        IncrementPointer = options.IncrementPointer;
        DecrementPointer = options.DecrementPointer;
        IncrementCurrent = options.IncrementCurrent;
        DecrementCurrent = options.DecrementCurrent;
        Output = options.Output;
        Input = options.Input;
        Begin = options.Begin;
        End = options.End;
    }
    /// <summary>Increment the data pointer by one (to point to the next cell to the right).</summary>
    public string IncrementPointer = BrainfuckOptionsDefault.IncrementPointer;
    /// <summary>Decrement the data pointer by one (to point to the next cell to the left).</summary>
    public string DecrementPointer = BrainfuckOptionsDefault.DecrementPointer;
    /// <summary>Increment the byte at the data pointer by one.</summary>
    public string IncrementCurrent = BrainfuckOptionsDefault.IncrementCurrent;
    /// <summary>Decrement the byte at the data pointer by one.</summary>
    public string DecrementCurrent = BrainfuckOptionsDefault.DecrementCurrent;
    /// <summary>Output the byte at the data pointer.</summary>
    public string Output = BrainfuckOptionsDefault.Output;
    /// <summary>Accept one byte of input, storing its value in the byte at the data pointer.</summary>
    public string Input = BrainfuckOptionsDefault.Input;
    /// <summary>If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.</summary>
    public string Begin = BrainfuckOptionsDefault.Begin;
    /// <summary>If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.</summary>
    public string End = BrainfuckOptionsDefault.End;
}
