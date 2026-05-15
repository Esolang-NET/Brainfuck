using Esolang.Processor;
using System.IO.Pipelines;
using System.Text;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : ITextProcessor<ReadOnlyMemory<BrainfuckSequence>>, IPipeProcessor<ReadOnlyMemory<BrainfuckSequence>>
{
    /// <inheritdoc/>
    ReadOnlyMemory<BrainfuckSequence> IProcessor<ReadOnlyMemory<BrainfuckSequence>>.Program => Program;

    // ── ITextProcessor ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public int RunToEnd(TextReader? input = null, TextWriter? output = null, CancellationToken cancellationToken = default)
    {
        var pipeInput = input is null ? Input : CreatePipeReaderFromTextReader(input);

        if (output is null)
        {
            Run(Context with { Input = pipeInput }, cancellationToken);
            return 0;
        }

        var outputPipe = new Pipe();
        Run(Context with { Input = pipeInput, Output = outputPipe.Writer }, cancellationToken);
        outputPipe.Writer.Complete();
        DrainPipeToTextWriter(outputPipe.Reader, output);
        outputPipe.Reader.Complete();
        return 0;
    }

    /// <inheritdoc/>
    public async ValueTask<int> RunToEndAsync(TextReader? input = null, TextWriter? output = null, CancellationToken cancellationToken = default)
    {
        var pipeInput = input is null ? Input : CreatePipeReaderFromTextReader(input);

        if (output is null)
        {
            await RunAsync(Context with { Input = pipeInput }, cancellationToken);
            return 0;
        }

        var outputPipe = new Pipe();
        await RunAsync(Context with { Input = pipeInput, Output = outputPipe.Writer }, cancellationToken);
        await outputPipe.Writer.CompleteAsync();
        await DrainPipeToTextWriterAsync(outputPipe.Reader, output, cancellationToken);
        await outputPipe.Reader.CompleteAsync();
        return 0;
    }

    // ── IPipeProcessor ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public int RunToEnd(PipeReader input, PipeWriter output, CancellationToken cancellationToken = default)
    {
        Run(Context with { Input = input, Output = output }, cancellationToken);
        return 0;
    }

    /// <inheritdoc/>
    public async ValueTask<int> RunToEndAsync(PipeReader input, PipeWriter output, CancellationToken cancellationToken = default)
    {
        await RunAsync(Context with { Input = input, Output = output }, cancellationToken);
        return 0;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static PipeReader CreatePipeReaderFromTextReader(TextReader reader)
    {
        var content = reader.ReadToEnd();
        var bytes = Encoding.UTF8.GetBytes(content);
        return PipeReader.Create(new MemoryStream(bytes));
    }

    private static void DrainPipeToTextWriter(PipeReader reader, TextWriter writer)
    {
        while (reader.TryRead(out var result))
        {
            foreach (var segment in result.Buffer)
                writer.Write(Encoding.UTF8.GetString(segment.Span.ToArray()));
            reader.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted) break;
        }
    }

    private static async Task DrainPipeToTextWriterAsync(PipeReader reader, TextWriter writer, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            foreach (var segment in result.Buffer)
                writer.Write(Encoding.UTF8.GetString(segment.Span.ToArray()));
            reader.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted) break;
        }
    }
}
