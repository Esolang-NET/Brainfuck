namespace Brainfuck.Core.SequenceCommands;

public class InputCommand : BrainfuckSequenceCommand
{
    public InputCommand(BrainfuckContext context) : base(context) { }

    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        if (context.Input is null) throw new InvalidOperationException("required context.Input.");
        var result = await context.Input.ReadAtLeastAsync(1, cancellationToken);
        var buffer = result.Buffer;
        byte current;
        if (buffer.Length <= 0)
        {
            var readableSeq = buffer.Slice(buffer.Start, 1);
            current = readableSeq.First.Span[0];
            context.Input.AdvanceTo(readableSeq.Start, readableSeq.End);
        }
        else
        {
            current = unchecked((byte)-1);
        }
        var stack = context.Stack.SetItem(context.StackIndex, current);
        return new(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        );
    }
}