using TUnit.Assertions.Enums;
using static Esolang.Brainfuck.BrainfuckSequence;
namespace Esolang.Brainfuck.Tests;

public class BrainfuckSequenceEnumerableTests
{
    readonly TestContext TestContext;

    void LogWriteLine(string message) => TestContext.OutputWriter.WriteLine(message);

    public BrainfuckSequenceEnumerableTests() => TestContext = TestContext.Current!;
    internal static IEnumerable<object?[]> GetEnumerableTestData
    {
        get
        {
            yield return GetEnumerableTest(
                source: string.Empty,
                expected: []
            );
            yield return GetEnumerableTest(
                source: "><+-.,[]",
                expected: [.. new[] {
                    (IncrementPointer, ">"),
                    (DecrementPointer, "<"),
                    (IncrementCurrent, "+"),
                    (DecrementCurrent, "-"),
                    (Output, "."),
                    (Input, ","),
                    (Begin, "["),
                    (End,"]"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2))]
            );
            yield return GetEnumerableTest(
                source: "😀😁😂🤣😃😄😅😆",
                options: new()
                {
                    IncrementPointer = "😀",
                    DecrementPointer = "😁",
                    IncrementCurrent = "😂",
                    DecrementCurrent = "🤣",
                    Output = "😃",
                    Input = "😄",
                    Begin = "😅",
                    End = "😆",
                },
                expected: [.. new[] {
                    (IncrementPointer, "😀"),
                    (DecrementPointer, "😁"),
                    (IncrementCurrent, "😂"),
                    (DecrementCurrent, "🤣"),
                    (Output, "😃"),
                    (Input, "😄"),
                    (Begin, "😅"),
                    (End, "😆"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2))]
            );

            yield return GetEnumerableTest(
                source: "test[]test",
                options: new TestShared.BrainfuckOptions(),
                expected: [.. new[]
                {
                    (Comment,  "test"),
                    (Begin, "["),
                    (End, "]"),
                    (Comment,  "test"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2))]
            );

            static object?[] GetEnumerableTest(string source, TestShared.BrainfuckOptions? options = default!, Tuple<BrainfuckSequence, string>[]? expected = null)
                => [source, options, (expected ?? [])];
        }
    }
    [Test]
    [MethodDataSource(nameof(GetEnumerableTestData))]
    public async Task GetEnumerableTest(string source, TestShared.BrainfuckOptions? options, Tuple<BrainfuckSequence, string>[] expected)
    {
        var actual = new BrainfuckSequenceEnumerable(source, options).ToArray();
        LogWriteLine($"expected:[{string.Join(", ", expected.Select(v => $"{v.Item1}:\"{v.Item2}\""))}] actual:[{string.Join(", ", actual.Select(v => $"{v.Sequence}:\"{v.Syntax}\""))}]");
        await Assert.That(actual.Select(v => (v.Sequence, v.Syntax.ToString())).ToArray())
            .IsEquivalentTo(expected.Select(v => (v.Item1, v.Item2)).ToArray(), CollectionOrdering.Matching);
    }
    [Test]
    public async Task RequiredInputTest()
    {
        var e1 = new BrainfuckSequenceEnumerable("[");
        await Assert.That(e1.RequiredInput).IsFalse();
        var e2 = new BrainfuckSequenceEnumerable(",");
        await Assert.That(e2.RequiredInput).IsTrue();
    }
    [Test]
    public async Task RequiredOutputTest()
    {
        var e1 = new BrainfuckSequenceEnumerable("]");
        await Assert.That(e1.RequiredOutput).IsFalse();
        var e2 = new BrainfuckSequenceEnumerable(".");
        await Assert.That(e2.RequiredOutput).IsTrue();
    }
    [Test]
    public async Task ToStringTest()
    {
        var e1 = new BrainfuckSequenceEnumerable(ReadOnlyMemory<char>.Empty);
        await Assert.That(e1.ToString()).IsNotNull();
        var e2 = new BrainfuckSequenceEnumerable("]");
        await Assert.That(e2.ToString()).IsNotNull();
    }
    [Test]
    public async Task EnumeratorTest()
    {
        var enumerator = ((System.Collections.IEnumerable)new BrainfuckSequenceEnumerable("[")).GetEnumerator();
        try
        {
            var e = enumerator;
            await Assert.That(e.MoveNext()).IsTrue();
            var syntax1 = (e.Current as (BrainfuckSequence, ReadOnlyMemory<char>)?)?.Item1;
            await Assert.That(syntax1).IsEqualTo(Begin);

            e.Reset();
            var syntax2 = (e.Current as (BrainfuckSequence, ReadOnlyMemory<char>)?)?.Item1;
            await Assert.That(syntax2).IsEqualTo(default(BrainfuckSequence));
        }
        finally
        {
            if (enumerator is IDisposable d) d.Dispose();
        }
    }
}
