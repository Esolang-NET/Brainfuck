using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.DecrementPointerCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class DecrementPointerCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    internal static IEnumerable<object[]> ExecuteTestData
    {
        get
        {
            {
                // stackPointer -1 
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0, 0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    StackIndex: 1
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                        StackIndex = 0,
                    }
                );
            }
            {
                // no op pattern.
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
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
