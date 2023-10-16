using System.Collections;

namespace Brainfuck;

public partial class BrainfuckSequenceEnumerable
{
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
            static (ReadOnlyMemory<char> Memory, (BrainfuckSequence Sequence, ReadOnlyMemory<char> Text) Current) Next(ReadOnlyMemory<char> memory, (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes)
            {
                int? max = null;
                ReadOnlyMemory<char> text;
                foreach (var (sequenceValue, syntax) in optionSyntaxes)
                {
                    var indexOf = memory.Span.IndexOf(syntax.Span, StringComparison.Ordinal);
                    if (indexOf < 0) continue;
                    if (indexOf > 0)
                    {
                        max = Math.Min(indexOf, max ?? int.MaxValue);
                        continue;
                    }
                    text = memory[..syntax.Length];
                    memory = memory[syntax.Length..];
                    return (memory, (sequenceValue, text));
                }
                max = Math.Min(memory.Length, max ?? int.MaxValue);
                text = memory[..max.Value];
                memory = memory[max.Value..];
                return (memory, (default, text));
            }
        }
        public void Dispose()
        {
            current = default;
            memory = default;
        }
        public void Reset() => throw new NotImplementedException();
    }
}
