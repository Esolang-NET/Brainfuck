using TUnit.Assertions.Enums;

namespace Esolang.Brainfuck.Processor.Tests;

public class BrainfuckProcessorTests
{
    readonly TestContext TestContext;
    public BrainfuckProcessorTests() => TestContext = TestContext.Current!;
    void LogWriteLine(string message) => TestContext.OutputWriter.WriteLine(message);
    internal static IEnumerable<object?[]> RunAndOutputStringTestData
    {
        get
        {
            yield return RunAndOutputStringTest(
                source: "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.",
                expected: "Hello, world!"
            );
            yield return RunAndOutputStringTest(
                source: "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.+.+.>++++++++++.",
                expected: "ABC\n"
            );
            yield return RunAndOutputStringTest(
                source: "++++++[>++++++++<-]++++++++++[>.+<-]",
                expected: "0123456789"
            );
            // Note: Interactive input test requires complex event handling setup.
            // Simplified for now.
            static object?[] RunAndOutputStringTest(string source, string? input = default, string? expected = default)
                => [source, input, expected];
        }
    }
    [Test]
    [MethodDataSource(nameof(RunAndOutputStringTestData))]
    public async Task RunAndOutputStringAsyncTest(string source, string? input, string? expected, CancellationToken CancellationToken)
    {
        var enumerable = new BrainfuckSequenceEnumerable(source);
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckProcessor(sequences);

        var actual = await runner.RunAndOutputStringAsync(input, CancellationToken);

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task BrainfuckRunnerTest()
    {
        var runner = new BrainfuckProcessor("]");
        runner.Deconstruct(out var sequences);
        await Assert.That(sequences.ToArray()).IsEquivalentTo((BrainfuckSequence[])[BrainfuckSequence.End], CollectionOrdering.Matching);
        LogWriteLine(runner.ToString());
    }
    [Test]
    public async Task BrainfuckRunnerWithOptionTest()
    {
        {
            TestShared.BrainfuckOptions options = new();
            var runner = new BrainfuckProcessor("]", options);
            runner.Deconstruct(out var sequences);
            await Assert.That(sequences.ToArray()).IsEquivalentTo((BrainfuckSequence[])[BrainfuckSequence.End], CollectionOrdering.Matching);
            LogWriteLine(runner.ToString());
        }
        {
            TestShared.BrainfuckOptions options = new();
            var runner = new BrainfuckProcessor("[", options);
            runner.Deconstruct(out var sequences);
            await Assert.That(sequences.ToArray()).IsEquivalentTo((BrainfuckSequence[])[BrainfuckSequence.Begin], CollectionOrdering.Matching);
            LogWriteLine(runner.ToString());
        }

    }

}
