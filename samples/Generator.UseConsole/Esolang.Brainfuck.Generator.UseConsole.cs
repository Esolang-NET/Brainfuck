using Esolang.Brainfuck;
using System.IO.Pipelines;
using System.Text;

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod1)}: {await BrainfuckSample.SampleMethod1()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod2)}: {await BrainfuckSample.SampleMethod2()}");

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod3)}: {await BrainfuckSample.SampleMethod3("A")}");

var inputPipe = new Pipe();
await inputPipe.Writer.WriteAsync(Encoding.UTF8.GetBytes("Z"));
await inputPipe.Writer.CompleteAsync();
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod4)}: {await BrainfuckSample.SampleMethod4(inputPipe.Reader)}");
await inputPipe.Reader.CompleteAsync();

var outputPipe = new Pipe();
await BrainfuckSample.SampleMethod5(outputPipe.Writer);
await outputPipe.Writer.CompleteAsync();
var outputText = await ReadAllAsUtf8StringAsync(outputPipe.Reader);
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod5)}: {outputText}");
await outputPipe.Reader.CompleteAsync();

static async Task<string> ReadAllAsUtf8StringAsync(PipeReader reader)
{
    var bytes = new List<byte>();
    while (true)
    {
        var readResult = await reader.ReadAsync();
        var buffer = readResult.Buffer;
        foreach (var segment in buffer)
        {
            bytes.AddRange(segment.Span.ToArray());
        }

        reader.AdvanceTo(buffer.End);
        if (readResult.IsCompleted)
        {
            break;
        }
    }

    return Encoding.UTF8.GetString(bytes.ToArray());
}

partial class BrainfuckSample
{
    /// <summary>
    /// Return pattern: Task&lt;string?&gt;. Output text is collected and returned to caller.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial Task<string?> SampleMethod1();

    /// <summary>
    /// Return pattern: Task&lt;string?&gt; with a longer program (Hello, world!).
    /// </summary>
    [GenerateBrainfuckMethod("1+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.")]
    public static partial Task<string?> SampleMethod2();

    /// <summary>
    /// Input pattern: string input. Brainfuck ",." echoes one byte.
    /// </summary>
    [GenerateBrainfuckMethod(",.")]
    public static partial Task<string?> SampleMethod3(string input);

    /// <summary>
    /// Input pattern: PipeReader input. Useful when reading from pipelines/streams.
    /// </summary>
    [GenerateBrainfuckMethod(",.")]
    public static partial Task<string?> SampleMethod4(PipeReader input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Output pattern: PipeWriter output. Generated method writes bytes directly to the writer.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial Task SampleMethod5(PipeWriter output, CancellationToken cancellationToken = default);

    /// <summary>
    /// Custom token pattern: demonstrates replacing Brainfuck command symbols.
    /// </summary>
    [GenerateBrainfuckMethod("😀😁😂🤣😃😄😅😅😅😅😆😆", IncrementPointer = "😀", DecrementPointer = "😁", IncrementCurrent = "😂", DecrementCurrent = "🤣", Output = "😃", Input = "😄", Begin = "😅", End = "😆")]
    public static partial Task<string?> SampleMethod(string input);
}
