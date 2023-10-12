using Brainfuck.Core.SequenceCommands;
using System.Collections.Immutable;
using System.IO.Pipelines;
using System.Text;

namespace Brainfuck;

/// <summary>
/// 
/// </summary>
public record BrainfuckRunner
{
    readonly ReadOnlyMemory<BrainfuckSequence> Sequences;
    readonly PipeReader? Input;
    readonly PipeWriter? Output;
    BrainfuckContext Context => new(Sequences, SequencesIndex: 0, Stack: ImmutableList.Create<byte>(0), StackIndex: 0, Input: Input, Output: Output);
    public BrainfuckRunner(string source, BrainfuckOptions? sourceOptions = default, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) { }
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions? sourceOptions)
        => new BrainfuckSequencer(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();

    public BrainfuckRunner(ReadOnlyMemory<BrainfuckSequence> sequences, PipeWriter? output = default, PipeReader? input = default)
    {
        Sequences = sequences;
        Input = input;
        Output = output;
    }
    public ValueTask RunAsync(CancellationToken cancellationToken = default) => RunAsync(Context, cancellationToken);
    static async ValueTask RunAsync(BrainfuckContext context, CancellationToken cancellationToken = default)
    {
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
            context = await command.ExecuteAsync(cancellationToken);
    }
    public async ValueTask<string?> RunAndOutputStringAsync(CancellationToken cancellationToken = default)
    {
        var pipe = new Pipe();
        var context = Context with
        {
            Output = pipe.Writer,
        };

        await RunAsync(context, cancellationToken);

        await pipe.Writer.CompleteAsync();
        using var stream = new MemoryStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        await pipe.Reader.CopyToAsync(stream, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Length == 0) return null;
        return await reader.ReadToEndAsync();

    }
}