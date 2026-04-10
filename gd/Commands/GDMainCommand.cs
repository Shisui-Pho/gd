using GD.Commands.Settings;
using GD.Utilities;
using Spectre.Console.Cli;

namespace GD.Commands
{
    internal class GDMainCommand : Command<DefaultSettings>
    {
        public override int Execute(CommandContext context, DefaultSettings settings, CancellationToken cancellationToken)
        {
            ConsoleMarkupUtility.PrintInfo("Welcome to the GD Command Line Interface!");
            return 0;
        }
    }
}
