using MockBrainfuckOptions = TestShared.BrainfuckOptions;
namespace Esolang.Brainfuck.Tests;

public class BrainfuckOptionsTests
{
    [Test]
    public async Task BrainfuckOptionsTest_IBrainfuckOptions()
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
        await Assert.That(actual).IsEqualTo(expected);
    }
    [Test]
    public async Task IBrainfuckOptions_Equals()
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
        await Assert.That<IEquatable<IBrainfuckOptions>>(expected)
            .HasProperty(v => v.Equals(null!), false)
            .And.HasProperty(v => v.Equals(expected), true);
    }

}
