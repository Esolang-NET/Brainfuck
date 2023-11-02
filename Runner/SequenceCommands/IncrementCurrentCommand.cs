namespace Esolang.Brainfuck.Runner.SequenceCommands;

public sealed record IncrementCurrentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => IncrementCurrent();

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(IncrementCurrent());
    }
    BrainfuckContext IncrementCurrent()
    {
        var sequencesIndex = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        current++;
        var stack = Context.Stack.SetItem(Context.StackIndex, current);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
}
