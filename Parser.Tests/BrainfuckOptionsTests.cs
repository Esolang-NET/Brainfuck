using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockBrainfuckOptions = TestShared.BrainfuckOptions;
namespace Brainfuck.Tests;

[TestClass]
public class BrainfuckOptionsTests
{
    [TestMethod]
    public void BrainfuckOptionsTest_IBrainfuckOptions()
    {
        BrainfuckOptions expected = new()
        {
            IncrementPointer = "😀",
            DecrementPointer = "😁",
            IncrementCurrent = "😂",
            DecrementCurrent = "🤣",
            Output = "😃",
            Input = "😄",
            Begin = "😅",
            End = "😆",
        };
        MockBrainfuckOptions mock = new()
        {
            IncrementPointer = "😀",
            DecrementPointer = "😁",
            IncrementCurrent = "😂",
            DecrementCurrent = "🤣",
            Output = "😃",
            Input = "😄",
            Begin = "😅",
            End = "😆",
        };
        BrainfuckOptions actual = new(mock);
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    public void IBrainfuckOptions_Equals()
    {

        BrainfuckOptions expected = new()
        {
            IncrementPointer = "😀",
            DecrementPointer = "😁",
            IncrementCurrent = "😂",
            DecrementCurrent = "🤣",
            Output = "😃",
            Input = "😄",
            Begin = "😅",
            End = "😆",
        };
        MockBrainfuckOptions mock = new()
        {
            IncrementPointer = "😀",
            DecrementPointer = "😁",
            IncrementCurrent = "😂",
            DecrementCurrent = "🤣",
            Output = "😃",
            Input = "😄",
            Begin = "😅",
            End = "😆",
        };
        Assert.IsFalse(((IEquatable<IBrainfuckOptions?>)expected).Equals(null));
        Assert.IsTrue(((IEquatable<IBrainfuckOptions>)expected).Equals(mock));
    }

}
