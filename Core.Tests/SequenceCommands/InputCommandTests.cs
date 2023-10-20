using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.IO.Pipelines;
using static Brainfuck.BrainfuckSequence;
using Command = Brainfuck.Core.SequenceCommands.InputCommand;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class InputCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // input set.
                var sequences = new[] { Input }.AsMemory();
                var stack = ImmutableArray.Create<byte>(2);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    new byte[] { 1 },
                    context with
                    {
                        Stack = ImmutableArray.Create<byte>(1),
                        SequencesIndex = 1,
                    }
                );
            }
            {
                // input nodata.
                var sequences = new[] { Input }.AsMemory();
                var stack = ImmutableArray.Create<byte>(2);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    Array.Empty<byte>(),
                    context with
                    {
                        Stack = ImmutableArray.Create<byte>(0),
                        SequencesIndex = 1,
                    }
                );
            }
            static object?[] ExecuteAsyncTest(TestShared.BrainfuckContext context, byte[] input, TestShared.BrainfuckContext expected)
                => new object?[] { context, input.ToSerializable(), expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, SerializableArrayWrapper<byte> input, TestShared.BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        context = context with
        {
            Input = pipe.Reader,
        };
        expected = expected with
        {
            Input = pipe.Reader,
        };

        var waiter = new Command(context).ExecuteAsync(token);
        if (input.Array.Length > 0)
            await pipe.Writer.WriteAsync(input, token);
        await pipe.Writer.CompleteAsync();
        var actual = await waiter;
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(TestShared.BrainfuckContext context, SerializableArrayWrapper<byte> input, TestShared.BrainfuckContext expected)
    {
        var pipe = new Pipe();
        context = context with
        {
            Input = pipe.Reader,
        };
        expected = expected with
        {
            Input = pipe.Reader,
        };

        if (input.Array.Length > 0)
        {
            var dest = pipe.Writer.GetSpan(input.Array.Length);
            input.Array.AsSpan().CopyTo(dest);
            pipe.Writer.Advance(input.Array.Length);
        }
        pipe.Writer.Complete();
        var actual = new Command(context).Execute();
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }

    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(true, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredOutput);
    }
}
