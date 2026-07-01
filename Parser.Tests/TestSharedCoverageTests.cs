using TestShared;

namespace Esolang.Brainfuck.Parser.Tests;

public class TestSharedCoverageTests
{
    [Test]
    public async Task BrainfuckOptions_DefaultConstructor_UsesDefaults()
    {
        var options = new BrainfuckOptions();

        await Assert.That(options.IncrementPointer).IsEqualTo(BrainfuckOptionsDefault.IncrementPointer);
        await Assert.That(options.DecrementPointer).IsEqualTo(BrainfuckOptionsDefault.DecrementPointer);
        await Assert.That(options.IncrementCurrent).IsEqualTo(BrainfuckOptionsDefault.IncrementCurrent);
        await Assert.That(options.DecrementCurrent).IsEqualTo(BrainfuckOptionsDefault.DecrementCurrent);
        await Assert.That(options.Output).IsEqualTo(BrainfuckOptionsDefault.Output);
        await Assert.That(options.Input).IsEqualTo(BrainfuckOptionsDefault.Input);
        await Assert.That(options.Begin).IsEqualTo(BrainfuckOptionsDefault.Begin);
        await Assert.That(options.End).IsEqualTo(BrainfuckOptionsDefault.End);
    }

    [Test]
    public async Task BrainfuckOptions_Copy_Works()
    {
        var source = new BrainfuckOptions("R", "L", "A", "B", "O", "I", "S", "E");
        var copied = new BrainfuckOptions(source);

        var eq = ((IEquatable<IBrainfuckOptions>)copied)
            .Equals(source);

        await Assert.That(eq).IsTrue();
    }

    [Test]
    public void AssemblyLoadContext_Dispose_CanBeCalledMultipleTimes()
    {
        var alc = new AssemblyLoadContext();
        alc.Dispose();
        alc.Dispose();
    }

#if !NET
    [Test]
    public void AssemblyLoadContext_LoadFromStream_ThrowsOnNullAssembly()
    {
        var alc = new AssemblyLoadContext();
        try
        {
            try
            {
                alc.LoadFromStream(null!);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // expected
            }
        }
        finally
        {
            alc.Dispose();
        }
    }
#endif
}
