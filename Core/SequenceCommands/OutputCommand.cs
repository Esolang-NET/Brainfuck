namespace Brainfuck.Core.SequenceCommands;

public class OutputCommand : BrainfuckSequenceCommand
{
    public OutputCommand(BrainfuckContext context) : base(context)
    {
    }
    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        if (context.Output is null) throw new InvalidOperationException("required context.Output.");
        var current = context.Stack[context.StackIndex];
        var memory = new byte[] { current }.AsMemory();
        await context.Output.WriteAsync(memory, cancellationToken);
        return context with
        {
            SequencesIndex = sequencesIndex,
        };
    }
}