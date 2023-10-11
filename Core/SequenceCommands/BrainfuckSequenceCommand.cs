namespace Brainfuck.Core.SequenceCommands;

public abstract class BrainfuckSequenceCommand
{
    protected readonly BrainfuckContext context;
    public BrainfuckSequenceCommand(BrainfuckContext context) => this.context = context;
    abstract public ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default);
    public static BrainfuckSequenceCommand? GetCommand(BrainfuckContext context)
    {
        if (context.Sequences.Length <= context.SequencesIndex)
            return null;
        var sequence = context.Sequences.Span[context.SequencesIndex];
        return sequence switch
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
    }
}