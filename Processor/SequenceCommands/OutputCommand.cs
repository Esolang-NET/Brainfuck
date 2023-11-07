namespace Esolang.Brainfuck.Processor.SequenceCommands;

public sealed record OutputCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override bool RequiredOutput => true;

    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        var sequenceIndex = Output();
        return Context with
        {
            SequencesIndex = sequenceIndex,
        };
    }

    public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = await OutputAsync(cancellationToken);
        return Context with
        {
            SequencesIndex = sequencesIndex,
        };
    }
    async ValueTask<int> OutputAsync(CancellationToken cancellationToken)
    {
        if (Context.Output is null) throw new InvalidOperationException("required context.Output.");
        var sequencesIndex = Context.SequencesIndex + 1;
        var memory = Context.Stack.AsMemory().Slice(Context.StackIndex, 1);
        await Context.Output.WriteAsync(memory, cancellationToken);
        return sequencesIndex;
    }
    int Output()
    {

        if (Context.Output is null) throw new InvalidOperationException("required context.Output.");
        var sequencesIndex = Context.SequencesIndex + 1;
        Context.Stack.AsMemory().Slice(Context.StackIndex, 1)
            .Span.CopyTo(Context.Output.GetSpan(1));
        Context.Output.Advance(1);
        return sequencesIndex;
    }
}
