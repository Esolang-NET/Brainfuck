using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.EndCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

public class EndCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    internal static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // while(true) {
                // } ← before
                // ← after
                var sequences = new[] { Begin, Comment, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 2
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 3,
                    }
                );
            }
            {
                // while(true) {
                // ← after
                // } ← before
                var sequences = new[] { Begin, Comment, Begin, Comment, End, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 5
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                    }
                );
            }
            {
                // invalid pattern 1
                // loop out end -> other
                var sequences = new[] { Comment, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 1
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 2,
                    }
                );
            }
            {
                // invalid pattern 2
                // loop out end -> end
                var sequences = new[] { End, End, Comment }.AsMemory();
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
            {
                // invalid pattern 3
                // invalid loop skip loop
                var sequences = new[] { End, End, Comment }.AsMemory();
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
                    }
                );
            }
            static object?[] ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
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
