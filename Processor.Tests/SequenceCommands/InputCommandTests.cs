using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using static Esolang.Processor.IOEvent;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.InputCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class InputCommandTests
{
    public TestContext TestContext { get; set; } = default!;

    [Test]
    public async Task ExecuteAsyncTest(CancellationToken CancellationToken)
    {
        var sequences = new[] { Input }.AsMemory();
        var stack = ImmutableArray.Create<byte>(2);
        BrainfuckContext context = new(
            Sequences: sequences,
            Stack: stack
        );

        var command = new Command(context);
        var ioEvent = await command.GetIoEventAsync(CancellationToken);
        await Assert.That(ioEvent).IsTypeOf<InputCharEvent>(); ;

        ((InputCharEvent)ioEvent!).Write('A');

        var actual = await command.ExecuteAsync(ioEvent!, CancellationToken);

        var expected = context with
        {
            Stack = [(byte)'A'],
            SequencesIndex = 1,
        };
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task RequiredInputTest()
    {
        var command = new Command(default);
        await Assert.That(command.RequiredInput).IsTrue();
    }
    [Test]
    public async Task RequiredOutputTest()
    {
        var command = new Command(default);
        await Assert.That(command.RequiredOutput).IsFalse();
    }
}
