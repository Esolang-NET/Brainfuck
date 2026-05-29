using Esolang.Processor;

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
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => throw new NotSupportedException("Synchronous input is not supported in the event-based I/O model.");

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException("Synchronous input is not supported in the event-based I/O model.");
}
