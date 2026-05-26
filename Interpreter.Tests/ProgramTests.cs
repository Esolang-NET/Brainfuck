using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Esolang.Brainfuck.Interpreter.Tests;

[TestClass]
public class ProgramTests
{
    static int Run(string[] args)
    {
        var entryPoint = typeof(Program).Assembly.EntryPoint!;
        Assert.IsNotNull(entryPoint);
        object?[] parmaeters = [args];
        var result = entryPoint.Invoke(null, parmaeters) as int?;
        Assert.IsNotNull(result);
        return result.Value;
    }
    [TestMethod]
    public void RunAsync_ParseCommand_ReturnsZero() => Assert.AreEqual(0, Run(["parse", "++"]));

    [TestMethod]
    public void RunAsync_DefaultCommand_ReturnsZero() => Assert.AreEqual(0, Run(["+++"]));

    [TestMethod]
    public void RunAsync_EmptyArgs_ReturnOne() => Assert.AreEqual(1, Run([]));

    [TestMethod]
    public void RunAsync_Help_ReturnZero() => Assert.AreEqual(0, Run(["--help"]));
}
