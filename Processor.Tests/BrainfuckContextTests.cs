namespace Esolang.Brainfuck.Processor.Tests;

public class BrainfuckContextTests
{
    readonly TestContext TestContext;
    public BrainfuckContextTests() => TestContext = TestContext.Current!;

    void LogWriteLine(string message) => TestContext.OutputWriter.WriteLine(message);

    [Test]
    public void ToStringTest()
    {
        var context1 = new BrainfuckContext();
        Assert.NotNull(context1.ToString());

        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: [0]
        );
        Assert.NotNull(context2.ToString());

        BrainfuckContext context3 = default;

        Assert.NotNull(context3.ToString());
    }
    [Test]
    public async Task EqualsTest()
    {

        var context1 = new BrainfuckContext();
        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: [0]
        );
        BrainfuckContext context3 = default;
        await Assert.That(context1).IsNotEqualTo(context2);
        await Assert.That(context1).IsEqualTo(context3);
    }
    [Test]
    public async Task GetHashCodeTest()
    {

        var context1 = new BrainfuckContext();
        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: [0]
        );
        BrainfuckContext context3 = default;
        var hashCode1 = context1.GetHashCode();
        var hashCode2 = context2.GetHashCode();
        var hashCode3 = context3.GetHashCode();
        LogWriteLine($"{nameof(context1)}:{hashCode1}");
        LogWriteLine($"{nameof(context2)}:{hashCode2}");
        LogWriteLine($"{nameof(context3)}:{hashCode3}");
        await Assert.That(hashCode1).IsNotEqualTo(hashCode2);
        await Assert.That(hashCode1).IsEqualTo(hashCode3);
    }
}
