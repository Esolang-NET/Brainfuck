using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainfuck.Generator.Sequences.Tests;
[TestClass]
public class SequenceExtensionsTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> NestAndUnNestTestData
    {
        get
        {
            yield return NestAndUnNestTest("+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.");
            yield return NestAndUnNestTest("++++++[>++++++++<-]++++++++++[>.+<-]");
            static object?[] NestAndUnNestTest(string source)
                => new object?[] { source };
        }
    }
    [TestMethod]
    [DynamicData(nameof(NestAndUnNestTestData))]
    public void NestAndUnNestTest(string source)
    {

        var expected = new BrainfuckSequenceEnumerable(source).Select((v, i) => new Sequence(i, v.Sequence, v.Syntax)).ToArray();
        TestContext.WriteLine("expected : [" + string.Join(", ", (IEnumerable<Sequence>)expected) + "]");
        var nested = expected.Nest();
        TestContext.WriteLine("nested :   [" + string.Join(", ", nested) + "]");
        var actual = nested.UnNest().ToArray();
        TestContext.WriteLine("nested :   [" + string.Join(", ", (IEnumerable<Sequence>)actual) + "]");
        CollectionAssert.AreEqual(expected, actual);
    }
}
