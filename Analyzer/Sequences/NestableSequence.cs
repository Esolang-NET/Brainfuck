using System.Collections;
using System.Text;

namespace Brainfuck.Analyzer.Sequences;

internal sealed record NestableSequence(IEnumerable<INestableSequence> Nest, Sequence Begin, Sequence End) : INestableSequence, IEnumerable<Sequence>
{
    public IEnumerator<Sequence> GetEnumerator()
    {
        yield return Begin;
        foreach (var sequence in Nest)
            if (sequence is Sequence sequence3)
                yield return sequence3;
            else if (sequence is NestableSequence nestable)
                foreach (var sequence2 in nestable)
                    yield return sequence2;
        yield return End;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    bool PrintMembers(StringBuilder builder)
    {
        builder.Append(nameof(Begin) + " = ");
        builder.Append(Begin);
        builder.Append(", " + nameof(Nest) + " = ");
        if (Nest != null)
        {
            builder.Append('[');
            builder.Append(string.Join(", ", Nest));
            builder.Append(']');
        }
        builder.Append(", " + nameof(End) + " = ");
        builder.Append(End);
        return true;
    }
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(NestableSequence) + " { ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
