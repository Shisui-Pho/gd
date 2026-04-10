using GD.Models;
using Spectre.Console;

namespace GD.Utilities;

internal static class ConsoleMarkupUtility
{
    public static void PrintError(string message)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {message}");
    }

    public static void PrintWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]Warning:[/] {message}");
    }
    public static void PrintLine(string message)
    {
        AnsiConsole.WriteLine(message);
    }
    public static void PrintInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]Info:[/] {message}");
    }

    public static void PrintSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }
    public static void HandleException(Exception ex)
    {
        Console.WriteLine(ex.ToString());
        PrintError(ex.Message);
        //And probably log it to a file in the future
    }
    public static void HandleServiceResult(ServiceResult results)
    {
        if (results.IsSuccess) return;

        //Messages go first
        PrintError(results.Message);

        if (results.Exception != null)
            HandleException(results.Exception);
    }
    public static void Spacer()
    {
        AnsiConsole.WriteLine();
    }
    public static void PrintGodotVersion(GodotVersion tool, string title = "Godot Version")
    {
        if(tool == null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]No Godot versions detected. Install one to get started.[/]");
            AnsiConsole.WriteLine();
            return;
        }
        var table = new Table()
            .Border(TableBorder.None)
            .AddColumn("[cyan]Version[/]")
            .AddColumn("[cyan]Status[/]")
            .AddColumn("[cyan]Mono/Standard[/]");

        string version =
            tool.IsActive ? $"[bold green]{tool.Version}[/]" :
            !tool.IsStatableVersion ? $"[yellow]{tool.Version}[/]" :
            tool.IsBroken ? $"[red]{tool.Version}[/]" :
                            $"[green]{tool.Version}[/]";

        string status =
            tool.IsActive ? "[bold green]Active[/]" :
            !tool.IsStatableVersion ? "[yellow]Beta[/]" :
            tool.IsBroken ? "[red]Broken[/]" :
            "[grey]Installed[/]";

        string dotnetSupport = tool.SupportsDotNet ? "Mono" : "Standard";

        table.AddRow(version, status, $"[gray]{dotnetSupport}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]{title}[/]");
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
    public static void PrintGodotVersionsTable(IEnumerable<GodotVersion> tools)
    {
        if (tools == null || !tools.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]No Godot versions detected. Install one to get started.[/]");
            AnsiConsole.WriteLine();
            return;
        }
        var table = new Table()
            .Border(TableBorder.None)
            .AddColumn("[cyan]Version[/]")
            .AddColumn("[cyan]Status[/]")
            .AddColumn("[cyan]Mono/Standard[/]");

        foreach (var tool in tools.OrderByDescending(t => t.Version))
        {
            string version =
                tool.IsActive ? $"[bold green]{tool.Version}[/]" :
                !tool.IsStatableVersion ? $"[yellow]{tool.Version}[/]" :
                tool.IsBroken ? $"[red]{tool.Version}[/]" :
                                $"[green]{tool.Version}[/]";

            string status =
                tool.IsActive ? "[bold green]Active[/]" :
                !tool.IsStatableVersion ? "[yellow]Beta[/]" :
                tool.IsBroken ? "[red]Broken[/]" :
                "[grey]Installed[/]";

            string dotnetSupport = tool.SupportsDotNet ? "Mono" : "Standard";

            table.AddRow(version, status, $"[gray]{dotnetSupport}[/]");
        }
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}