using Esolang.Brainfuck.Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CommandLine;

namespace Esolang.Brainfuck.Interpreter.Tests;

[TestClass]
public class BrainfuckConsoleExtensionsTests
{
    [TestMethod]
    public void AddDefaultGlobalOptions_ShouldRegisterAllSyntaxOptions()
    {
        var root = new RootCommand();

        root.AddDefaultGlobalOptions();

        AssertCanParse(root, ["--syntax-no-use-default-value"]);
        AssertCanParse(root, ["-snd"]);
        AssertCanParse(root, ["--syntax-increment-pointer", "x"]);
        AssertCanParse(root, ["-sip", "x"]);
        AssertCanParse(root, ["--syntax-dencrement-pointer", "x"]);
        AssertCanParse(root, ["-sdp", "x"]);
        AssertCanParse(root, ["--syntax-increment-current", "x"]);
        AssertCanParse(root, ["-sic", "x"]);
        AssertCanParse(root, ["--syntax-decrement-current", "x"]);
        AssertCanParse(root, ["-sdc", "x"]);
        AssertCanParse(root, ["--syntax-output", "x"]);
        AssertCanParse(root, ["-so", "x"]);
        AssertCanParse(root, ["--syntax-input", "x"]);
        AssertCanParse(root, ["-si", "x"]);
        AssertCanParse(root, ["--syntax-begin", "x"]);
        AssertCanParse(root, ["-sb", "x"]);
        AssertCanParse(root, ["--syntax-end", "x"]);
        AssertCanParse(root, ["-se", "x"]);
    }

    [TestMethod]
    public void GetValue_ShouldUseDefaults_WhenNoOverrides()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        var parsed = root.Parse([]);
        var options = binder.GetValue(parsed);

        Assert.AreEqual(BrainfuckOptionsDefault.IncrementPointer, options.IncrementPointer);
        Assert.AreEqual(BrainfuckOptionsDefault.DecrementPointer, options.DecrementPointer);
        Assert.AreEqual(BrainfuckOptionsDefault.IncrementCurrent, options.IncrementCurrent);
        Assert.AreEqual(BrainfuckOptionsDefault.DecrementCurrent, options.DecrementCurrent);
        Assert.AreEqual(BrainfuckOptionsDefault.Output, options.Output);
        Assert.AreEqual(BrainfuckOptionsDefault.Input, options.Input);
        Assert.AreEqual(BrainfuckOptionsDefault.Begin, options.Begin);
        Assert.AreEqual(BrainfuckOptionsDefault.End, options.End);
    }

    [TestMethod]
    public void GetValue_ShouldUseExplicitOverrides()
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

        Assert.AreEqual("😀", options.IncrementPointer);
        Assert.AreEqual("😁", options.DecrementPointer);
        Assert.AreEqual("😂", options.IncrementCurrent);
        Assert.AreEqual("🤣", options.DecrementCurrent);
        Assert.AreEqual("😃", options.Output);
        Assert.AreEqual("😄", options.Input);
        Assert.AreEqual("😅", options.Begin);
        Assert.AreEqual("😆", options.End);
    }

    [TestMethod]
    public void AddDefaultCommand_ShouldRegisterSourceArgument()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        root.AddDefaultCommand(binder);

        Assert.IsTrue(root.Arguments.Any(v => v.Name == "source"));
    }

    [TestMethod]
    public void AddParseCommand_ShouldRegisterParseSubcommand()
    {
        var root = new RootCommand();
        var binder = root.AddDefaultGlobalOptions();

        root.AddParseCommand(binder);

        Assert.IsTrue(root.Subcommands.Any(v => v.Name == "parse"));
    }

    static void AssertCanParse(Command root, string[] args)
    {
        var parsed = root.Parse(args);
        Assert.AreEqual(0, parsed.Errors.Count, $"Expected successful parse for args: {string.Join(" ", args)}");
    }
}
