namespace Brainfuck.Core.SequenceCommands;

public record OutputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override bool RequiredOutput => true;
    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Context.Output is null) throw new InvalidOperationException("required context.Output.");
        var sequencesIndex = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        var memory = new byte[] { current }.AsMemory();
        await Context.Output.WriteAsync(memory, cancellationToken);
        return Context with
        {
            SequencesIndex = sequencesIndex,
        };
    }
}