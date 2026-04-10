using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD.Commands
{
    internal class VersionRestoreCommand : Command<VersionRestoreCommand.VersionRestoreSettings>
    {
        public override int Execute(CommandContext context, VersionRestoreSettings settings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public class VersionRestoreSettings : CommandSettings { }
    }
}
