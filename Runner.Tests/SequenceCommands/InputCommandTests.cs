using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.IO.Pipelines;
using static Esolang.Brainfuck.BrainfuckSequence;
using static TestShared.ArrayExtensions;
using Command = Esolang.Brainfuck.Runner.SequenceCommands.InputCommand;

namespace Esolang.Brainfuck.Runner.SequenceCommands.Tests;

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
                TestShared.BrainfuckContext context = new(
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
                TestShared.BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    Array.Empty<byte>(),
                    context with
                    {
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
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.Array<byte> input, TestShared.BrainfuckContext expected)
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
        if (input.AsArray().Length > 0)
            await pipe.Writer.WriteAsync(input.AsArray(), token);
        await pipe.Writer.CompleteAsync();
        var actual = await waiter;
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(TestShared.BrainfuckContext context, TestShared.Array<byte> input, TestShared.BrainfuckContext expected)
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

        if (input.Length > 0)
        {
            var dest = pipe.Writer.GetSpan(input.Length);
            input.AsArray().AsSpan().CopyTo(dest);
            pipe.Writer.Advance(input.AsArray().Length);
        }
        pipe.Writer.Complete();
        var actual = new Command(context).Execute();
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }
    [TestMethod]
    public void ExecuteAsync_ThrowTest()
    {
        var token = TestContext.CancellationTokenSource.Token;
        var command = new Command(new BrainfuckContext(Sequences: new[] { Input }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await command.ExecuteAsync(token)); ;
    }
    [TestMethod]
    public void Execute_ThrowTest()
    {
        var command = new Command(new BrainfuckContext(Sequences: new[] { Input }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        Assert.ThrowsException<InvalidOperationException>(() => command.Execute());
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
