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
                source:string.Empty,
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
                    IncrementPointer= "😀",
                    DecrementPointer= "😁",
                    IncrementCurrent= "😂",
                    DecrementCurrent = "🤣",
                    Output = "😃",
                    Input= "😄",
                    Begin= "😅",
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
            static object?[] GetEnumerableTest(string source, BrainfuckOptions? options = default!, Tuple<BrainfuckSequence, string>[]? expected = null)
                => new object?[] { source, options, SerializableArrayWrapper.Create(expected ?? Array.Empty<Tuple<BrainfuckSequence, string>>()) };
        }
    }
    [TestMethod]
    [DynamicData(nameof(GetEnumerableTestData))]
    public void GetEnumerableTest(string source, BrainfuckOptions? options,  SerializableArrayWrapper<Tuple<BrainfuckSequence, string>> expected)
    {
        var actual = new BrainfuckSequenceEnumerable(source, options).ToArray();
        TestContext.WriteLine("expected:[{0}] actual:[{1}]",
            string.Join(", ",expected.Array.Select(v => $"{v.Item1}:\"{v.Item2}\"")), 
            string.Join(", ", actual.Select(v => $"{v.Sequence}:\"{v.Syntax}\"")));
        CollectionAssert.AreEqual(expected.Array.Select(v =>(v.Item1, v.Item2)).ToArray(), actual.Select(v => (v.Sequence, v.Syntax.ToString())).ToArray());
    }
}