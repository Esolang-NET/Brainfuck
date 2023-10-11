namespace Brainfuck.Core.SequenceCommands;

public class DecrementPointerCommand : BrainfuckSequenceCommand
{
    public DecrementPointerCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        var stackIndex = context.StackIndex - 1;
        if (stackIndex < 0)
            throw new IndexOutOfRangeException(nameof(stackIndex));
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: context.Stack, stackIndex: stackIndex,
            input: context.Input, output: context.Output
        ));
    }
}