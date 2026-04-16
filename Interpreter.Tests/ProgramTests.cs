using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Esolang.Brainfuck.Interpreter.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public async Task RunAsync_ParseCommand_ReturnsZero()
    {
        var exitCode = await Program.RunAsync(["parse", "++"]);
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public async Task RunAsync_DefaultCommand_ReturnsZero()
    {
        var exitCode = await Program.RunAsync(["++++"]);
        Assert.AreEqual(0, exitCode);
    }
}
