using TUnit.Assertions.Enums;

namespace Esolang.Brainfuck.Generator.Sequences.Tests;

public class SequenceExtensionsTests
{
    readonly TestContext TestContext;
    void LogWriteLine(string message) => TestContext.OutputWriter.WriteLine(message);
    public SequenceExtensionsTests() => TestContext = TestContext.Current!;
    internal static IEnumerable<object?[]> NestAndUnNestTestData
    {
        get
        {
            yield return NestAndUnNestTest("+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.");
            yield return NestAndUnNestTest("++++++[>++++++++<-]++++++++++[>.+<-]");
            static object?[] NestAndUnNestTest(string source)
                => [source];
        }
    }
    [Test]
    [MethodDataSource(nameof(NestAndUnNestTestData))]
    public async Task NestAndUnNestTest(string source)
    {

        var expected = new BrainfuckSequenceEnumerable(source).Select((v, i) => new Sequence(i, v.Sequence, v.Syntax)).ToArray();
        LogWriteLine("expected : [" + string.Join(", ", (IEnumerable<Sequence>)expected) + "]");
        var nested = expected.Nest();
        LogWriteLine("nested :   [" + string.Join(", ", nested) + "]");
        var actual = nested.UnNest().ToArray();
        LogWriteLine("nested :   [" + string.Join(", ", (IEnumerable<Sequence>)actual) + "]");
        await Assert.That(actual).IsEquivalentTo(expected, CollectionOrdering.Matching);
    }
}
