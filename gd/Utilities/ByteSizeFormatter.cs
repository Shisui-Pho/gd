namespace GD.Utilities;

internal static class ByteSizeFormatter
{
    private const int BYTES_IN_KILOBYTE = 1024;

    private readonly static string[] suffixes = ["B", "KiB", "MiB", "GiB"];

    public static ByteUnit DetermineByteSizeUnit(long totalBytes)
    {
        if (totalBytes < BYTES_IN_KILOBYTE) 
            return ByteUnit.Byte;

        long mega = BYTES_IN_KILOBYTE * BYTES_IN_KILOBYTE;

        if (totalBytes < mega)
            return ByteUnit.Kilobyte;

        long giga = mega * BYTES_IN_KILOBYTE;

        if (totalBytes < giga)
            return ByteUnit.Megabyte;

        return ByteUnit.Gigabyte;
    }
    public static long ReduceBytesToByteSizeUnit(long totalBytes, ByteUnit measurement)
    {
        return measurement switch
        {
            ByteUnit.Byte => totalBytes,
            ByteUnit.Kilobyte => totalBytes / BYTES_IN_KILOBYTE,
            ByteUnit.Megabyte => totalBytes / (BYTES_IN_KILOBYTE * BYTES_IN_KILOBYTE),
            ByteUnit.Gigabyte => totalBytes / (BYTES_IN_KILOBYTE * BYTES_IN_KILOBYTE * BYTES_IN_KILOBYTE),
            _ => throw new NotImplementedException(),
        };
    }
    public static string FormatBytesToReadable(long totalBytes)
    {
        var measurement = DetermineByteSizeUnit(totalBytes);
        var reduced = ReduceBytesToByteSizeUnit(totalBytes, measurement);
        return measurement switch
        {
            ByteUnit.Byte => $"{reduced} {suffixes[0]}",
            ByteUnit.Kilobyte => $"{reduced} {suffixes[1]}",
            ByteUnit.Megabyte => $"{reduced} {suffixes[2]}",
            ByteUnit.Gigabyte => $"{reduced} {suffixes[3]}",
            _ => throw new NotImplementedException()
        };
    }
    public static string FormatBytesToReadable(long totalBytes, ByteUnit measurement)
    {
        var reduced = ReduceBytesToByteSizeUnit(totalBytes, measurement);
        return measurement switch
        {
            ByteUnit.Byte => $"{reduced} {suffixes[0]}",
            ByteUnit.Kilobyte => $"{reduced} {suffixes[1]}",
            ByteUnit.Megabyte => $"{reduced} {suffixes[2]}",
            ByteUnit.Gigabyte => $"{reduced} {suffixes[3]}",
            _ => throw new NotImplementedException()
        };
    }
}