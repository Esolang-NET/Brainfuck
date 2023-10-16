using Brainfuck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using BrainfuckOptions = Brainfuck.Console.BrainfuckOptions;

var app = ConsoleApp.CreateBuilder(args)
.ConfigureServices((HostBuilderContext context, IServiceCollection services) =>
{
    var section = context.Configuration.GetSection("Brainfuck");
    services
        .AddOptions<BrainfuckOptions>()
        .Bind(section)
        .ValidateDataAnnotations();
})
.Build();

app.AddRootCommand("run brainfuck from source code", RunAsync);
static async ValueTask<int> RunAsync(ConsoleAppContext context, IOptions<BrainfuckOptions> options, [Option(0, "brainfuck source")] string source)
{
    var o = options.Value;
    var sequence = new BrainfuckSequenceEnumerable(source, o).Select(v => v.Sequence).ToArray().AsMemory();
    var input = new Pipe();
    var output = new Pipe();
    var cancellationToken = context.CancellationToken;
    var encoding = Encoding.UTF8;
    var runner = new BrainfuckRunner(source: source, sourceOptions: o, output: output.Writer, input: input.Reader);
    foreach (var command in runner.RunStep())
    {
        if (command.RequiredInput)
        {
            var kc = await ConsoleReadAsync(cancellationToken);

            // 改行 or バックスペース or ESC はESC と同じ扱いにする
            // if (kc is '\n' or '\r' or '\b' or (char)0)
            // {
            //    kc = (char)0;
            // }
            var bytes = encoding.GetBytes(new[] { kc });
            var flush = await input.Writer.WriteAsync(bytes, cancellationToken);
            await input.Writer.FlushAsync(cancellationToken);

        }
        await command.ExecuteAsync(cancellationToken);
        if (command.RequiredOutput)
        {
            var reader = await output.Reader.ReadAsync(cancellationToken);
            var buffer = reader.Buffer;
            if (buffer.Length > 0)
            {
                var sequence2 = buffer.Slice(buffer.Start, buffer.End);
                if (encoding.GetString(sequence2.FirstSpan) is string chars && !string.IsNullOrEmpty(chars))
                {
                    if (!Console.IsOutputRedirected && chars == "\r") chars = Environment.NewLine;
                    Console.Write(chars);
                }

                output.Reader.AdvanceTo(buffer.End);
            }
        }
    }
    return 0;
    static async ValueTask<char> ConsoleReadAsync(CancellationToken cancellationToken)
    {

        if (Console.IsInputRedirected)
            return Convert.ToChar(Console.Read());
        var cursorTop = Console.CursorTop;
        var cursorLeft = Console.CursorLeft;
        if (Console.KeyAvailable == false)
            await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
        var key = Console.ReadKey(true);
        return key.KeyChar;
    }
}
app.AddCommand("parse", "parse brainfuck source code.", Parse);

static int Parse(ConsoleAppContext context, IOptions<BrainfuckOptions> options, [Option(0)] string source)
{
    var o = options.Value;
    foreach (var (sequence, syntaxes) in new BrainfuckSequenceEnumerable(source, o))
    {
        Console.WriteLine($"{sequence}: {syntaxes}");
    }
    return 0;
};
await app.RunAsync();
