namespace Brainfuck.Core.SequenceCommands;

public class DecrementCurrentCommand : BrainfuckSequenceCommand
{
    public DecrementCurrentCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        current--;
        var stack = context.Stack.SetItem(context.StackIndex, current);
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        ));
    }
}