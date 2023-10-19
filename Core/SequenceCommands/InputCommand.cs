using System;
using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Brainfuck.Core.SequenceCommands;

public record InputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override bool RequiredInput => true;

    public override BrainfuckContext Execute()
    {
        var (sequencesIndex, stack) = Input();
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }

    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var (sequencesIndex, stack) = await InputAsync(cancellationToken);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
    async ValueTask<(int SequencesIndex, ImmutableArray<byte> Stack)> InputAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        var sequencesIndex = Context.SequencesIndex + 1;
        var result = await Context.Input.ReadAtLeastAsync(1, cancellationToken);
        if (!TryReadFromResult(Context.Input, result, out var current))
            current = 0;
        var stack = Context.Stack.SetItem(Context.StackIndex, current);
        return (sequencesIndex, stack);
    }
    (int SequenceIndex, ImmutableArray<byte> Stack) Input()
    {
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        var sequencesIndex = Context.SequencesIndex + 1;
        if (!(Context.Input.TryRead(out var result) && TryReadFromResult(Context.Input, result, out var current)))
            current = 0;
        var stack = Context.Stack.SetItem(Context.StackIndex, current);
        return (sequencesIndex, stack);
    }
    bool TryReadFromResult(PipeReader reader, ReadResult result, out byte current)
    {
        current = 0;
        var buffer = result.Buffer;
        if (buffer.Length <= 0)
            return false;

        var readableSeq = buffer.Slice(buffer.Start, 1);
        current = readableSeq.First.Span[0];
        reader.AdvanceTo(readableSeq.End);
        return true;
    }

}
