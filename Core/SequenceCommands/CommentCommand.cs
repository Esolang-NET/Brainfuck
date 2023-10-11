namespace Brainfuck.Core.SequenceCommands;

public class CommentCommand : BrainfuckSequenceCommand
{
    public CommentCommand(BrainfuckContext context) : base(context)
    {
    }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(Next());
    }
}