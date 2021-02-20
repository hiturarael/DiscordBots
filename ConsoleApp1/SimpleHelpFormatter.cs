using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace Nine
{
    public class SimpleHelpFormatter : BaseHelpFormatter
    {
        private StringBuilder MessageBuilder { get; }

        public SimpleHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.MessageBuilder = new StringBuilder();
        }

        //called first, sets command
        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.MessageBuilder.Append("Command: ")
                .AppendLine(Formatter.Bold(command.Name))
                .AppendLine();

            this.MessageBuilder.Append("Description: ")
                .AppendLine(command.Description)
                .AppendLine();

            if (command is CommandGroup)
                this.MessageBuilder.AppendLine("This group has a standalone command.").AppendLine();

            this.MessageBuilder.Append("Aliases: ")
                .AppendLine(string.Join(", ", command.Aliases))
                .AppendLine();

            foreach(var overload in command.Overloads)
            {
                if(overload.Arguments.Count == 0)
                {
                    continue;
                }

                this.MessageBuilder.Append($"[Overload {overload.Priority}] Arguments: ")
                    .AppendLine(string.Join(", ", overload.Arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.Name})")))
                    .AppendLine();
            }

            return this;
        }

        //second - sets current group's subcommands
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            this.MessageBuilder.Append("Subcommands: ")
                .AppendLine(string.Join(", ", subcommands.Select(xc => xc.Name)))
                .AppendLine();

            return this;
        }

        //last called, produce final message and return
        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(this.MessageBuilder.ToString().Replace("\r\n", "\n"));
        }
    }
}
