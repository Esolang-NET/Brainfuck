using Esolang.Processor;
using static Esolang.Processor.IOEvent;

namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.Input"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record InputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{

    /// <inheritdoc />
    public override bool RequiredInput => true;

    /// <inheritdoc />
    public override bool IsIoCommand => true;

    readonly TaskCompletionSource<char> source = new();

    /// <inheritdoc />
    public override ValueTask<IOEvent?> GetIoEventAsync(CancellationToken ct) => new(InputChar(c => source.TrySetResult(c)));

    /// <inheritdoc />
    public override async ValueTask<BrainfuckContext> ExecuteAsync(IOEvent ioEvent, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!source.Task.IsCompleted) return Context;
        var inputChar = await source.Task;
        return Context with
        {
            Stack = Context.Stack.SetItem(Context.StackIndex, (byte)inputChar),
            SequencesIndex = Context.SequencesIndex + 1
        };
    }

    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => throw new NotSupportedException("Synchronous input is not supported in the event-based I/O model.");

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException("Synchronous input is not supported in the event-based I/O model.");
}
