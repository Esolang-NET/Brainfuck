using System.Diagnostics.CodeAnalysis;

namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Base type for commands that execute one Brainfuck instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public abstract record class BrainfuckSequenceCommand(BrainfuckContext Context)
{
    /// <summary>
    /// The current execution context.
    /// </summary>
    protected readonly BrainfuckContext Context = Context;
    /// <summary>
    /// <see cref="Context"/> need Input
    /// </summary>
    public virtual bool RequiredInput => false;
    /// <summary>
    /// <see cref="Context"/> need Output
    /// </summary>
    public virtual bool RequiredOutput => false;
    /// <summary>
    /// Executes this command asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    abstract public ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes this command synchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context after execution.</returns>
    abstract public BrainfuckContext Execute(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an executable command from the instruction at the current position.
    /// </summary>
    /// <param name="context">The context to inspect.</param>
    /// <param name="command">The generated command.</param>
    /// <returns><see langword="true"/> if a command was created; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCommand(BrainfuckContext context, [NotNullWhen(true)] out BrainfuckSequenceCommand command)
    {
        command = default!;
        if (context.Sequences.Length <= context.SequencesIndex)
            return false;
        var sequence = context.Sequences.Span[context.SequencesIndex];
        command = sequence switch
        {
            BrainfuckSequence.IncrementPointer => new IncrementPointerCommand(context),
            BrainfuckSequence.DecrementPointer => new DecrementPointerCommand(context),
            BrainfuckSequence.IncrementCurrent => new IncrementCurrentCommand(context),
            BrainfuckSequence.DecrementCurrent => new DecrementCurrentCommand(context),
            BrainfuckSequence.Output => new OutputCommand(context),
            BrainfuckSequence.Input => new InputCommand(context),
            BrainfuckSequence.Begin => new BeginCommand(context),
            BrainfuckSequence.End => new EndCommand(context),
            BrainfuckSequence.Comment or _ => new CommentCommand(context),
        };
        return true;
    }
    /// <summary>
    /// Returns a context advanced by one instruction.
    /// </summary>
    /// <returns>The context at the next instruction index.</returns>
    protected BrainfuckContext Next()
    {
        var sequencesIndex = Context.SequencesIndex + 1;
        return Context with
        {
            SequencesIndex = sequencesIndex,
        };

    }
    /// <summary>
    /// <see cref="BrainfuckContext"/> → <see cref="BrainfuckSequenceCommand"/>
    /// </summary>
    /// <param name="Command">The source command.</param>
    public static implicit operator BrainfuckContext(BrainfuckSequenceCommand Command) => Command.Context;
    /// <summary>
    /// <see cref="BrainfuckSequenceCommand"/> → <see cref="BrainfuckContext"/>
    /// </summary>
    /// <param name="Context">The source context.</param>
    public static implicit operator BrainfuckSequenceCommand?(BrainfuckContext Context) => TryGetCommand(Context, out var command) ? command : null;
}
