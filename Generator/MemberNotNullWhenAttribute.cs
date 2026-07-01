
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します
namespace System.Diagnostics.CodeAnalysis;
#if NETSTANDARD2_0
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
sealed class MemberNotNullWhenAttribute : Attribute
{
    public MemberNotNullWhenAttribute(bool returnValue, params string[] members) { }
    public bool ReturnValue { get; }
    public string[] Members { get; }
}
#endif

#pragma warning restore IDE0060 // 未使用のパラメーターを削除します
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
