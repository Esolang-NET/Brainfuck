using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.BrainfuckSequenceCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class BrainfuckSequenceCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    internal static IEnumerable<object?[]> TryGetCommandTestData
    {
        get
        {
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementPointer }, Stack = [0], },
                typeof(IncrementPointerCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { DecrementPointer }, Stack = [0], },
                typeof(DecrementPointerCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementCurrent }, Stack = [0], },
                typeof(IncrementCurrentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { DecrementCurrent }, Stack = [0], },
                typeof(DecrementCurrentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Output }, Stack = [0], },
                typeof(OutputCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Input }, Stack = [0], },
                typeof(InputCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Begin }, Stack = [0], },
                typeof(BeginCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { End }, Stack = [0], },
                typeof(EndCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Comment }, Stack = [0], },
                typeof(CommentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { (BrainfuckSequence)byte.MaxValue }, Stack = [0], },
                typeof(CommentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementPointer }, SequencesIndex = 1, Stack = [0], },
                null
            );
            static object?[] TryGetCommandTest(TestShared.BrainfuckContext context, Type? expected)
                => [context, expected];
        }
    }
    [Test]
    [MethodDataSource(nameof(TryGetCommandTestData))]
    public async Task TryGetCommandTest(TestShared.BrainfuckContext context, Type expected)
    {
        var result = Command.TryGetCommand(context, out var command);
        await Assert.That(result).IsEqualTo(expected is not null);
        if (!result)
        {
            await Assert.That(command).IsNull();
            return;
        }
        Assert.NotNull(command);
        await Assert.That(command.GetType()).IsEqualTo(expected);
    }
    [Test]
    [MethodDataSource(nameof(TryGetCommandTestData))]
    public async Task Cast(TestShared.BrainfuckContext context, Type expected)
    {
        var command = (Command?)(BrainfuckContext)context;
        await Assert.That(command is not null).IsEqualTo(expected is not null);
        if (command is null)
        {
            await Assert.That(command).IsNull();
            return;
        }
        Assert.NotNull(command);
        await Assert.That(command.GetType()).IsEqualTo(expected);
    }

}
