using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Brainfuck;

public partial class BrainfuckSequenceEnumerable : IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>
{
    readonly BrainfuckOptions options;
    readonly ReadOnlyMemory<char> source;
    public BrainfuckSequenceEnumerable(string source) : this(source.AsMemory(), null) { }
    public BrainfuckSequenceEnumerable(ReadOnlyMemory<char> source) : this(source, null) { }

    public BrainfuckSequenceEnumerable(string source, BrainfuckOptions? options) : this(source.AsMemory(), options) { }
    public BrainfuckSequenceEnumerable(ReadOnlyMemory<char> source, BrainfuckOptions? options)
    {
        this.source = source;
        this.options = options ?? new();
    }
    public BrainfuckSequenceEnumerable(string source, IBrainfuckOptions? options) : this(source.AsMemory(), options) { }
    public BrainfuckSequenceEnumerable(ReadOnlyMemory<char> source, IBrainfuckOptions? options)
    {
        this.source = source;
        this.options = options switch
        {
            BrainfuckOptions bo => bo,
            not null => new(options),
            _ => new(),
        };
    }
    [MemberNotNull(nameof(_needInput), nameof(_needOutput))]
    void InitializeNeeds()
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
    public bool NeedInput
    {
        get
        {
            InitializeNeeds();
            return _needInput.Value;
        }
    }

    bool? _needOutput;
    public bool NeedOutput
    {
        get
        {
            InitializeNeeds();
            return _needOutput.Value;
        }
    }

    public Enumerator GetEnumerator() => new(source, OptionSyntaxes.OrderByDescending(v => v.Syntax.Length).ToArray());
    IEnumerator<(BrainfuckSequence, ReadOnlyMemory<char>)> IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>.GetEnumerator() => GetEnumerator();
    IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)> OptionSyntaxes
    {
        get
        {
            if (!string.IsNullOrEmpty(options.IncrementPointer))
                yield return (BrainfuckSequence.IncrementPointer, options.IncrementPointer.AsMemory());
            if (!string.IsNullOrEmpty(options.DecrementPointer))
                yield return (BrainfuckSequence.DecrementPointer, options.DecrementPointer.AsMemory());
            if (!string.IsNullOrEmpty(options.IncrementCurrent))
                yield return (BrainfuckSequence.IncrementCurrent, options.IncrementCurrent.AsMemory());
            if (!string.IsNullOrEmpty(options.DecrementCurrent))
                yield return (BrainfuckSequence.DecrementCurrent, options.DecrementCurrent.AsMemory());
            if (!string.IsNullOrEmpty(options.Input))
                yield return (BrainfuckSequence.Input, options.Input.AsMemory());
            if (!string.IsNullOrEmpty(options.Output))
                yield return (BrainfuckSequence.Output, options.Output.AsMemory());
            if (!string.IsNullOrEmpty(options.Begin))
                yield return (BrainfuckSequence.Begin, options.Begin.AsMemory());
            if (!string.IsNullOrEmpty(options.End))
                yield return (BrainfuckSequence.End, options.End.AsMemory());
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
