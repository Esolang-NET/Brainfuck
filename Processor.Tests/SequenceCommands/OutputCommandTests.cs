using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using static Esolang.Processor.IOEvent;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.OutputCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class OutputCommandTests
{
    public TestContext TestContext { get; set; } = default!;

    [Test]
    public async Task ExecuteAsyncTest(CancellationToken CancellationToken)
    {
        var sequences = new[] { Output }.AsMemory();
        var stack = ImmutableArray.Create<byte>(65); // 'A'
        BrainfuckContext context = new(
            Sequences: sequences,
            Stack: stack,
            StackIndex: 0
        );

        var command = new Command(context);
        var ioEvent = await command.GetIoEventAsync(CancellationToken);
        await Assert.That(ioEvent).IsTypeOf<OutputCharEvent>();
        await Assert.That(((OutputCharEvent)ioEvent!).Output).IsEqualTo('A');

        var actual = await command.ExecuteAsync(ioEvent!, CancellationToken);

        var expected = context with
        {
            SequencesIndex = 1,
        };
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task RequiredInputTest()
    {
        var command = new Command(default);
        await Assert.That(command.RequiredInput).IsFalse();
    }
    [Test]
    public async Task RequiredOutputTest()
    {
        var command = new Command(default);
        await Assert.That(command.RequiredOutput).IsTrue();
    }
}
