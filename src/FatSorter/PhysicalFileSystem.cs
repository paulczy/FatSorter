namespace FatSorter;

public sealed class PhysicalFileSystem : IFileSystem
{
    public IReadOnlyList<FileSystemEntryInfo> GetEntries(string directoryPath)
    {
        return Directory
            .EnumerateFileSystemEntries(directoryPath)
            .Select(path => new FileSystemEntryInfo(
                Path.GetFileName(path),
                path,
                Directory.Exists(path)))
            .ToArray();
    }

    public bool EntryExists(string path) => Directory.Exists(path) || File.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void MoveEntry(string sourcePath, string destinationPath)
    {
        if (Directory.Exists(sourcePath))
        {
            Directory.Move(sourcePath, destinationPath);
            return;
        }

        File.Move(sourcePath, destinationPath);
    }

    public void DeleteDirectory(string path) => Directory.Delete(path, false);
}
