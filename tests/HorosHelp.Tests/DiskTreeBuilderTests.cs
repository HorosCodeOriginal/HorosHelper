using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Services.Storage;

namespace HorosHelp.Tests;

public class DiskTreeBuilderTests
{
    [Fact]
    public void BuildTree_AggregatesChildSizes()
    {
        var scanner = new InMemoryFileSystemScanner()
            .AddDirectory(@"C:\root")
            .AddDirectory(@"C:\root\large")
            .AddFile(@"C:\root\large\big.bin", 1000)
            .AddDirectory(@"C:\root\small")
            .AddFile(@"C:\root\small\tiny.bin", 10);

        var tree = DiskTreeBuilder.BuildTree(@"C:\root", scanner, maxDepth: 2);

        Assert.Equal(1010, tree.SizeBytes);
        Assert.Equal(2, tree.Children.Count);
        Assert.Equal("large", tree.Children[0].Name);
        Assert.Equal(1000, tree.Children[0].SizeBytes);
    }

    private sealed class InMemoryFileSystemScanner : IFileSystemScanner
    {
        private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> _files = new(StringComparer.OrdinalIgnoreCase);

        public InMemoryFileSystemScanner AddDirectory(string path)
        {
            _directories.Add(path.TrimEnd('\\'));
            return this;
        }

        public InMemoryFileSystemScanner AddFile(string path, long size)
        {
            _files[path] = size;
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(parent))
                _directories.Add(parent);
            return this;
        }

        public bool DirectoryExists(string path) => _directories.Contains(path.TrimEnd('\\'));

        public IEnumerable<string> EnumerateDirectories(string path) =>
            _directories.Where(d =>
            {
                var parent = Path.GetDirectoryName(d);
                return parent is not null
                       && parent.Equals(path.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            });

        public IEnumerable<string> EnumerateFiles(string path) =>
            _files.Keys.Where(f =>
            {
                var parent = Path.GetDirectoryName(f);
                return parent is not null
                       && parent.Equals(path.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            });

        public long GetFileSize(string path) => _files.TryGetValue(path, out var size) ? size : 0;
    }
}
