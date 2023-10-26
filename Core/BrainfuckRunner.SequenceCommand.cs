using Brainfuck.Core.SequenceCommands;
using System.Text;

namespace Brainfuck;

public sealed partial class BrainfuckRunner
{
    public sealed record SequenceCommand : BrainfuckSequenceCommand
    {
        public BrainfuckSequenceCommand Command { get; init; }
        public BrainfuckContext? Executed { get; private set; }
        public SequenceCommand(BrainfuckSequenceCommand Command, BrainfuckContext? Executed) : base(ToContextAndCommand(ref Command))
        {
            this.Command = Command;
            this.Executed = Executed;
        }
        public SequenceCommand(BrainfuckSequenceCommand command) : this(command, null) { }
        static BrainfuckContext ToContextAndCommand(ref BrainfuckSequenceCommand command)
        {
#if NET5_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            if (command is null) throw new ArgumentNullException(nameof(command));
#endif
            while (command is SequenceCommand command_)
                (command, _) = command_;
            return command;
        }

        public void Deconstruct(out BrainfuckSequenceCommand command, out BrainfuckContext? executed) => (command, executed) = (Command, Executed);

        public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = await Command.ExecuteAsync(cancellationToken);
            Executed = result;
            return result;
        }
        public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
        {
            var result = Command.Execute(cancellationToken);
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
