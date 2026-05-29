using System.Collections;
using TestShared;

namespace Esolang.Brainfuck.Parser.Tests;

[TestClass]
public class TestSharedCoverageTests
{
    [TestMethod]
    public void BrainfuckOptions_DefaultConstructor_UsesDefaults()
    {
        var options = new BrainfuckOptions();

        Assert.AreEqual(BrainfuckOptionsDefault.IncrementPointer, options.IncrementPointer);
        Assert.AreEqual(BrainfuckOptionsDefault.DecrementPointer, options.DecrementPointer);
        Assert.AreEqual(BrainfuckOptionsDefault.IncrementCurrent, options.IncrementCurrent);
        Assert.AreEqual(BrainfuckOptionsDefault.DecrementCurrent, options.DecrementCurrent);
        Assert.AreEqual(BrainfuckOptionsDefault.Output, options.Output);
        Assert.AreEqual(BrainfuckOptionsDefault.Input, options.Input);
        Assert.AreEqual(BrainfuckOptionsDefault.Begin, options.Begin);
        Assert.AreEqual(BrainfuckOptionsDefault.End, options.End);
    }

    [TestMethod]
    public void BrainfuckOptions_Copy_Works()
    {
        var source = new BrainfuckOptions("R", "L", "A", "B", "O", "I", "S", "E");
        var copied = new BrainfuckOptions(source);

        var eq = ((IEquatable<IBrainfuckOptions>)copied)
            .Equals(source);

        Assert.IsTrue(eq);
    }

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
        Assert.AreNotEqual(0, wrapped.GetHashCode());
        Assert.Contains("Int32[] [ 1, 2, 3 ]", wrapped.ToString());

        var list = ((IEnumerable<int>)wrapped).ToArray();
        CollectionAssert.AreEqual(raw, list);

        var nonGenericEnumerator = ((IEnumerable)wrapped).GetEnumerator();
        Assert.IsTrue(nonGenericEnumerator.MoveNext());
    }

    [TestMethod]
#if !NET
    public void ArrayWrapper_SerializationCtor_FallbacksToEmpty_WhenMissingValue()
    {
        var emptyInfo = new System.Runtime.Serialization.SerializationInfo(typeof(Array<int>), new System.Runtime.Serialization.FormatterConverter());
        emptyInfo.AddValue("InnerArray", null, typeof(int[]));
        var empty = new Array<int>(emptyInfo, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All));
        Assert.AreEqual(0, empty.Length);

        var info = new System.Runtime.Serialization.SerializationInfo(typeof(Array<int>), new System.Runtime.Serialization.FormatterConverter());
        var source = (Array<int>)new[] { 7, 8 };
        ((System.Runtime.Serialization.ISerializable)source).GetObjectData(info, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All));
        var restored = new Array<int>(info, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All));

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

#if !NET
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
