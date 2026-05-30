using Esolang.Processor;
using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.InputCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

[TestClass()]
public class InputCommandTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public async Task ExecuteAsyncTest()
    {
        var token = TestContext.CancellationToken;
        var sequences = new[] { Input }.AsMemory();
        var stack = ImmutableArray.Create<byte>(2);
        BrainfuckContext context = new(
            Sequences: sequences,
            Stack: stack
        );

        var command = new Command(context);
        var ioEvent = await command.GetIoEventAsync(token);
        Assert.IsInstanceOfType<InputCharEvent>(ioEvent);

        ((InputCharEvent)ioEvent!).Write('A');

        var actual = await command.ExecuteAsync(ioEvent!, token);

        var expected = context with
        {
            Stack = [(byte)'A'],
            SequencesIndex = 1,
        };
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.IsTrue(command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.IsFalse(command.RequiredOutput);
    }
}
