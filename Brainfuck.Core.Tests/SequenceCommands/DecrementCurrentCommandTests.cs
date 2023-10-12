using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class DecrementCurrentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // currentStack -1
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableList.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                        Stack = ImmutableList.Create<byte>(0),
                    }
                );
            }
            {
                // stackPointer -1 underflow 0 → 255
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableList.Create(byte.MinValue);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                        Stack = ImmutableList.Create(byte.MaxValue),
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

        var actual = await new DecrementCurrentCommand(context).ExecuteAsync(token);
        Assert.AreEqual(expected, actual);
    }
}