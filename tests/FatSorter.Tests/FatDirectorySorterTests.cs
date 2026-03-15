namespace FatSorter.Tests;

public sealed class FatDirectorySorterTests
{
    [Test]
    public async Task SortDirectoryEntries_WhenVisibleEntriesAreUnsorted_MovesEntriesThroughTemporaryDirectoryInSortedOrder()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddDirectory("/root");
        fileSystem.AddFile("/root/z-last.gcode");
        fileSystem.AddFile("/root/.hidden");
        fileSystem.AddFile("/root/A-first.gcode");
        fileSystem.AddFile("/root/m-middle.gcode");

        using var logger = new OperationLogger(verbose: false, logFilePath: null);
        var sorter = new FatDirectorySorter(fileSystem, logger, () => ".fat_sort_temp_test");

        var changed = sorter.SortDirectoryEntries("/root");

        await Assert.That(changed).IsTrue();
        await Assert.That(fileSystem.GetEntries("/root").Select(entry => entry.FullPath).ToArray())
            .IsEquivalentTo(
            [
                "/root/.hidden",
                "/root/A-first.gcode",
                "/root/m-middle.gcode",
                "/root/z-last.gcode"
            ]);

        await Assert.That(fileSystem.Moves.ToArray()).IsEquivalentTo(
        [
            ("/root/A-first.gcode", "/root/.fat_sort_temp_test/A-first.gcode"),
            ("/root/m-middle.gcode", "/root/.fat_sort_temp_test/m-middle.gcode"),
            ("/root/z-last.gcode", "/root/.fat_sort_temp_test/z-last.gcode"),
            ("/root/.fat_sort_temp_test/A-first.gcode", "/root/A-first.gcode"),
            ("/root/.fat_sort_temp_test/m-middle.gcode", "/root/m-middle.gcode"),
            ("/root/.fat_sort_temp_test/z-last.gcode", "/root/z-last.gcode")
        ]);
    }

    [Test]
    public async Task SortDirectoryEntries_WhenAlreadySorted_ReturnsFalse()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddDirectory("/root");
        fileSystem.AddFile("/root/a-first.gcode");
        fileSystem.AddFile("/root/b-second.gcode");
        fileSystem.AddFile("/root/c-third.gcode");

        using var logger = new OperationLogger(verbose: false, logFilePath: null);
        var sorter = new FatDirectorySorter(fileSystem, logger, () => ".fat_sort_temp_test");

        var changed = sorter.SortDirectoryEntries("/root");

        await Assert.That(changed).IsFalse();
        await Assert.That(fileSystem.Moves).IsEmpty();
    }

    [Test]
    public async Task SortDirectoryEntries_WhenDirectoryIsEmpty_ReturnsFalse()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddDirectory("/root");

        using var logger = new OperationLogger(verbose: false, logFilePath: null);
        var sorter = new FatDirectorySorter(fileSystem, logger, () => ".fat_sort_temp_test");

        var changed = sorter.SortDirectoryEntries("/root");

        await Assert.That(changed).IsFalse();
    }

    [Test]
    public async Task SortDirectoryEntries_WhenOnlyHiddenEntries_ReturnsFalse()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddDirectory("/root");
        fileSystem.AddFile("/root/.hidden1");
        fileSystem.AddFile("/root/.hidden2");

        using var logger = new OperationLogger(verbose: false, logFilePath: null);
        var sorter = new FatDirectorySorter(fileSystem, logger, () => ".fat_sort_temp_test");

        var changed = sorter.SortDirectoryEntries("/root");

        await Assert.That(changed).IsFalse();
    }

    [Test]
    public async Task SortDirectoryRecursive_WhenChildDirectoryNeedsSorting_ProcessesChildDirectories()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddDirectory("/root");
        fileSystem.AddFile("/root/A-first.gcode");
        fileSystem.AddDirectory("/root/folder");
        fileSystem.AddFile("/root/folder/z-last.gcode");
        fileSystem.AddFile("/root/folder/B-middle.gcode");
        fileSystem.AddFile("/root/folder/a-first.gcode");

        using var logger = new OperationLogger(verbose: false, logFilePath: null);
        var sorter = new FatDirectorySorter(fileSystem, logger, () => ".fat_sort_temp_test");

        var summary = sorter.SortDirectoryRecursive("/root");

        await Assert.That(summary).IsEqualTo(new DirectorySortSummary(2, 1));
        await Assert.That(fileSystem.GetEntries("/root/folder").Select(entry => entry.FullPath).ToArray())
            .IsEquivalentTo(
            [
                "/root/folder/a-first.gcode",
                "/root/folder/B-middle.gcode",
                "/root/folder/z-last.gcode"
            ]);
    }
}
