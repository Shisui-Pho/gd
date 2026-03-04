using Spectre.Console.Cli;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using GD.Commands;
using GD.Services;
using GD.Services.Abstractions;
using GD.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;

//DPI
var services = new ServiceCollection();
services.AddSingleton<IGDListService,GDListService>();

var register = new TypeRegistrar(services);

var app = new CommandApp(register);
app.Configure(config =>
{
    config.AddCommand<ListCommand>("list");
});

return app.Run(args);