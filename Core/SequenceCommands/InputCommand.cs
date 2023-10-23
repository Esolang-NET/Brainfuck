using System.Buffers;
using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Brainfuck.Core.SequenceCommands;

public record InputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override bool RequiredInput => true;

    public override BrainfuckContext Execute()
    {
        if (!TryInput(out var sequencesIndex, out var stack))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }

    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (await InputAsync(cancellationToken) is not (var sequencesIndex, var stack))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
    async ValueTask<(int SequencesIndex, ImmutableArray<byte> Stack)?> InputAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        var sequencesIndex = Context.SequencesIndex + 1;
        var memory = new byte[1].AsMemory();
        if (!((await Context.Input.ReadAtLeastAsync(memory.Length, cancellationToken)) is { } result
            && TryReadWriteFromResult(Context.Input, result, memory.Span)))
            return null;
        var stack = Context.Stack.SetItem(Context.StackIndex, memory.Span[0]);
        return (sequencesIndex, stack);
    }
    bool TryInput(out int sequencesIndex, out ImmutableArray<byte> stack)
    {
        sequencesIndex = default;
        stack = default!;
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        sequencesIndex = Context.SequencesIndex + 1;
        Span<byte> span = stackalloc byte[1];
        if (!Context.Input.TryRead(out var result)) return false;
        if (!TryReadWriteFromResult(Context.Input, result, span)) return false;
        stack = Context.Stack.SetItem(Context.StackIndex, span[0]);
        return true;
    }
    static bool TryReadWriteFromResult(PipeReader reader, ReadResult result, Span<byte> dest)
    {
        var buffer = result.Buffer;
        var readableSeq = buffer.IsEmpty ? buffer : buffer.Slice(buffer.Start, dest.Length);
        if (readableSeq.Length > 0) readableSeq.CopyTo(dest);
        reader.AdvanceTo(readableSeq.End);
        return readableSeq.Length == dest.Length;
    }

}
