using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Analyzer.Sequences;
internal static class SequenceExtensions
{
    /// <summary>
    /// enumerable sequence to nestable sequence
    /// </summary>
    /// <param name="sequences"></param>
    /// <returns></returns>
    public static IEnumerable<INestableSequence> Nest(this Sequence[] sequences) => Nest((ReadOnlyMemory<Sequence>)sequences);
    /// <summary>
    /// enumerable sequence to nestable sequence
    /// </summary>
    /// <param name="sequences"></param>
    /// <returns></returns>
    public static IEnumerable<INestableSequence> Nest(this Memory<Sequence> sequences) => Nest((ReadOnlyMemory<Sequence>)sequences);
    /// <summary>
    /// enumerable sequence to nestable sequence
    /// </summary>
    /// <param name="sequences"></param>
    /// <returns></returns>
    public static IEnumerable<INestableSequence> Nest(this ReadOnlyMemory<Sequence> sequences)
    {
        while (sequences.Length > 0)
        {
            var current = sequences.Span[0];
            sequences = sequences[1..];
            if (current is not { Value: Begin })
            {
                yield return current;
                continue;
            }
            if (!TryGetPairEnd(sequences, out var nest, out var end))
            {
                yield return current;
                continue;
            }
            sequences = sequences[(nest.Length + 1)..];
            yield return new NestableSequence(Nest(nest), current, end);
        }
    }
    public static IEnumerable<Sequence> UnNest(this IEnumerable<INestableSequence> sequences)
    {
        foreach (var sequence in sequences)
        {
            if (sequence is Sequence sequence1)
                yield return sequence1;
            else if (sequence is NestableSequence nestableSequence)
                foreach (var sequence2 in nestableSequence)
                    yield return sequence2;
        }
    }
    public static bool TryGetPairEnd(ReadOnlyMemory<Sequence> sequences, out ReadOnlyMemory<Sequence> nest, out Sequence end)
    {
        nest = ReadOnlyMemory<Sequence>.Empty;
        end = null!;
        var inc = 0;
        for (var i = 0; i < sequences.Length; i++)
        {
            var current = sequences.Span[i];
            if (current is not { Value: Begin or End }) continue;
            if (current is { Value: Begin })
            {
                inc++;
                continue;
            }
            if (current is { Value: End })
            {
                if (inc > 0)
                {
                    inc--;
                    continue;
                }
                nest = sequences[..Math.Max(i, 0)];
                end = current;
                return true;
            }
        }
        return false;
    }

}
