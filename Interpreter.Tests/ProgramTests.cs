using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Esolang.Brainfuck.Interpreter.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public void RunAsync_ParseCommand_ReturnsZero()
    {
        var entryPoint = typeof(BrainfuckOptionBinder).Assembly.EntryPoint!;

        object?[] parmaeters = [new string[] { "parse", "++" }];
        var result = entryPoint.Invoke(null, parmaeters) as int?;
        Assert.IsNotNull(result);
        var exitCode = result.Value;
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public void RunAsync_DefaultCommand_ReturnsZero()
    {
        var entryPoint = typeof(BrainfuckOptionBinder).Assembly.EntryPoint!;
        object?[] parmaeters = [new string[] { "++++" }];
        var result = entryPoint.Invoke(null, parmaeters) as int?;
        Assert.IsNotNull(result);
        var exitCode = result.Value;
        Assert.AreEqual(0, exitCode);
    }
}
