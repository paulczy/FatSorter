namespace FatSorter;

public sealed record FileSystemEntryInfo(
    string Name,
    string FullPath,
    bool IsDirectory);
