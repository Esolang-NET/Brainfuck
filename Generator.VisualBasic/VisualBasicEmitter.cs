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

    public static string Emit(IEnumerable<(BrainfuckSequence Sequence, ReadOnlyMemory<char> Syntax)> sequence, int indent, string? outputVar, string? inputVar)
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
                    if (outputVar != null)
                        sb.AppendLine($"{indentString}{outputVar}.Write(ChrW(memory(pointer)))");
                    else
                        sb.AppendLine(EmitError("Output requested but no output variable provided", indent));
                    break;
                case BrainfuckSequence.Input:
                    if (inputVar != null)
                        sb.AppendLine($"{indentString}memory(pointer) = CByte({inputVar}.Read())");
                    else
                        sb.AppendLine(EmitError("Input requested but no input variable provided", indent));
                    break;
                case BrainfuckSequence.Begin:
                    sb.AppendLine($"{indentString}While memory(pointer) <> 0");
                    indent++;
                    indentString = new string(' ', indent * 4);
                    break;
                case BrainfuckSequence.End:
                    indent--;
                    indentString = new string(' ', indent * 4);
                    sb.AppendLine($"{indentString}End While");
                    break;
            }
            index++;
        }
        return sb.ToString();
    }
}
