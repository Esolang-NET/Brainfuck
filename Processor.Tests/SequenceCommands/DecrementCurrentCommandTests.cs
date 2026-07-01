using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.DecrementCurrentCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class DecrementCurrentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    internal static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // currentStack -1
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableArray.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                        Stack = [0],
                    }
                );
            }
            {
                // stackPointer -1 underflow 0 → 255
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableArray.Create(byte.MinValue);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                        Stack = [byte.MaxValue],
                    }
                );
            }
            static object[] ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
                => [context, expected];
        }
    }
    [Test]
    [MethodDataSource(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected, CancellationToken CancellationToken)
    {
        var actual = await new Command(context).ExecuteAsync(CancellationToken);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(ExecuteTestData))]
    public async Task ExecuteTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected, CancellationToken CancellationToken)
    {
        var actual = new Command(context).Execute(CancellationToken);
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
        await Assert.That(command.RequiredOutput).IsFalse();
    }
}
