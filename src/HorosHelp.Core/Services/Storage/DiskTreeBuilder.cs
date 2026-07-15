namespace HorosHelp.Core.Services.Storage;

using HorosHelp.Core.Models.Storage;

public interface IFileSystemScanner
{
    bool DirectoryExists(string path);

    IEnumerable<string> EnumerateDirectories(string path);

    IEnumerable<string> EnumerateFiles(string path);

    long GetFileSize(string path);
}

public sealed class PhysicalFileSystemScanner : IFileSystemScanner
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> EnumerateDirectories(string path) =>
        Directory.EnumerateDirectories(path);

    public IEnumerable<string> EnumerateFiles(string path) =>
        Directory.EnumerateFiles(path);

    public long GetFileSize(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }
}

public static class DiskTreeBuilder
{
    public static FolderTreeNode BuildTree(
        string rootPath,
        IFileSystemScanner scanner,
        int maxDepth = 2,
        int maxChildren = 8,
        CancellationToken cancellationToken = default)
    {
        return BuildNode(rootPath, scanner, maxDepth, maxChildren, cancellationToken);
    }

    private static FolderTreeNode BuildNode(
        string path,
        IFileSystemScanner scanner,
        int depthRemaining,
        int maxChildren,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(name))
            name = path;

        long directFileSize = 0;
        var childNodes = new List<FolderTreeNode>();

        if (scanner.DirectoryExists(path))
        {
            foreach (var file in scanner.EnumerateFiles(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                directFileSize += scanner.GetFileSize(file);
            }

            if (depthRemaining > 0)
            {
                var directories = scanner.EnumerateDirectories(path)
                    .Select(dir => BuildNode(dir, scanner, depthRemaining - 1, maxChildren, cancellationToken))
                    .OrderByDescending(n => n.SizeBytes)
                    .Take(maxChildren)
                    .ToList();

                childNodes.AddRange(directories);
            }
        }

        var childrenSize = childNodes.Sum(c => c.SizeBytes);
        return new FolderTreeNode
        {
            Name = name,
            Path = path,
            SizeBytes = directFileSize + childrenSize,
            Children = childNodes,
        };
    }
}
