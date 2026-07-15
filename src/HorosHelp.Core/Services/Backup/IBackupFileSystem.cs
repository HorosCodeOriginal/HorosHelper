namespace HorosHelp.Core.Services.Backup;

public interface IBackupFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> EnumerateFiles(string directory, string pattern, SearchOption searchOption);
    void CreateDirectory(string path);
    void CopyFile(string source, string destination, bool overwrite);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    string ReadAllText(string path);
    long GetFileLength(string path);
    DateTime GetLastWriteTimeUtc(string path);
    Stream OpenRead(string path);
    Stream OpenWrite(string path);
    void DeleteFile(string path);
}

public sealed class PhysicalBackupFileSystem : IBackupFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public IEnumerable<string> EnumerateFiles(string directory, string pattern, SearchOption searchOption) =>
        Directory.EnumerateFiles(directory, pattern, searchOption);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void CopyFile(string source, string destination, bool overwrite) =>
        File.Copy(source, destination, overwrite);

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) =>
        File.WriteAllTextAsync(path, content, cancellationToken);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public long GetFileLength(string path) => new FileInfo(path).Length;

    public DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream OpenWrite(string path) => File.Create(path);

    public void DeleteFile(string path) => File.Delete(path);
}
