using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Processor.SequenceCommands.CommentCommand;

namespace Esolang.Brainfuck.Processor.SequenceCommands.Tests;

[TestClass()]
public class CommentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // noop
                var sequences = new[] { Comment }.AsMemory();
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
            static object?[] ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
                => new object?[] { context, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var actual = await new Command(context).ExecuteAsync(token);
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
    {
        var actual = new Command(context).Execute();
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }
    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredOutput);
    }
}
