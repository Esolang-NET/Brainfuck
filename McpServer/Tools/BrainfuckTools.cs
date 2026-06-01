using Esolang.Brainfuck.Processor;
using static Esolang.Processor.IOEvent;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Esolang.Brainfuck.McpServer.Tools;

/// <summary>
/// MCP tools for executing Brainfuck code.
/// </summary>
class BrainfuckTools
{
    [McpServerTool]
    [Description("Executes Brainfuck code and returns the collected output.")]
    public async Task<string> ExecuteBrainfuck(
        [Description("The Brainfuck source code to execute")] string source)
    {
        var processor = new BrainfuckProcessor(source);
        var output = new StringBuilder();

        await foreach (var ioEvent in processor.RunAsyncEnumerable())
        {
            if (ioEvent is OutputCharEvent outputEvent)
            {
                output.Append(outputEvent.Output);
            }
            else if (ioEvent is InputCharEvent inputEvent)
            {
                // Temporarily providing a placeholder as we work on proper Prompt integration
                inputEvent.Write('?');
            }
        }

        return output.ToString();
    }
}
