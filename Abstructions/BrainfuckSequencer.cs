using System.Collections;

namespace Brainfuck;

public class BrainfuckSequencer : IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>
{
    readonly BrainfuckOptions options;
    readonly ReadOnlyMemory<char> input;
    public BrainfuckSequencer(string input) : this(input.AsMemory(), null) { }
    public BrainfuckSequencer(ReadOnlyMemory<char> input) : this(input, null) { }
    public BrainfuckSequencer(string input, BrainfuckOptions? options):this(input.AsMemory(), options) { }
    public BrainfuckSequencer(ReadOnlyMemory<char> input, BrainfuckOptions? options)
    {
        this.input = input;
        this.options = options is not null ? new(options) : new();
    }

    public Enumerator GetEnumerator() => new(input, OptionSyntaxes.OrderByDescending(v => v.Syntax.Length).ToArray());
    IEnumerator<(BrainfuckSequence, ReadOnlyMemory<char>)> IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)>.GetEnumerator() => GetEnumerator();
    IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)> OptionSyntaxes
    {
        get
        {
            yield return (BrainfuckSequence.IncrementPointer, options.IncrementPointer.AsMemory());
            yield return (BrainfuckSequence.DecrementPointer, options.DecrementPointer.AsMemory());
            yield return (BrainfuckSequence.IncrementCurrent, options.IncrementCurrent.AsMemory());
            yield return (BrainfuckSequence.DecrementCurrent, options.DecrementCurrent.AsMemory());
            yield return (BrainfuckSequence.Input, options.Input.AsMemory());
            yield return (BrainfuckSequence.Output, options.Output.AsMemory());
            yield return (BrainfuckSequence.Begin, options.Begin.AsMemory());
            yield return (BrainfuckSequence.End, options.End.AsMemory());
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public struct Enumerator : IEnumerator<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Text)>
    {
        (BrainfuckSequence, ReadOnlyMemory<char>) current;
        ReadOnlyMemory<char> memory;
        
        readonly (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes;
        public readonly (BrainfuckSequence, ReadOnlyMemory<char>) Current => current;

        readonly object IEnumerator.Current => current;
        internal Enumerator(ReadOnlyMemory<char> memory, (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes)
        {
            this.memory = memory;
            this.optionSyntaxes = optionSyntaxes;
        }

        public bool MoveNext()
        {
            if (memory.Length <= 0) return false;
            (memory, current) = Next(memory, optionSyntaxes);
            return true;
            static (ReadOnlyMemory<char> Memory,(BrainfuckSequence Sequence, ReadOnlyMemory<char> Text) Current) Next(ReadOnlyMemory<char> memory, (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes)
            {
                int? max = null;
                ReadOnlyMemory<char> text;
                foreach (var (sequenceValue, syntax) in optionSyntaxes)
                {
                    var indexOf = memory.Span.IndexOf(syntax.Span, StringComparison.Ordinal);
                    if (indexOf > 0)
                    {
                        max = Math.Min(indexOf, max ?? int.MaxValue);
                        continue;
                    }
                    text = memory.Slice(0, syntax.Length);
                    memory = memory.Slice(syntax.Length);
                    return (memory, (sequenceValue, text));
                }
                max = Math.Min(memory.Length, max ?? int.MaxValue);
                text = memory.Slice(0, max.Value);
                memory = memory.Slice(max.Value);
                return (memory , (default, text));
            }
        }
        public readonly void Dispose() { }
        public void Reset() => throw new NotImplementedException();
    }
}
