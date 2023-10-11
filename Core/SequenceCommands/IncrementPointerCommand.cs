namespace Brainfuck.Core.SequenceCommands;

public class IncrementPointerCommand : BrainfuckSequenceCommand
{
    public IncrementPointerCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        var stack = sequencesIndex < context.Stack.Count 
            ? context.Stack 
            : context.Stack.Add(0);
        var stackIndex = context.StackIndex + 1;
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: stack, stackIndex: stackIndex,
            input: context.Input, output: context.Output
        ));
    }
}