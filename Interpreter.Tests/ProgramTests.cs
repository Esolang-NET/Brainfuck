namespace Esolang.Brainfuck.Interpreter.Tests;

public class ProgramTests
{
    static int Run(string[] args)
    {
        var entryPoint = typeof(Program).Assembly.EntryPoint!;
        Assert.NotNull(entryPoint);
        object?[] parmaeters = [args];
        var result = entryPoint.Invoke(null, parmaeters) as int?;
        Assert.NotNull(result);
        return result.Value;
    }
    [Test]
    public async Task RunAsync_ParseCommand_ReturnsZero() => await Assert.That(Run(["parse", "++"])).IsEqualTo(0);

    [Test]
    public async Task RunAsync_DefaultCommand_ReturnsZero() => await Assert.That(Run(["+++"])).IsEqualTo(0);

    [Test]
    public async Task RunAsync_EmptyArgs_ReturnOne() => await Assert.That(Run([])).IsEqualTo(1);

    [Test]
    public async Task RunAsync_Help_ReturnZero() => await Assert.That(Run(["--help"])).IsEqualTo(0);
}
