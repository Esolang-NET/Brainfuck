using System.Collections;

namespace Esolang.Brainfuck;

public sealed partial record BrainfuckSequenceEnumerable
{

    /// <summary>
    /// brainfuck source to enumerator.
    /// </summary>
    public struct Enumerator : IEnumerator<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Text)>
    {
        readonly ReadOnlyMemory<char> original;
        (BrainfuckSequence, ReadOnlyMemory<char>) current;
        ReadOnlyMemory<char> memory;

        readonly (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes;

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public readonly (BrainfuckSequence, ReadOnlyMemory<char>) Current => current;

        readonly object IEnumerator.Current => current;
        internal Enumerator(ReadOnlyMemory<char> memory, (BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)[] optionSyntaxes)
        {
            original = memory;
            this.memory = memory;
            this.optionSyntaxes = optionSyntaxes;
        }

        /// <inheritdoc cref="IEnumerator.MoveNext"/>
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

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            current = default;
            memory = default;
        }

        /// <inheritdoc cref="IEnumerator.Reset"/>
        public void Reset()
        {
            memory = original;
            current = default;
        }
    }
}
