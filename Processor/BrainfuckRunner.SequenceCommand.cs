using Esolang.Brainfuck.Processor.SequenceCommands;
using System.Text;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor
{
    /// <summary>
    /// Wrapper that stores an executable command and its executed context.
    /// </summary>
    public sealed record SequenceCommand : BrainfuckSequenceCommand
    {
        /// <summary>
        /// Gets the command to execute.
        /// </summary>
        public BrainfuckSequenceCommand Command { get; init; }

        /// <summary>
        /// Gets the context after execution, or <see langword="null"/> if not executed yet.
        /// </summary>
        public BrainfuckContext? Executed { get; private set; }

        /// <summary>
        /// Initializes with a command and an existing executed context.
        /// </summary>
        /// <param name="Command">The command to execute.</param>
        /// <param name="Executed">The context after execution.</param>
        public SequenceCommand(BrainfuckSequenceCommand Command, BrainfuckContext? Executed) : base(ToContextAndCommand(ref Command))
        {
            this.Command = Command;
            this.Executed = Executed;
        }

        /// <summary>
        /// Initializes with a command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
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

        /// <summary>
        /// Deconstructs and returns the internally stored values.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="executed">The context after execution.</param>
        public void Deconstruct(out BrainfuckSequenceCommand command, out BrainfuckContext? executed) => (command, executed) = (Command, Executed);

        /// <inheritdoc />
        public override async ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = await Command.ExecuteAsync(cancellationToken);
            Executed = result;
            return result;
        }

        /// <inheritdoc />
        public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
        {
            var result = Command.Execute(cancellationToken);
            Executed = result;
            return result;
        }

        /// <inheritdoc />
        public override bool RequiredInput => Command.RequiredInput;

        /// <inheritdoc />
        public override bool RequiredOutput => Command.RequiredOutput;

        /// <inheritdoc />
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
