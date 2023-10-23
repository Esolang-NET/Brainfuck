using System.Diagnostics.CodeAnalysis;

namespace Brainfuck.Core.SequenceCommands;

public abstract record class BrainfuckSequenceCommand(BrainfuckContext Context)
{
    protected readonly BrainfuckContext Context = Context;
    /// <summary>
    /// <see cref="Context"/> need Input
    /// </summary>
    public virtual bool RequiredInput => false;
    /// <summary>
    /// <see cref="Context"/> need Output
    /// </summary>
    public virtual bool RequiredOutput => false;
    abstract public ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default);
    abstract public BrainfuckContext Execute(CancellationToken cancellationToken = default);
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
    /// <param name="Command"></param>
    public static implicit operator BrainfuckContext(BrainfuckSequenceCommand Command) => Command.Context;
    /// <summary>
    /// <see cref="BrainfuckSequenceCommand"/> → <see cref="BrainfuckContext"/>
    /// </summary>
    /// <param name="Context"></param>
    public static implicit operator BrainfuckSequenceCommand?(BrainfuckContext Context) => TryGetCommand(Context, out var command) ? command : null;
}
