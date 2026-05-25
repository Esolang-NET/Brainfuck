using System.Text;
using Esolang.Brainfuck;

namespace Esolang.Brainfuck.Generator;

internal static class VisualBasicEmitter
{
    public static string EmitError(string message, int indent)
    {
        var indentString = new string(' ', indent * 4);
        const string newLine = "\r\n";
        return $"{indentString}' Error: {message}{newLine}{indentString}Throw New System.InvalidOperationException(\"{message}\")";
    }

    public static string Emit(IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)> sequence, int indent)
    {
        var sb = new StringBuilder();
        var indentString = new string(' ', indent * 4);
        var index = 0;
        foreach (var (command, syntax) in sequence)
        {
            var commentSyntax = syntax.ToString().Replace("\r", "\\r").Replace("\n", "\\n");
            var comment = $"{indentString}' {index} {command}:{commentSyntax}";
            sb.AppendLine(comment);
            
            switch (command)
            {
                case BrainfuckSequence.IncrementPointer:
                    sb.AppendLine($"{indentString}pointer += 1");
                    break;
                case BrainfuckSequence.DecrementPointer:
                    sb.AppendLine($"{indentString}pointer -= 1");
                    break;
                case BrainfuckSequence.IncrementCurrent:
                    sb.AppendLine($"{indentString}memory(pointer) += 1");
                    break;
                case BrainfuckSequence.DecrementCurrent:
                    sb.AppendLine($"{indentString}memory(pointer) -= 1");
                    break;
                case BrainfuckSequence.Output:
                    sb.AppendLine($"{indentString}output.Write(ChrW(memory(pointer)))");
                    break;
                case BrainfuckSequence.Input:
                    sb.AppendLine($"{indentString}memory(pointer) = CByte(input.Read())");
                    break;
            }
            index++;
        }
        return sb.ToString();
    }
}
