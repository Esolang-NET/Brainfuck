using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;
using Command = Brainfuck.Runner.SequenceCommands.BrainfuckSequenceCommand;

namespace Brainfuck.Runner.SequenceCommands.Tests;

[TestClass]
public class BrainfuckSequenceCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> TryGetCommandTestData
    {
        get
        {
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementPointer }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(IncrementPointerCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { DecrementPointer }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(DecrementPointerCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementCurrent }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(IncrementCurrentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { DecrementCurrent }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(DecrementCurrentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Output }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(OutputCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Input }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(InputCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Begin }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(BeginCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { End }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(EndCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { Comment }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(CommentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { (BrainfuckSequence)byte.MaxValue }, Stack = ImmutableArray.Create<byte>(0), },
                typeof(CommentCommand)
            );
            yield return TryGetCommandTest(
                new() { Sequences = new[] { IncrementPointer }, SequencesIndex = 1, Stack = ImmutableArray.Create<byte>(0), },
                null
            );
            static object?[] TryGetCommandTest(TestShared.BrainfuckContext context, Type? expected)
                => new object?[] { context, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(TryGetCommandTestData))]
    public void TryGetCommandTest(TestShared.BrainfuckContext context, Type expected)
    {
        var result = Command.TryGetCommand(context, out var command);
        Assert.AreEqual(expected is not null, result);
        if (!result)
        {
            Assert.IsNull(command);
            return;
        }
        Assert.IsNotNull(command);
        Assert.AreEqual(expected, command.GetType());
    }
    [TestMethod]
    [DynamicData(nameof(TryGetCommandTestData))]
    public void Cast(TestShared.BrainfuckContext context, Type expected)
    {
        var command = (Command?)(BrainfuckContext)context;
        Assert.AreEqual(expected is not null, command is not null);
        if (command is null)
        {
            Assert.IsNull(command);
            return;
        }
        Assert.IsNotNull(command);
        Assert.AreEqual(expected, command.GetType());
    }

}
