using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace Brainfuck.TestShared;
/// <summary>
/// MSTest 向けの serialize な Array wrapper
/// </summary>
/// <typeparam name="T">要素型</typeparam>
[Serializable]
public readonly struct Array<T> : ISerializable, IEquatable<T[]>, IEquatable<Array<T>>, IReadOnlyList<T>
{
    readonly T[] InnerArray;
    public readonly int Length => InnerArray.Length;

    int IReadOnlyCollection<T>.Count => Length;

    public T this[int index] => InnerArray[index];

    public Array(T[] array) => InnerArray = array;

    public Array(SerializationInfo info, StreamingContext context) => InnerArray = info.GetValue(nameof(InnerArray), typeof(T[])) as T[] ?? System.Array.Empty<T>();
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue(nameof(InnerArray), InnerArray, typeof(T[]));

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(typeof(T).Name).Append("[] [ ");
        if (InnerArray.Length > 0)
        {
            builder.Append(string.Join(", ", InnerArray));
            builder.Append(' ');
        }
        builder.Append(']');
        return builder.ToString();
    }

    public bool Equals(T[]? other) => other is not null && (InnerArray.Equals(other) || InnerArray.SequenceEqual(other));
    public bool Equals(Array<T> other) => Equals(other.InnerArray);

    public static implicit operator T[](Array<T> other) => other.InnerArray;

    public static implicit operator Array<T>(T[] array) => new(array);

    public override bool Equals(object? obj) => obj is Array<T> wrapper && Equals(wrapper);

    public override int GetHashCode()
    {
#if NETSTANDARD2_1_OR_GREATER
        var hash = new HashCode();
        hash.Add(Array.Length);
        foreach (var a in Array)
            hash.Add(a);
        return hash.ToHashCode();
#else
        var seed = 1009;
        var factor = 9176;
        var hash = seed;
        hash = (hash * factor) + InnerArray.Length.GetHashCode();
        foreach (var a in InnerArray)
            hash = (hash * factor) + (a?.GetHashCode() ?? 0);
        return hash;
#endif
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)InnerArray).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => InnerArray.GetEnumerator();

    public static bool operator ==(Array<T> left, Array<T> right) => left.Equals(right);

    public static bool operator !=(Array<T> left, Array<T> right) => !(left == right);
}
/// <summary>
/// MSTest 向けの serialize な Array wrapper
/// </summary>
internal static class SerializableArrayWrapper
{
    public static Array<T> ToSerializable<T>(this T[] array) => new(array);
    public static T[] AsArray<T>(this Array<T> array) => (T[])array;
    public static Memory<T> AsMemory<T>(this Array<T> array) => (T[])array;
}
