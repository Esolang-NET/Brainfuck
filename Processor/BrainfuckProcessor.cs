using System.Diagnostics;
using System.Text;
using static Esolang.Processor.IOEvent;

namespace Esolang.Brainfuck.Processor;

/// <summary>
/// Runner that executes Brainfuck instruction sequences.
/// </summary>
/// <remarks>
/// Initializes the processor from instruction sequences.
/// </remarks>
/// <param name="Program">The instruction sequences to execute.</param>
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public sealed partial class BrainfuckProcessor(ReadOnlyMemory<BrainfuckSequence> Program)
{

    /// <summary>
    /// Deconstructs and returns internal state.
    /// </summary>
    /// <param name="sequences">The instruction sequences.</param>
    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences)
        => sequences = Program;

    BrainfuckContext Context => new(Program, SequencesIndex: 0, Stack: [0], StackIndex: 0);

    /// <summary>
    /// Initializes the processor from source code.
    /// </summary>
    /// <param name="source">The Brainfuck source.</param>
    public BrainfuckProcessor(string source) : this(source, new()) { }

    /// <summary>
    /// Initializes the processor from source code and syntax options.
    /// </summary>
    /// <param name="source">The Brainfuck source.</param>
    /// <param name="sourceOptions">The syntax options.</param>
    public BrainfuckProcessor(string source, IBrainfuckOptions? sourceOptions)
        : this(SourceToSequences(source, sourceOptions)) { }

    /// <summary>
    /// Initializes the processor from source code and syntax options.
    /// </summary>
    /// <param name="source">The Brainfuck source.</param>
    /// <param name="sourceOptions">The syntax options.</param>
    public BrainfuckProcessor(string source, BrainfuckOptions sourceOptions)
        : this(SourceToSequences(source, sourceOptions)) { }
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, IBrainfuckOptions? sourceOptions)
        => new BrainfuckSequenceEnumerable(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions sourceOptions)
        => new BrainfuckSequenceEnumerable(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();

    /// <summary>
    /// Runs the processor and returns output as a UTF-8 string.
    /// </summary>
    /// <param name="input">Optional input string for Input commands.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The output string, or <see langword="null"/> when output is empty.</returns>
    public async ValueTask<string?> RunAndOutputStringAsync(string? input = null, CancellationToken cancellationToken = default)
    {
        var output = new StringBuilder();
        var inputIndex = 0;

        await foreach (var ioEvent in RunAsyncEnumerable(cancellationToken))
        {
            if (ioEvent is InputCharEvent inputCharEvent)
            {
                if (inputIndex < (input?.Length ?? 0))
                {
                    inputCharEvent.Write(input![inputIndex++]);
                }
                else
                {
                    inputCharEvent.Write('\0');
                }
            }
            else if (ioEvent is OutputCharEvent outputChar)
            {
                output.Append(outputChar.Output);
            }
            else if (ioEvent is OutputIntEvent outputInt)
            {
                output.Append(outputInt.Output);
            }
        }

        return output.Length == 0 ? null : output.ToString();
    }

    bool PrintMembers(StringBuilder builder)
    {
        builder.Append(nameof(Program) + " = [");
        builder.Append(string.Join(", ", Program));
        builder.Append(']');
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(BrainfuckProcessor) + " { ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
