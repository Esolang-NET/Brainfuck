namespace Brainfuck.Core.SequenceCommands;

public class CommentCommand : BrainfuckSequenceCommand
{
    public CommentCommand(BrainfuckContext context) : base(context)
    {
    }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: context.Stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        ));
    }
}