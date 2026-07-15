namespace HorosHelp.Core.Models.Storage;

public sealed class FolderTreeNode
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public IReadOnlyList<FolderTreeNode> Children { get; init; } = [];
    public bool IsExpanded { get; init; }
}

public sealed class DiskAnalysisProgress
{
    public double Percent { get; init; }
    public string CurrentPath { get; init; } = "";
    public int ScannedFolders { get; init; }
}

public sealed class DiskAnalysisResult
{
    public FolderTreeNode? Root { get; init; }
    public bool WasCancelled { get; init; }
    public int TotalFoldersScanned { get; init; }
}
