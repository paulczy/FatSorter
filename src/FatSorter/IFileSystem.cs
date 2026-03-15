namespace FatSorter;

public interface IFileSystem
{
    IReadOnlyList<FileSystemEntryInfo> GetEntries(string directoryPath);

    bool EntryExists(string path);

    void CreateDirectory(string path);

    void MoveEntry(string sourcePath, string destinationPath);

    void DeleteDirectory(string path);
}
