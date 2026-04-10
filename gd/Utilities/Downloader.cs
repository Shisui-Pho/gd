using GD.Models;
using System.Diagnostics;
namespace GD.Utilities;

internal static class Downloader
{
    private static readonly HttpClient httpClient = new();
    public static async Task<ServiceResult> DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken, delReportProgress progress = null)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? 0;

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);
            long totalRead = 0;
            int read;
            var stopwatch = Stopwatch.StartNew();
            var lastReportTime = stopwatch.Elapsed;
            long lastBytes = 0;

            var buffer = new byte[8192];
            Memory<byte> memory = new(buffer, 0, buffer.Length);


            while((read = await contentStream.ReadAsync(memory, cancellationToken)) > 0) 
            {
                await fileStream.WriteAsync(memory[..read], cancellationToken);
                totalRead += read;

                var now = stopwatch.Elapsed;

                if ((now - lastReportTime).TotalMilliseconds > 200)
                {
                    var bytesDelta = totalRead - lastBytes;
                    var timeDelta = (now - lastReportTime).TotalSeconds;

                    double speed = timeDelta > 0 ? bytesDelta / timeDelta : 0;

                    progress?.Invoke(totalRead, totalBytes);

                    lastBytes = totalRead;
                    lastReportTime = now;
                }
            }
            return ServiceResult.Success();
        }
        catch(OperationCanceledException)
        {
            return ServiceResult.Fail("Operation cancelled.");
        }
        catch(Exception ex)
        {
            ConsoleMarkupUtility.HandleException(ex);
            return ServiceResult.Fail(ex);
        }
    }
}