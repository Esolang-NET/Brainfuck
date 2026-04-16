using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Runtime.Serialization;

namespace TestShared.Tests;

[TestClass]
public class TestSharedCoverageTests
{
    [TestMethod]
    public void BrainfuckOptions_DefaultConstructor_UsesDefaults()
    {
        var options = new BrainfuckOptions();

        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.IncrementPointer, options.IncrementPointer);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.DecrementPointer, options.DecrementPointer);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.IncrementCurrent, options.IncrementCurrent);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.DecrementCurrent, options.DecrementCurrent);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.Output, options.Output);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.Input, options.Input);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.Begin, options.Begin);
        Assert.AreEqual(Esolang.Brainfuck.BrainfuckOptionsDefault.End, options.End);
    }

    [TestMethod]
    public void BrainfuckOptions_Copy_Works()
    {
        var source = new BrainfuckOptions("R", "L", "A", "B", "O", "I", "S", "E");
        var copied = new BrainfuckOptions((Esolang.Brainfuck.IBrainfuckOptions)source);

        var eq = ((IEquatable<Esolang.Brainfuck.IBrainfuckOptions>)copied)
            .Equals((Esolang.Brainfuck.IBrainfuckOptions)source);

        Assert.IsTrue(eq);
    }

#if NET48
    [TestMethod]
    public void BrainfuckOptions_SerializationRoundTrip_Works()
    {
        var source = new BrainfuckOptions("R", "L", "A", "B", "O", "I", "S", "E");
        var info = new SerializationInfo(typeof(BrainfuckOptions), new FormatterConverter());
        ((ISerializable)source).GetObjectData(info, new StreamingContext(StreamingContextStates.All));

        var restored = new BrainfuckOptions(info, new StreamingContext(StreamingContextStates.All));
        Assert.AreEqual(source, restored);
    }
#endif

    [TestMethod]
    public void ArrayWrapper_BasicBehavior_Works()
    {
        int[] raw = [1, 2, 3];
        Array<int> wrapped = raw;

        Assert.AreEqual(3, wrapped.Length);
        Assert.AreEqual(2, wrapped[1]);
        Assert.IsTrue(wrapped.Equals(raw));
        Assert.IsTrue(wrapped.Equals((object)wrapped));
        Assert.IsTrue(wrapped == (Array<int>)new[] { 1, 2, 3 });
        Assert.IsTrue(wrapped != (Array<int>)new[] { 1, 2, 4 });
        Assert.IsTrue(wrapped.GetHashCode() != 0);
        StringAssert.Contains(wrapped.ToString(), "Int32[] [ 1, 2, 3 ]");

        var list = ((IEnumerable<int>)wrapped).ToArray();
        CollectionAssert.AreEqual(raw, list);

        var nonGenericEnumerator = ((IEnumerable)wrapped).GetEnumerator();
        Assert.IsTrue(nonGenericEnumerator.MoveNext());
    }

    [TestMethod]
#if NET48
    public void ArrayWrapper_SerializationCtor_FallbacksToEmpty_WhenMissingValue()
    {
        var emptyInfo = new SerializationInfo(typeof(Array<int>), new FormatterConverter());
        emptyInfo.AddValue("InnerArray", null, typeof(int[]));
        var empty = new Array<int>(emptyInfo, new StreamingContext(StreamingContextStates.All));
        Assert.AreEqual(0, empty.Length);

        var info = new SerializationInfo(typeof(Array<int>), new FormatterConverter());
        var source = (Array<int>)new[] { 7, 8 };
        ((ISerializable)source).GetObjectData(info, new StreamingContext(StreamingContextStates.All));
        var restored = new Array<int>(info, new StreamingContext(StreamingContextStates.All));

        CollectionAssert.AreEqual(new[] { 7, 8 }, restored.AsArray());
    }
#else
    public void ArrayWrapper_SerializationCtor_FallbacksToEmpty_WhenMissingValue()
        => Assert.Inconclusive("SerializationInfo-based test is only run on net48.");
#endif

    [TestMethod]
    public void ArrayExtensions_Convert_AsExpected()
    {
        int[] source = [10, 20];
        var serializable = source.ToSerializable();
        var array = serializable.AsArray();
        var memory = serializable.AsMemory();

        CollectionAssert.AreEqual(source, array);
        CollectionAssert.AreEqual(source, memory.ToArray());
    }

    [TestMethod]
    public void AssemblyLoadContext_Dispose_CanBeCalledMultipleTimes()
    {
        var alc = new AssemblyLoadContext();
        alc.Dispose();
        alc.Dispose();
    }

#if NET48
    [TestMethod]
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
