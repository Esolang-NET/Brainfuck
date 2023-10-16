using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public record InputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override bool RequiredInput => true;
    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var (sequencesIndex, stack) = await Input(cancellationToken);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
    async ValueTask<(int SequencesIndex, ImmutableList<byte> Stack)> Input(CancellationToken cancellationToken = default)
    {
        var sequencesIndex = Context.SequencesIndex + 1;
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        var result = await Context.Input.ReadAtLeastAsync(1, cancellationToken);
        var buffer = result.Buffer;
        byte current;
        if (buffer.Length > 0)
        {
            var readableSeq = buffer.Slice(buffer.Start, 1);
            current = readableSeq.First.Span[0];
            Context.Input.AdvanceTo(readableSeq.Start, readableSeq.End);
        }
        else
        {
            current = 0;
        }
        var stack = Context.Stack.SetItem(Context.StackIndex, current);
        return (sequencesIndex, stack);
    }
}
