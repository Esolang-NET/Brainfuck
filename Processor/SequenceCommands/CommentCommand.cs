﻿namespace Esolang.Brainfuck.Processor.SequenceCommands;

public sealed record CommentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => Next();

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(Next());
    }

}
