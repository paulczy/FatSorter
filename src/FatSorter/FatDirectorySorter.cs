namespace FatSorter;

public sealed class FatDirectorySorter(IFileSystem fileSystem, OperationLogger logger, Func<string>? temporaryDirectoryNameFactory = null)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly OperationLogger _logger = logger;
    private readonly Func<string> _temporaryDirectoryNameFactory = temporaryDirectoryNameFactory ?? DefaultTemporaryDirectoryNameFactory;

    public DirectorySortSummary SortDirectoryRecursive(string rootDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectoryPath);

        var counters = new SortCounters();
        SortRecursive(rootDirectoryPath, counters);
        return new DirectorySortSummary(counters.Processed, counters.Sorted);
    }

    public bool SortDirectoryEntries(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        return SortDirectoryEntries(directoryPath, GetVisibleEntries(directoryPath));
    }

    private bool SortDirectoryEntries(string directoryPath, IReadOnlyList<FileSystemEntryInfo> currentEntries)
    {
        _logger.Info($"Processing directory: {directoryPath}");

        if (currentEntries.Count is 0)
        {
            _logger.Info($"Directory is empty or has only hidden entries: {directoryPath}");
            return false;
        }

        var sortedEntries = currentEntries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (currentEntries.Select(entry => entry.Name).SequenceEqual(sortedEntries.Select(entry => entry.Name), StringComparer.Ordinal))
        {
            _logger.Info($"Directory already sorted: {directoryPath}");
            return false;
        }

        var temporaryDirectoryPath = Path.Combine(directoryPath, _temporaryDirectoryNameFactory());
        var movedEntryNames = new List<string>(sortedEntries.Length);

        try
        {
            _fileSystem.CreateDirectory(temporaryDirectoryPath);
            _logger.Info($"Created temporary directory: {temporaryDirectoryPath}");

            foreach (var entry in sortedEntries)
            {
                if (!_fileSystem.EntryExists(entry.FullPath))
                {
                    _logger.Warning($"Entry disappeared during operation: {entry.Name}");
                    continue;
                }

                var destinationPath = Path.Combine(temporaryDirectoryPath, entry.Name);
                _fileSystem.MoveEntry(entry.FullPath, destinationPath);
                movedEntryNames.Add(entry.Name);
                _logger.Info($"Moved to temp: {entry.Name}");
            }

            FlushFileSystem();

            foreach (var entryName in movedEntryNames)
            {
                var sourcePath = Path.Combine(temporaryDirectoryPath, entryName);
                var destinationPath = Path.Combine(directoryPath, entryName);
                _fileSystem.MoveEntry(sourcePath, destinationPath);
                _logger.Info($"Moved back: {entryName}");
            }

            FlushFileSystem();

            _fileSystem.DeleteDirectory(temporaryDirectoryPath);
            _logger.Info($"Removed temporary directory: {temporaryDirectoryPath}");
            return movedEntryNames.Count > 0;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to sort directory '{directoryPath}': {exception.Message}");

            foreach (var entryName in movedEntryNames)
            {
                try
                {
                    var tempPath = Path.Combine(temporaryDirectoryPath, entryName);
                    if (_fileSystem.EntryExists(tempPath))
                    {
                        var originalPath = Path.Combine(directoryPath, entryName);
                        _fileSystem.MoveEntry(tempPath, originalPath);
                        _logger.Info($"Recovered: {entryName}");
                    }
                }
                catch (Exception rollbackException)
                {
                    _logger.Warning($"Failed to recover '{entryName}': {rollbackException.Message}");
                }
            }

            try
            {
                if (_fileSystem.EntryExists(temporaryDirectoryPath))
                {
                    _fileSystem.DeleteDirectory(temporaryDirectoryPath);
                }
            }
            catch
            {
                _logger.Warning($"Temporary directory may still exist: {temporaryDirectoryPath}");
            }

            return false;
        }
    }

    private static string DefaultTemporaryDirectoryNameFactory() =>
        $".fat_sort_temp_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

    private IReadOnlyList<FileSystemEntryInfo> GetVisibleEntries(string directoryPath)
    {
        try
        {
            return _fileSystem
                .GetEntries(directoryPath)
                .Where(entry => !entry.Name.StartsWith(".", StringComparison.Ordinal))
                .ToArray();
        }
        catch (Exception exception)
        {
            _logger.Error($"Error reading directory '{directoryPath}': {exception.Message}");
            return [];
        }
    }

    private void FlushFileSystem()
    {
        try
        {
            FileSystemSync.Flush();
        }
        catch (Exception exception)
        {
            _logger.Warning($"File system sync failed: {exception.Message}");
        }
    }

    private void SortRecursive(string directoryPath, SortCounters counters)
    {
        counters.Processed++;

        var entries = GetVisibleEntries(directoryPath);
        var subdirectories = entries.Where(entry => entry.IsDirectory).ToArray();

        if (SortDirectoryEntries(directoryPath, entries))
        {
            counters.Sorted++;
        }

        foreach (var subdirectory in subdirectories)
        {
            SortRecursive(subdirectory.FullPath, counters);
        }
    }

    private sealed class SortCounters
    {
        public int Processed { get; set; }

        public int Sorted { get; set; }
    }
}
