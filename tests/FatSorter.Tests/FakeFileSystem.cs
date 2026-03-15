namespace FatSorter.Tests;

internal sealed class FakeFileSystem : IFileSystem
{
    private readonly DirectoryNode _root = new(string.Empty);
    private readonly List<(string Source, string Destination)> _moves = [];

    public IReadOnlyList<(string Source, string Destination)> Moves => _moves;

    public void AddDirectory(string path)
    {
        var segments = GetSegments(path);
        var current = _root;

        foreach (var segment in segments)
        {
            current = GetOrAddDirectory(current, segment);
        }
    }

    public void AddFile(string path)
    {
        var (parentPath, name) = SplitParent(path);
        var parent = GetDirectory(parentPath);

        if (parent.Children.Any(child => NameComparer.Equals(child.Name, name)))
        {
            throw new InvalidOperationException($"Path already exists: {path}");
        }

        parent.Children.Add(new FileNode(name));
    }

    public IReadOnlyList<FileSystemEntryInfo> GetEntries(string directoryPath)
    {
        var directory = GetDirectory(directoryPath);
        return directory.Children
            .Select(child => new FileSystemEntryInfo(child.Name, Combine(directoryPath, child.Name), child is DirectoryNode))
            .ToArray();
    }

    public bool EntryExists(string path) => TryGetNode(path) is not null;

    public void CreateDirectory(string path)
    {
        var (parentPath, name) = SplitParent(path);
        var parent = GetDirectory(parentPath);

        if (parent.Children.Any(child => NameComparer.Equals(child.Name, name)))
        {
            throw new InvalidOperationException($"Path already exists: {path}");
        }

        parent.Children.Add(new DirectoryNode(name));
    }

    public void MoveEntry(string sourcePath, string destinationPath)
    {
        var (sourceParentPath, sourceName) = SplitParent(sourcePath);
        var sourceParent = GetDirectory(sourceParentPath);
        var sourceIndex = sourceParent.Children.FindIndex(child => NameComparer.Equals(child.Name, sourceName));
        if (sourceIndex < 0)
        {
            throw new InvalidOperationException($"Source path does not exist: {sourcePath}");
        }

        var node = sourceParent.Children[sourceIndex];
        sourceParent.Children.RemoveAt(sourceIndex);

        var (destinationParentPath, destinationName) = SplitParent(destinationPath);
        var destinationParent = GetDirectory(destinationParentPath);
        if (destinationParent.Children.Any(child => NameComparer.Equals(child.Name, destinationName)))
        {
            throw new InvalidOperationException($"Destination path already exists: {destinationPath}");
        }

        node.Name = destinationName;
        destinationParent.Children.Add(node);
        _moves.Add((Normalize(sourcePath), Normalize(destinationPath)));
    }

    public void DeleteDirectory(string path)
    {
        var directory = GetDirectory(path);
        if (directory.Children.Count > 0)
        {
            throw new InvalidOperationException($"Directory is not empty: {path}");
        }

        var (parentPath, name) = SplitParent(path);
        var parent = GetDirectory(parentPath);
        parent.Children.RemoveAll(child => NameComparer.Equals(child.Name, name));
    }

    private Node? TryGetNode(string path)
    {
        var segments = GetSegments(path);
        Node current = _root;

        foreach (var segment in segments)
        {
            if (current is not DirectoryNode directory)
            {
                return null;
            }

            var next = directory.Children.FirstOrDefault(child => NameComparer.Equals(child.Name, segment));
            if (next is null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private DirectoryNode GetDirectory(string path)
    {
        var node = TryGetNode(path);
        return node as DirectoryNode
            ?? throw new InvalidOperationException($"Directory does not exist: {path}");
    }

    private static DirectoryNode GetOrAddDirectory(DirectoryNode parent, string name)
    {
        var existing = parent.Children
            .OfType<DirectoryNode>()
            .FirstOrDefault(child => NameComparer.Equals(child.Name, name));

        if (existing is not null)
        {
            return existing;
        }

        var directory = new DirectoryNode(name);
        parent.Children.Add(directory);
        return directory;
    }

    private static (string ParentPath, string Name) SplitParent(string path)
    {
        var normalized = Normalize(path);
        var separatorIndex = normalized.LastIndexOf('/');
        return separatorIndex switch
        {
            < 0 => ("/", normalized),
            0 => ("/", normalized[(separatorIndex + 1)..]),
            _ => (normalized[..separatorIndex], normalized[(separatorIndex + 1)..])
        };
    }

    private static string[] GetSegments(string path)
    {
        var normalized = Normalize(path);
        return normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string Combine(string parentPath, string name)
    {
        var normalizedParent = Normalize(parentPath);
        return normalizedParent == "/"
            ? $"/{name}"
            : $"{normalizedParent}/{name}";
    }

    private static string Normalize(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return "/";
        }

        normalized = normalized.TrimEnd('/');
        return normalized.Length == 0 ? "/" : normalized;
    }

    private static StringComparer NameComparer => StringComparer.OrdinalIgnoreCase;

    private abstract class Node(string name)
    {
        public string Name { get; set; } = name;
    }

    private sealed class FileNode(string name) : Node(name);

    private sealed class DirectoryNode(string name) : Node(name)
    {
        public List<Node> Children { get; } = [];
    }
}
