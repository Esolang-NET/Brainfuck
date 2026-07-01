using System.CommandLine;

namespace Esolang.Brainfuck.Interpreter.Tests;

public class BrainfuckConsoleExtensionsTests
{
    [Test]
    public async Task AddDefaultGlobalOptions_ShouldRegisterAllSyntaxOptions()
    {
        var root = new RootCommand();

        root.AddDefaultGlobalOptions();

        await AssertCanParse(root, ["--syntax-no-use-default-value"]);
        await AssertCanParse(root, ["-snd"]);
        await AssertCanParse(root, ["--syntax-increment-pointer", "x"]);
        await AssertCanParse(root, ["-sip", "x"]);
        await AssertCanParse(root, ["--syntax-dencrement-pointer", "x"]);
        await AssertCanParse(root, ["-sdp", "x"]);
        await AssertCanParse(root, ["--syntax-increment-current", "x"]);
        await AssertCanParse(root, ["-sic", "x"]);
        await AssertCanParse(root, ["--syntax-decrement-current", "x"]);
        await AssertCanParse(root, ["-sdc", "x"]);
        await AssertCanParse(root, ["--syntax-output", "x"]);
        await AssertCanParse(root, ["-so", "x"]);
        await AssertCanParse(root, ["--syntax-input", "x"]);
        await AssertCanParse(root, ["-si", "x"]);
        await AssertCanParse(root, ["--syntax-begin", "x"]);
        await AssertCanParse(root, ["-sb", "x"]);
        await AssertCanParse(root, ["--syntax-end", "x"]);
        await AssertCanParse(root, ["-se", "x"]);
    }

    [Test]
    public async Task GetValue_ShouldUseDefaults_WhenNoOverrides()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        var parsed = root.Parse([]);
        var options = binder.GetValue(parsed);

        await Assert.That(options.IncrementPointer).IsEqualTo(BrainfuckOptionsDefault.IncrementPointer);
        await Assert.That(options.DecrementPointer).IsEqualTo(BrainfuckOptionsDefault.DecrementPointer);
        await Assert.That(options.IncrementCurrent).IsEqualTo(BrainfuckOptionsDefault.IncrementCurrent);
        await Assert.That(options.DecrementCurrent).IsEqualTo(BrainfuckOptionsDefault.DecrementCurrent);
        await Assert.That(options.Output).IsEqualTo(BrainfuckOptionsDefault.Output);
        await Assert.That(options.Input).IsEqualTo(BrainfuckOptionsDefault.Input);
        await Assert.That(options.Begin).IsEqualTo(BrainfuckOptionsDefault.Begin);
        await Assert.That(options.End).IsEqualTo(BrainfuckOptionsDefault.End);
    }

    [Test]
    public async Task GetValue_ShouldUseExplicitOverrides()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        var parsed = root.Parse([
            "--syntax-increment-pointer", "😀",
            "--syntax-dencrement-pointer", "😁",
            "--syntax-increment-current", "😂",
            "--syntax-decrement-current", "🤣",
            "--syntax-output", "😃",
            "--syntax-input", "😄",
            "--syntax-begin", "😅",
            "--syntax-end", "😆",
        ]);
        var options = binder.GetValue(parsed);

        await Assert.That(options.IncrementPointer).IsEqualTo("😀");
        await Assert.That(options.DecrementPointer).IsEqualTo("😁");
        await Assert.That(options.IncrementCurrent).IsEqualTo("😂");
        await Assert.That(options.DecrementCurrent).IsEqualTo("🤣");
        await Assert.That(options.Output).IsEqualTo("😃");
        await Assert.That(options.Input).IsEqualTo("😄");
        await Assert.That(options.Begin).IsEqualTo("😅");
        await Assert.That(options.End).IsEqualTo("😆");
    }

    [Test]
    public async Task AddDefaultCommand_ShouldRegisterSourceArgument()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        root.AddDefaultCommand(binder);

        await Assert.That(root.Arguments).Contains(v => v.Name == "source");
    }

    [Test]
    public async Task AddParseCommand_ShouldRegisterParseSubcommand()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        root.AddParseCommand(binder);

        await Assert.That(root.Subcommands).Contains(v => v.Name == "parse");
    }

    static async Task AssertCanParse(Command root, string[] args)
    {
        var parsed = root.Parse(args);
        await Assert.That(parsed.Errors).IsEmpty().Because($"Expected successful parse for args: {string.Join(" ", args)}");
    }
}
