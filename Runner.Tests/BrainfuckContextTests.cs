using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace Brainfuck.Runner.Tests;

[TestClass]
public class BrainfuckContextTests
{
    public TestContext TestContext { get; set; } = default!;
    [TestMethod]
    public void ToStringTest()
    {
        var context1 = new BrainfuckContext();
        Assert.IsNotNull(context1.ToString());

        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: ImmutableArray.Create<byte>(0)
        );
        Assert.IsNotNull(context2.ToString());

        BrainfuckContext context3 = default;

        Assert.IsNotNull(context3.ToString());
    }
    [TestMethod]
    public void EqualsTest()
    {

        var context1 = new BrainfuckContext();
        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: ImmutableArray.Create<byte>(0)
        );
        BrainfuckContext context3 = default;
        Assert.AreNotEqual(context1, context2);
        Assert.AreEqual(context1, context3);
    }
    [TestMethod]
    public void GetHashCodeTest()
    {

        var context1 = new BrainfuckContext();
        var context2 = new BrainfuckContext(
            Sequences: new[] { BrainfuckSequence.Comment },
            Stack: ImmutableArray.Create<byte>(0)
        );
        BrainfuckContext context3 = default;
        var hashCode1 = context1.GetHashCode();
        var hashCode2 = context2.GetHashCode();
        var hashCode3 = context3.GetHashCode();
        TestContext.WriteLine($"{nameof(context1)}:{hashCode1}");
        TestContext.WriteLine($"{nameof(context2)}:{hashCode2}");
        TestContext.WriteLine($"{nameof(context3)}:{hashCode3}");
        Assert.AreNotEqual(hashCode1, hashCode2, $"{nameof(hashCode1)} != {nameof(hashCode2)}");
        Assert.AreEqual(hashCode1, hashCode3, $"{nameof(hashCode1)} == {nameof(hashCode3)}");
    }
}
