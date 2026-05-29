using Esolang.Brainfuck.Processor.SequenceCommands;
using Esolang.Processor;
using System.Diagnostics;
using System.Text;

namespace Esolang.Brainfuck.Processor;

/// <summary>
/// Runner that executes Brainfuck instruction sequences.
/// </summary>
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public sealed partial class BrainfuckProcessor
{
    readonly ReadOnlyMemory<BrainfuckSequence> Program;
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
    /// Initializes the processor from instruction sequences.
    /// </summary>
    /// <param name="sequences">The instruction sequences to execute.</param>
    public BrainfuckProcessor(ReadOnlyMemory<BrainfuckSequence> sequences)
        => Program = sequences;

    /// <summary>
    /// Deconstructs and returns internal state.
    /// </summary>
    /// <param name="sequences">The instruction sequences.</param>
    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences)
        => sequences = Program;

    /// <summary>
    /// Runs synchronously from the default context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    public BrainfuckContext Run(CancellationToken cancellationToken = default) => Run(null, cancellationToken);

    /// <summary>
    /// Runs synchronously from the specified context.
    /// </summary>
    /// <param name="context">The starting context. If <see langword="null"/>, uses the default context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    public BrainfuckContext Run(BrainfuckContext? context = null, CancellationToken cancellationToken = default) => InternalRun(context ?? Context, cancellationToken);

    /// <summary>
    /// Runs asynchronously from the default context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    public ValueTask<BrainfuckContext> RunAsync(CancellationToken cancellationToken = default) => RunAsync(null, cancellationToken);

    /// <summary>
    /// Runs asynchronously from the specified context.
    /// </summary>
    /// <param name="context">The starting context. If <see langword="null"/>, uses the default context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    public ValueTask<BrainfuckContext> RunAsync(BrainfuckContext? context, CancellationToken cancellationToken = default) => InternalRunAsync(context ?? Context, cancellationToken);

    /// <summary>
    /// Enumerates executable commands from the current context.
    /// </summary>
    /// <returns>The sequence of executable commands.</returns>
    public IEnumerable<SequenceCommand> StepCommands() => InternalStepCommands(Context);
    static async ValueTask<BrainfuckContext> InternalRunAsync(BrainfuckContext context, CancellationToken cancellationToken = default)
    {
        var lastContext = context;
        foreach (var command in InternalStepCommands(context))
        {
            lastContext = await command.ExecuteAsync(cancellationToken);
        }
        return lastContext;
    }
    static BrainfuckContext InternalRun(BrainfuckContext context, CancellationToken cancellationToken)
    {
        var lastContext = context;
        foreach (var command in InternalStepCommands(context))
        {
            lastContext = command.Execute(cancellationToken);
        }
        return lastContext;
    }

    internal static IEnumerable<SequenceCommand> InternalStepCommands(BrainfuckContext context)
    {
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            var before = context;
            var command2 = new SequenceCommand(command);
            yield return command2;
            if (command2 is not (_, { } executed)) throw new InvalidOperationException($"required {nameof(command2.ExecuteAsync)}() or {nameof(command2.Execute)}() call.");
            context = executed;
        }
    }

    /// <summary>
    /// Runs the processor and returns output as a UTF-8 string.
    /// </summary>
    /// <param name="input">Optional input string for Input commands.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The output string, or <see langword="null"/> when output is empty.</returns>
    public async ValueTask<string?> RunAndOutputStringAsync(string? input = null, CancellationToken cancellationToken = default)
    {
        var output = new StringBuilder();
        var context = Context;
        var inputIndex = 0;

        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            if (command.IsIoCommand)
            {
                var ioEvent = await command.GetIoEventAsync(cancellationToken);
                
                if (ioEvent is InputCharEvent inputCharEvent)
                {
                    if (inputIndex < (input?.Length ?? 0))
                    {
                        inputCharEvent.Write(input![inputIndex++]);
                    }
                    else
                    {
                        // Handle end of input or throw/default? 
                        // For now, assuming EOF might be needed or just stop.
                        // Based on Brainfuck standards, might need a value for EOF (e.g., 0 or -1).
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

                context = await command.ExecuteAsync(ioEvent ?? throw new InvalidOperationException("ioEvent is null"), cancellationToken);
            }
            else
            {
                context = await command.ExecuteAsync(cancellationToken);
            }
        }

        return output.Length == 0 ? null : output.ToString();
    }
    /// <summary>
    /// Runs the processor and returns output as a UTF-8 string.
    /// </summary>
    /// <returns>The output string, or <see langword="null"/> when output is empty.</returns>
    public string? RunAndOutputString()
    {
        var output = new StringBuilder();
        var context = Context;

        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            var ioEvent = command.GetIoEventAsync(CancellationToken.None).AsTask().Result;
            if (ioEvent is OutputCharEvent outputChar)
            {
                output.Append(outputChar.Output);
            }
            else if (ioEvent is OutputIntEvent outputInt)
            {
                output.Append(outputInt.Output);
            }

            context = command.Execute(CancellationToken.None);
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
