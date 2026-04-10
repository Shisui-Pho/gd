using GD.Commands;
using GD.Infrastructure;
using GD.Infrastructure.Abstractions;
using GD.Services;
using GD.Services.Abstractions;
using GD.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

//DPI
var services = new ServiceCollection();
services.AddSingleton<IGDListService, GDListService>();
services.AddSingleton<IGDVersionResolver, GDVersionResolver>();
services.AddSingleton<IGDConfigurationsFactory, GDConfigurationsFactory>();
services.AddSingleton<IGodotVersionsManager, GodotVersionsManager>();
services.AddSingleton<IGDInstallService, GDInstallService>();
services.AddSingleton<IGodotReleaseResolver, GodotReleaseResolver>();

var register = new TypeRegistrar(services);

var app = new CommandApp<GDMainCommand>(register);

var cancellationTokenSource = new CancellationTokenSource();

//For Ctrl + C
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

app.Configure(config =>
{
    config.AddCommand<ListCommand>("list")
      .WithDescription("Displays all installed Godot versions on this machine.");

    config.AddCommand<RunCommand>("run")
          .WithDescription("Launches the Godot engine in GUI or console mode.");

    config.AddCommand<UseCommand>("use")
          .WithDescription("Sets the default Godot version for the current directory or globally.");

    config.AddCommand<InstallCommand>("install")
        .WithDescription("Downloads and installs a specified version of the Godot engine.");

    config.AddCommand<RemoveCommand>("remove")
        .WithDescription("Uninstalls a specified version of the Godot engine.");

    config.AddBranch("config", c =>
    {
        c.AddCommand<RepairCommand>("repair")
            .WithDescription("Repairs the GD config files.");
    });

    config.AddCommand<VersionRestoreCommand>("restore")
          .WithDescription("Restores/install Godot versions from the GD Config files.");

    config.AddCommand<ResetCommand>("reset");

    config.SetExceptionHandler((ex, _) =>
    {
        ConsoleMarkupUtility.HandleException(ex);

        return ex switch
        {
            ArgumentException => ExitCodes.InvalidArguments,
            FileNotFoundException => ExitCodes.NotFound,
            HttpRequestException => ExitCodes.NetworkError,
            UnauthorizedAccessException => ExitCodes.PermissionDenied,
            _ => ExitCodes.Error
        };
    });
}); 

int exitCode = app.Run(args, cancellationTokenSource.Token);

if(register != null && register.Provider != null)
{
    var gdConfig = register.Provider.GetRequiredService<IGDConfigurationsFactory>();
    if (gdConfig.GDConfigurationsLoaded() && gdConfig.CreateConfigurations().GDConf.ConfigsChanged)
    {
        gdConfig.ConfigurationsInstance.SaveConfigurations();
    }
}
return exitCode;