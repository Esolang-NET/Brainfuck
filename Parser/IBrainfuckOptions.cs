namespace Esolang.Brainfuck;

/// <summary>
/// brainfuck options interface
/// </summary>
public interface IBrainfuckOptions
{
    /// <summary>
    /// Increment the data pointer by one (to point to the next cell to the right).
    /// </summary>
    string IncrementPointer { get; }
    /// <summary>
    /// Decrement the data pointer by one (to point to the next cell to the left).
    /// </summary>
    string DecrementPointer { get; }
    /// <summary>
    /// Increment the byte at the data pointer by one.
    /// </summary>
    string IncrementCurrent { get; }
    /// <summary>
    /// Decrement the byte at the data pointer by one.
    /// </summary>
    string DecrementCurrent { get; }
    /// <summary>
    /// Output the byte at the data pointer.
    /// </summary>
    string Output { get; }
    /// <summary>
    /// Accept one byte of input, storing its value in the byte at the data pointer.
    /// </summary>
    string Input { get; }
    /// <summary>
    /// If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
    /// </summary>
    string Begin { get; }
    /// <summary>
    /// If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.
    /// </summary>
    string End { get; }
}
