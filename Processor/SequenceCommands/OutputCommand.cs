using Esolang.Processor;
using static Esolang.Processor.IOEvent;

namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.Output"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record OutputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    /// <inheritdoc />
    public override bool RequiredOutput => true;

    /// <inheritdoc />
    public override bool IsIoCommand => true;

    /// <inheritdoc />
    public override ValueTask<IOEvent?> GetIoEventAsync(CancellationToken ct) => new(OutputChar((char)Context.Stack[Context.StackIndex]));

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(IOEvent ioEvent, CancellationToken ct) => new(Context with { SequencesIndex = Context.SequencesIndex + 1 });

    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) =>
        // In event-based model, output happens via IOEvent, not synchronous write.
        // Returning the next context index.
        Context with { SequencesIndex = Context.SequencesIndex + 1 };

    /// <inheritdoc />
    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context with { SequencesIndex = Context.SequencesIndex + 1 };
    }
}
