namespace HorosHelp.Core.Services.ProblemScan;

public static class DirectoryCleanupHelper
{
    public static (int Deleted, int Errors) ClearDirectoryContents(string path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            return (0, 0);

        var deleted = 0;
        var errors = 0;

        foreach (var entry in Directory.EnumerateFileSystemEntries(path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (Directory.Exists(entry))
                    Directory.Delete(entry, recursive: true);
                else
                    File.Delete(entry);

                deleted++;
            }
            catch
            {
                errors++;
            }
        }

        return (deleted, errors);
    }

    public static long EstimateDirectorySize(string path, int maxDepth = 2)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return 0;

        return SumDirectorySizeRecursive(path, maxDepth);
    }

    private static long SumDirectorySizeRecursive(string path, int depthRemaining)
    {
        long total = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // skip locked files
                }
            }

            if (depthRemaining <= 0)
                return total;

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                try
                {
                    total += SumDirectorySizeRecursive(dir, depthRemaining - 1);
                }
                catch
                {
                    // skip inaccessible directories
                }
            }
        }
        catch
        {
            return total;
        }

        return total;
    }
}
