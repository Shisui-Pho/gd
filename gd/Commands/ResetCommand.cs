using Spectre.Console.Cli;
namespace GD.Commands;
internal class ResetCommand : Command<ResetCommand.ResetSettings>
{
    public override int Execute(CommandContext context, ResetSettings settings, CancellationToken cancellationToken)
    {
        return -1;
    }

    public class ResetSettings : CommandSettings { }
}
