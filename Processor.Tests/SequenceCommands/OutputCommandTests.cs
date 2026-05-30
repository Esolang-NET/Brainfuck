using Esolang.Processor;
using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.OutputCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

[TestClass()]
public class OutputCommandTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public async Task ExecuteAsyncTest()
    {
        var token = TestContext.CancellationToken;
        var sequences = new[] { Output }.AsMemory();
        var stack = ImmutableArray.Create<byte>(65); // 'A'
        BrainfuckContext context = new(
            Sequences: sequences,
            Stack: stack,
            StackIndex: 0
        );

        var command = new Command(context);
        var ioEvent = await command.GetIoEventAsync(token);
        Assert.IsInstanceOfType<OutputCharEvent>(ioEvent);
        Assert.AreEqual('A', ((OutputCharEvent)ioEvent!).Output);

        var actual = await command.ExecuteAsync(ioEvent!, token);

        var expected = context with
        {
            SequencesIndex = 1,
        };
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.IsFalse(command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.IsTrue(command.RequiredOutput);
    }
}
