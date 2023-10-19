using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// MSTest 向けの serialize な Array wrapper
/// </summary>
/// <typeparam name="T">要素型</typeparam>
[Serializable]
public readonly struct SerializableArrayWrapper<T> : ISerializable, IEquatable<T[]>, IEquatable<SerializableArrayWrapper<T>>
{
    public readonly T[] Array;
    public SerializableArrayWrapper(T[] array) => Array = array;

    public SerializableArrayWrapper(SerializationInfo info, StreamingContext context) => Array = info.GetValue(nameof(Array), typeof(T[])) as T[] ?? System.Array.Empty<T>();
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue(nameof(Array), Array, typeof(T[]));

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(typeof(T).Name).Append("[] [ ");
        if (Array.Length > 0)
        {
            builder.Append(string.Join(", ", Array));
            builder.Append(' ');
        }
        builder.Append(']');
        return builder.ToString();
    }

    public bool Equals(T[]? other) => other is not null && (Array.Equals(other) || Array.SequenceEqual(other));
    public bool Equals(SerializableArrayWrapper<T> other) => Equals(other.Array);

    public static implicit operator T[](SerializableArrayWrapper<T> other) => other.Array;

    public static implicit operator ReadOnlyMemory<T>(SerializableArrayWrapper<T> other) => other.Array;
    public static implicit operator SerializableArrayWrapper<T>(T[] array) => new(array);

    public override bool Equals(object? obj) => obj is SerializableArrayWrapper<T> wrapper && Equals(wrapper);

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
        hash = (hash * factor) + Array.Length.GetHashCode();
        foreach (var a in Array)
            hash = (hash * factor) + (a?.GetHashCode() ?? 0);
        return hash;
#endif
    }

    public static bool operator ==(SerializableArrayWrapper<T> left, SerializableArrayWrapper<T> right) => left.Equals(right);

    public static bool operator !=(SerializableArrayWrapper<T> left, SerializableArrayWrapper<T> right) => !(left == right);
}
/// <summary>
/// MSTest 向けの serialize な Array wrapper
/// </summary>
internal static class SerializableArrayWrapper
{
    public static SerializableArrayWrapper<T> ToSerializable<T>(this T[] array) => new(array);
}
