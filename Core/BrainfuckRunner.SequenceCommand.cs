using Brainfuck.Core.SequenceCommands;
using System.Text;

namespace Brainfuck;

public sealed partial class BrainfuckRunner
{
    internal sealed record SequenceCommand : BrainfuckSequenceCommand
    {
        private readonly BrainfuckSequenceCommand Command;
        public Type CommandType => Command.GetType();
        public BrainfuckContext? Executed;
        public SequenceCommand(BrainfuckSequenceCommand command) : base((BrainfuckContext)command) => Command = command;
        public SequenceCommand(BrainfuckSequenceCommand command, BrainfuckContext? executed) : base((BrainfuckContext)command) => (Command, Executed) = (command, executed);
        public void Deconstruct(out BrainfuckSequenceCommand command, out BrainfuckContext? executed) => (command, executed) = (Command, Executed);

        public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = await Command.ExecuteAsync(cancellationToken);
            Executed = result;
            return result;
        }
        public override bool RequiredInput => Command.RequiredInput;
        public override bool RequiredOutput => Command.RequiredOutput;
        protected override bool PrintMembers(StringBuilder builder)
        {
            builder.Append(nameof(Command) + " = ");
            builder.Append(Command);
            builder.Append(", " + nameof(Executed) + " = ");
            builder.Append(Executed);
            return true;
        }
    }
}
