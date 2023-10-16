using Microsoft.VisualStudio.TestTools.UnitTesting;
using Command = Brainfuck.Core.SequenceCommands.DecrementPointerCommand;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass]
public class DecrementPointerCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // stackPointer -1 
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0, 0);
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
                var stack = ImmutableList.Create<byte>(0);
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
            static object[] ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
                => new object[] { context, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var actual = await new Command(context).ExecuteAsync(token);
        Assert.AreEqual(expected, actual);
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