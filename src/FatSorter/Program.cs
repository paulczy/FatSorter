namespace FatSorter;

public static class Program
{
    public static int Main(string[] args)
    {
        var parseResult = CliParser.Parse(args);
        if (parseResult.ShowHelp)
        {
            Console.WriteLine(CliParser.HelpText);
            return 0;
        }

        if (parseResult.ErrorMessage is not null)
        {
            Console.Error.WriteLine(parseResult.ErrorMessage);
            Console.Error.WriteLine();
            Console.Error.WriteLine(CliParser.HelpHint);
            return 1;
        }

        var options = parseResult.Options!;
        var targetDirectory = Path.GetFullPath(options.DirectoryPath);

        if (!Directory.Exists(targetDirectory))
        {
            Console.Error.WriteLine($"Error: Directory does not exist: {targetDirectory}");
            return 1;
        }

        using var logger = new OperationLogger(options.Verbose, options.LogFilePath);

        Console.WriteLine($"FatSorter - Sorting directory: {targetDirectory}");
        if (options.Verbose)
        {
            Console.WriteLine("Verbose mode enabled");
        }

        if (options.LogFilePath is not null)
        {
            Console.WriteLine($"Logging to: {Path.GetFullPath(options.LogFilePath)}");
        }

        if (!options.AssumeYes)
        {
            Console.WriteLine("WARNING: This tool will move files and directories around on the target volume.");
            Console.WriteLine("Press Ctrl+C to abort, or Enter to continue...");
            Console.ReadLine();
        }

        try
        {
            var sorter = new FatDirectorySorter(new PhysicalFileSystem(), logger);
            var summary = sorter.SortDirectoryRecursive(targetDirectory);

            Console.WriteLine();
            Console.WriteLine("Operation completed:");
            Console.WriteLine($"  Directories processed: {summary.DirectoriesProcessed}");
            Console.WriteLine($"  Directories sorted: {summary.DirectoriesSorted}");
            Console.WriteLine($"  Directories already sorted: {summary.DirectoriesProcessed - summary.DirectoriesSorted}");

            if (summary.DirectoriesSorted > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Directory entries were reordered on disk.");
            }

            return 0;
        }
        catch (Exception exception)
        {
            logger.Error($"Unexpected error: {exception.Message}");
            return 1;
        }
    }
}
