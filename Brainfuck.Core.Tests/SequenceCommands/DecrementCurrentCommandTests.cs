using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
using System.Collections.Immutable;


namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class DecrementCurrentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // currentStack -1
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableList.Create<byte>(1);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: ImmutableList.Create<byte>(0),
                    sequencesIndex: 1
                ));
            }
            {
                // stackPointer -1 underflow 0 → 255
                var sequences = new[] { DecrementCurrent }.AsMemory();
                var stack = ImmutableList.Create(byte.MinValue);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: ImmutableList.Create(byte.MaxValue),
                    sequencesIndex: 1
                ));
            }
            static object[] ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext accept)
                => new object[] { context, accept };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext accept)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var result = await new DecrementCurrentCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}