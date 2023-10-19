namespace Brainfuck.Core.SequenceCommands;

public record CommentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute() => Next();

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(Next());
    }

}
