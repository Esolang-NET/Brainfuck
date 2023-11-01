namespace TestShared;

/// <summary>
/// MSTest 向けの serialize な Array wrapper
/// </summary>
internal static class ArrayExtensions
{
    public static Array<T> ToSerializable<T>(this T[] array) => new(array);
    public static T[] AsArray<T>(this Array<T> array) => (T[])array;
    public static Memory<T> AsMemory<T>(this Array<T> array) => (T[])array;
}
