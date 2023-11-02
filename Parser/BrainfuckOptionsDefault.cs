namespace Esolang.Brainfuck;

/// <summary>
/// brainfuck options default values.
/// </summary>
public static class BrainfuckOptionsDefault
{
    /// <summary>Increment the data pointer by one (to point to the next cell to the right).</summary>
    public const string IncrementPointer = ">";
    /// <summary>Decrement the data pointer by one (to point to the next cell to the left).</summary>
    public const string DecrementPointer = "<";
    /// <summary>Increment the byte at the data pointer by one.</summary>
    public const string IncrementCurrent = "+";
    /// <summary>Decrement the byte at the data pointer by one.</summary>
    public const string DecrementCurrent = "-";
    /// <summary>Output the byte at the data pointer.</summary>
    public const string Output = ".";
    /// <summary>Accept one byte of input, storing its value in the byte at the data pointer.</summary>
    public const string Input = ",";
    /// <summary>If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.</summary>
    public const string Begin = "[";
    /// <summary>If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.</summary>
    public const string End = "]";
}
