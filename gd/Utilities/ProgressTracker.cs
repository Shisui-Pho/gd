using Spectre.Console;

namespace GD.Utilities;

internal delegate void delReportProgress(long processed, long total);
internal enum ProgressType
{
    Downloading,
    Installing,
    Extracting,
    Processing,
    Configuring
}
internal class ProgressTracker
{
    private readonly ProgressContext _ctx;
    private ProgressTask progressTask;
    private string name;
    private string strMax = string.Empty;
    private ByteUnit bUnit = ByteUnit.Byte;
    private ProgressType progressType = ProgressType.Processing;
    public ProgressTracker(ProgressContext ctx)
    {
        _ctx = ctx;
    }
    public void StartNewTaskToTrack(ProgressType type)
    {
        name = type.ToString();
        progressTask = _ctx.AddTask($"{name}...");
        progressTask.IsIndeterminate(true);
        strMax = string.Empty;
        bUnit = ByteUnit.Byte;
        progressType = type;
    }
    public void UpdateProgress(long processed, long total)
    {
        //-By default it will be indeterminant
        if (progressTask.IsIndeterminate && total > 0)
        {
            progressTask.IsIndeterminate(false);
            progressTask.MaxValue = total;
            bUnit = ByteSizeFormatter.DetermineByteSizeUnit(total);
            strMax = ByteSizeFormatter.FormatBytesToReadable(total, bUnit);
        }

        progressTask.Value = processed;
        if(total > 0)
        {
            progressTask.Description = $"[cyan]{progressType.ToString()} ({ByteSizeFormatter.FormatBytesToReadable(processed, bUnit)} / {strMax})[/]";
        }
        else
        {
            progressTask.Description = $"[cyan]Processing ({ByteSizeFormatter.FormatBytesToReadable(processed)})[/]";
        }
    }
}