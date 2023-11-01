namespace Brainfuck;

/// <summary>
/// brainfuck sequence types
/// </summary>
public enum BrainfuckSequence : byte
{
    /// <summary>others. no operation.</summary>
    Comment = default,
    /// <summary>Increment the data pointer by one (to point to the next cell to the right).</summary>
    IncrementPointer,
    /// <summary>Decrement the data pointer by one (to point to the next cell to the left).</summary>
    DecrementPointer,
    /// <summary>Increment the byte at the data pointer by one.</summary>
    IncrementCurrent,
    /// <summary>Decrement the byte at the data pointer by one.</summary>
    DecrementCurrent,
    /// <summary>Output the byte at the data pointer.</summary>
    Output,
    /// <summary>Accept one byte of input, storing its value in the byte at the data pointer.</summary>
    Input,
    /// <summary>If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.</summary>
    Begin,
    /// <summary>If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.</summary>
    End,
}
