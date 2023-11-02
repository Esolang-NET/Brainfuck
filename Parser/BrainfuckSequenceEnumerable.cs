using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Esolang.Brainfuck;

/// <summary>
/// brainfuck source to sequence parser.
/// </summary>
/// <param name="Source">brainfuck source code</param>
/// <param name="Options">brainfuck parse options</param>
public sealed partial record BrainfuckSequenceEnumerable(ReadOnlyMemory<char> Source, BrainfuckOptions Options) : IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>
{
    /// <summary>
    /// brainfuck source code
    /// </summary>
    public readonly ReadOnlyMemory<char> Source = Source;

    /// <summary>
    /// brainfuck parse options
    /// </summary>
    public readonly BrainfuckOptions Options = Options;

    /// <summary>
    /// brainfuck source to sequence parser.
    /// </summary>
    /// <param name="source"></param>
    public BrainfuckSequenceEnumerable(string source) : this(source.AsMemory()) { }

    /// <summary>
    /// brainfuck source to sequence parser.
    /// </summary>
    /// <param name="source"></param>
    public BrainfuckSequenceEnumerable(ReadOnlyMemory<char> source) : this(source, null) { }

    /// <summary>
    /// brainfuck source to sequence parser.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="options"></param>
    public BrainfuckSequenceEnumerable(string source, BrainfuckOptions? options) : this(source.AsMemory(), options) { }

    /// <summary>
    /// brainfuck source to sequence parser.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="options"></param>
    public BrainfuckSequenceEnumerable(string source, IBrainfuckOptions? options) : this(source.AsMemory(), options) { }

    /// <summary>
    /// brainfuck source to sequence parser.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="options"></param>
    public BrainfuckSequenceEnumerable(ReadOnlyMemory<char> source, IBrainfuckOptions? options = null)
        : this(source, options switch
        {
            BrainfuckOptions bo => bo,
            not null => new(options),
            _ => new(),
        })
    { }

    [MemberNotNull(nameof(_needInput), nameof(_needOutput))]
    void InitializeRequired()
    {
        if (_needInput is not null && _needOutput is not null) return;
        foreach (var (sequence, _) in this)
        {
            if (_needInput is not null && _needOutput is not null)
                return;
            if (_needInput is null && sequence is BrainfuckSequence.Input) _needInput = true;
            if (_needOutput is null && sequence is BrainfuckSequence.Output) _needOutput = true;
        }
        _needInput ??= false;
        _needOutput ??= false;
    }
    bool? _needInput;

    /// <summary>
    /// <see cref="Source"/> is in required input.
    /// </summary>
    public bool RequiredInput
    {
        get
        {
            InitializeRequired();
            return _needInput.Value;
        }
    }

    bool? _needOutput;

    /// <summary>
    /// <see cref="Source"/> is in required output.
    /// </summary>
    public bool RequiredOutput
    {
        get
        {
            InitializeRequired();
            return _needOutput.Value;
        }
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Enumerator GetEnumerator() => new(Source, OptionSyntaxes.OrderByDescending(v => v.Syntax.Length).ToArray());
    IEnumerator<(BrainfuckSequence, ReadOnlyMemory<char>)> IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>.GetEnumerator() => GetEnumerator();
    IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)> OptionSyntaxes
    {
        get
        {
            if (!string.IsNullOrEmpty(Options.IncrementPointer))
                yield return (BrainfuckSequence.IncrementPointer, Options.IncrementPointer.AsMemory());
            if (!string.IsNullOrEmpty(Options.DecrementPointer))
                yield return (BrainfuckSequence.DecrementPointer, Options.DecrementPointer.AsMemory());
            if (!string.IsNullOrEmpty(Options.IncrementCurrent))
                yield return (BrainfuckSequence.IncrementCurrent, Options.IncrementCurrent.AsMemory());
            if (!string.IsNullOrEmpty(Options.DecrementCurrent))
                yield return (BrainfuckSequence.DecrementCurrent, Options.DecrementCurrent.AsMemory());
            if (!string.IsNullOrEmpty(Options.Input))
                yield return (BrainfuckSequence.Input, Options.Input.AsMemory());
            if (!string.IsNullOrEmpty(Options.Output))
                yield return (BrainfuckSequence.Output, Options.Output.AsMemory());
            if (!string.IsNullOrEmpty(Options.Begin))
                yield return (BrainfuckSequence.Begin, Options.Begin.AsMemory());
            if (!string.IsNullOrEmpty(Options.End))
                yield return (BrainfuckSequence.End, Options.End.AsMemory());
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool PrintMembers(StringBuilder builder)
    {
        builder.Append(nameof(Source) + ": ");
        if (!Source.IsEmpty)
            builder.Append(string.Join("", Source));
        builder.Append(", " + nameof(Options) + ": ");
        builder.Append(Options.ToString());
        if (!Source.IsEmpty)
        {
            builder.Append(", " + nameof(RequiredInput) + ": ");
            builder.Append(RequiredInput);
            builder.Append(", " + nameof(RequiredOutput) + ": ");
            builder.Append(RequiredOutput);
        }
        return true;
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(BrainfuckSequenceEnumerable) + "{ ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
