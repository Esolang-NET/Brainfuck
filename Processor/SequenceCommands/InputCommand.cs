using Esolang.Processor;
using System.Buffers;
using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.Input"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record InputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    private sealed class InputCharEventImpl : InputCharEvent
    {
        private readonly TaskCompletionSource<char> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override void Write(char c) => _tcs.TrySetResult(c);
        public Task<char> Task => _tcs.Task;
    }

    /// <inheritdoc />
    public override bool RequiredInput => true;

    /// <inheritdoc />
    public override bool IsIoCommand => true;

    /// <inheritdoc />
    public override ValueTask<IOEvent?> GetIoEventAsync(CancellationToken ct) => new(new InputCharEventImpl());

    /// <inheritdoc />
    public override async ValueTask<BrainfuckContext> ExecuteAsync(IOEvent ioEvent, CancellationToken ct)
    {
        var inputChar = await ((InputCharEventImpl)ioEvent).Task;
        return Context with
        {
            Stack = Context.Stack.SetItem(Context.StackIndex, (byte)inputChar),
            SequencesIndex = Context.SequencesIndex + 1
        };
    }

    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        if (!TryInput(out var sequencesIndex, out var stack, cancellationToken))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }

    /// <inheritdoc />
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
        var memory = new byte[1].AsMemory();
        if (!((await Context.Input.ReadAtLeastAsync(memory.Length, cancellationToken)) is { } result
            && TryReadWriteFromResult(Context.Input, result, memory.Span)))
            return null;
        var sequencesIndex = Context.SequencesIndex + 1;
        var stack = Context.Stack.SetItem(Context.StackIndex, memory.Span[0]);
        return (sequencesIndex, stack);
    }
    bool TryInput(out int sequencesIndex, out ImmutableArray<byte> stack, CancellationToken cancellationToken)
    {
        sequencesIndex = default;
        stack = default!;
        if (Context.Input is null) throw new InvalidOperationException("required context.Input.");
        Span<byte> span = stackalloc byte[1];
        ReadResult result;
        while (!Context.Input.TryRead(out result))
            if (cancellationToken.IsCancellationRequested) return false;
        if (!TryReadWriteFromResult(Context.Input, result, span)) return false;
        sequencesIndex = Context.SequencesIndex + 1;
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
