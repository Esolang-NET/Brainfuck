using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
namespace Brainfuck.Tests;

[TestClass]
public class BrainfuckSequenceEnumerableTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> GetEnumerableTestData
    {
        get
        {
            yield return GetEnumerableTest(
                source: string.Empty,
                expected: Array.Empty<Tuple<BrainfuckSequence, string>>()
            );
            yield return GetEnumerableTest(
                source: "><+-.,[]",
                expected: new[] {
                    (IncrementPointer, ">"),
                    (DecrementPointer, "<"),
                    (IncrementCurrent, "+"),
                    (DecrementCurrent, "-"),
                    (Output, "."),
                    (Input, ","),
                    (Begin, "["),
                    (End,"]"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2)).ToArray()
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
                expected: new[] {
                    (IncrementPointer, "😀"),
                    (DecrementPointer, "😁"),
                    (IncrementCurrent, "😂"),
                    (DecrementCurrent, "🤣"),
                    (Output, "😃"),
                    (Input, "😄"),
                    (Begin, "😅"),
                    (End, "😆"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2)).ToArray()
            );

            yield return GetEnumerableTest(
                source: "test[]test",
                options: new TestShared.BrainfuckOptions(),
                expected: new[]
                {
                    (Comment,  "test"),
                    (Begin, "["),
                    (End, "]"),
                    (Comment,  "test"),
                }.Select(v => Tuple.Create(v.Item1, v.Item2)).ToArray()
            );

            static object?[] GetEnumerableTest(string source, TestShared.BrainfuckOptions? options = default!, Tuple<BrainfuckSequence, string>[]? expected = null)
                => new object?[] { source, options, (expected ?? Array.Empty<Tuple<BrainfuckSequence, string>>()).ToSerializable() };
        }
    }
    [TestMethod]
    [DynamicData(nameof(GetEnumerableTestData))]
    public void GetEnumerableTest(string source, TestShared.BrainfuckOptions? options, SerializableArrayWrapper<Tuple<BrainfuckSequence, string>> expected)
    {
        var actual = new BrainfuckSequenceEnumerable(source, options).ToArray();
        TestContext.WriteLine("expected:[{0}] actual:[{1}]",
            string.Join(", ", expected.Array.Select(v => $"{v.Item1}:\"{v.Item2}\"")),
            string.Join(", ", actual.Select(v => $"{v.Sequence}:\"{v.Syntax}\"")));
        CollectionAssert.AreEqual(expected.Array.Select(v => (v.Item1, v.Item2)).ToArray(), actual.Select(v => (v.Sequence, v.Syntax.ToString())).ToArray());
    }
    [TestMethod]
    public void RequiredInputTest()
    {
        var e1 = new BrainfuckSequenceEnumerable("[");
        Assert.AreEqual(e1.RequiredInput, false);
        var e2 = new BrainfuckSequenceEnumerable(",");
        Assert.AreEqual(e2.RequiredInput, true);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var e1 = new BrainfuckSequenceEnumerable("]");
        Assert.AreEqual(e1.RequiredOutput, false);
        var e2 = new BrainfuckSequenceEnumerable(".");
        Assert.AreEqual(e2.RequiredOutput, true);
    }
    [TestMethod]
    public void EnumeratorTest()
    {
        var enumerator = ((System.Collections.IEnumerable)new BrainfuckSequenceEnumerable("[")).GetEnumerator();
        try
        {
            var e = enumerator;
            Assert.AreEqual(true, e.MoveNext());
            var syntax1 = (e.Current as (BrainfuckSequence, ReadOnlyMemory<char>)?)?.Item1;
            Assert.AreEqual(Begin, syntax1);

            e.Reset();
            var syntax2 = (e.Current as (BrainfuckSequence, ReadOnlyMemory<char>)?)?.Item1;
            Assert.AreEqual(default(BrainfuckSequence), syntax2);
        }
        finally
        {
            if (enumerator is IDisposable d) d.Dispose();
        }
    }
}
