using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Esolang.Brainfuck.BrainfuckSequence;
using Command = Esolang.Brainfuck.Runner.SequenceCommands.DecrementPointerCommand;

namespace Esolang.Brainfuck.Runner.SequenceCommands.Tests;

[TestClass]
public class DecrementPointerCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteTestData
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
                => new object[] { context, expected };
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
