using Esolang.Brainfuck;
using System.IO.Pipelines;
using System.Text;

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod1)}: {await BrainfuckSample.SampleMethod1()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod2)}: {await BrainfuckSample.SampleMethod2()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod6)}: {await BrainfuckSample.SampleMethod6()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod7)}: {BrainfuckSample.SampleMethod7()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod8)}: {Encoding.UTF8.GetString(BrainfuckSample.SampleMethod8().ToArray())}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod9)}: {Encoding.UTF8.GetString(await ToByteArrayAsync(BrainfuckSample.SampleMethod9()))}");

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod3)}: {await BrainfuckSample.SampleMethod3("A")}");

using var textReader = new StringReader("B");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod10)}: {await BrainfuckSample.SampleMethod10(textReader)}");

var inputPipe = new Pipe();
await inputPipe.Writer.WriteAsync(Encoding.UTF8.GetBytes("Z"));
await inputPipe.Writer.CompleteAsync();
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod4)}: {await BrainfuckSample.SampleMethod4(inputPipe.Reader)}");
await inputPipe.Reader.CompleteAsync();

using var textWriter = new StringWriter();
await BrainfuckSample.SampleMethod11(textWriter);
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod11)}: {textWriter.ToString()}");

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

static async Task<byte[]> ToByteArrayAsync(IAsyncEnumerable<byte> source)
{
    var bytes = new List<byte>();
    await foreach (var b in source)
    {
        bytes.Add(b);
    }
    return bytes.ToArray();
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
    /// Return pattern: ValueTask&lt;string?&gt;.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial ValueTask<string?> SampleMethod6();

    /// <summary>
    /// Return pattern: synchronous string?.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial string? SampleMethod7();

    /// <summary>
    /// Return pattern: IEnumerable&lt;byte&gt;.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial IEnumerable<byte> SampleMethod8();

    /// <summary>
    /// Return pattern: IAsyncEnumerable&lt;byte&gt;.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial IAsyncEnumerable<byte> SampleMethod9();

    /// <summary>
    /// Input pattern: string input. Brainfuck ",." echoes one byte.
    /// </summary>
    [GenerateBrainfuckMethod(",.")]
    public static partial Task<string?> SampleMethod3(string input);

    /// <summary>
    /// Input pattern: TextReader input.
    /// </summary>
    [GenerateBrainfuckMethod(",.")]
    public static partial Task<string?> SampleMethod10(TextReader input, CancellationToken cancellationToken = default);

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
    /// Output pattern: TextWriter output.
    /// </summary>
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial Task SampleMethod11(TextWriter output, CancellationToken cancellationToken = default);

    /// <summary>
    /// Custom token pattern: demonstrates replacing Brainfuck command symbols.
    /// </summary>
    [GenerateBrainfuckMethod("😀😁😂🤣😃😄😅😅😅😅😆😆", IncrementPointer = "😀", DecrementPointer = "😁", IncrementCurrent = "😂", DecrementCurrent = "🤣", Output = "😃", Input = "😄", Begin = "😅", End = "😆")]
    public static partial Task<string?> SampleMethod(string input);
}
