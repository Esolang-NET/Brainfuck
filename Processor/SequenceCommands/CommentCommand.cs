namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes an instruction treated as a comment/no-op.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record CommentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => Next();

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(Next());
    }

}
