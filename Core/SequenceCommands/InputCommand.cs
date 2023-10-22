using System.Buffers;
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
        var memory = new byte[1].AsMemory();
        if ((await Context.Input.ReadAtLeastAsync(memory.Length, cancellationToken)) is { } result)
            TryReadWriteFromResult(Context.Input, result, ref memory);
        var stack = Context.Stack.SetItem(Context.StackIndex, memory.Span[0]);
        return (sequencesIndex, stack);
    }
    (int SequenceIndex, ImmutableArray<byte> Stack) Input()
    {
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        var sequencesIndex = Context.SequencesIndex + 1;
        var memory = new byte[1].AsMemory();
        if (Context.Input.TryRead(out var result))
            TryReadWriteFromResult(Context.Input, result, ref memory);
        var stack = Context.Stack.SetItem(Context.StackIndex, memory.Span[0]);
        return (sequencesIndex, stack);
    }

    static bool TryReadWriteFromResult(PipeReader reader, ReadResult result, ref Memory<byte> dest)
    {
        var buffer = result.Buffer;
        var readableSeq = buffer.IsEmpty ? buffer : buffer.Slice(buffer.Start, dest.Length);
        if (readableSeq.Length > 0) readableSeq.CopyTo(dest.Span);
        reader.AdvanceTo(readableSeq.End);
        return readableSeq.Length == dest.Length;
    }

}
