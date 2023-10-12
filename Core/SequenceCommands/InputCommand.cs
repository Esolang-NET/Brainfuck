using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public class InputCommand : BrainfuckSequenceCommand
{
    public InputCommand(BrainfuckContext context) : base(context) { }

    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var (sequencesIndex, stack) = await Input(cancellationToken);
        return context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
    async ValueTask<(int SequencesIndex, ImmutableList<byte> Stack)> Input(CancellationToken cancellationToken = default)
    {
        var sequencesIndex = context.SequencesIndex + 1;
        if (context.Input is null) throw new InvalidOperationException("required context.Input.");
        var result = await context.Input.ReadAtLeastAsync(1, cancellationToken);
        var buffer = result.Buffer;
        byte current;
        if (buffer.Length > 0)
        {
            var readableSeq = buffer.Slice(buffer.Start, 1);
            current = readableSeq.First.Span[0];
            context.Input.AdvanceTo(readableSeq.Start, readableSeq.End);
        }
        else
        {
            current = 0;
        }
        var stack = context.Stack.SetItem(context.StackIndex, current);
        return (sequencesIndex, stack);
    }
}