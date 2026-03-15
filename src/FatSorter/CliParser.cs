namespace FatSorter;

public static class CliParser
{
    public const string HelpHint = "Run 'FatSorter --help' for usage.";

    public static string HelpText =>
        """
        FatSorter

        Usage:
          FatSorter <directory> [--verbose] [--log-file <path>] [--yes]

        Options:
          -v, --verbose        Enable verbose output
          -l, --log-file       Write all operations to a log file
          -y, --yes            Skip the interactive confirmation prompt
          -h, --help           Show this help text

        Examples:
          FatSorter E:\
          FatSorter /Volumes/SDCARD --verbose
          FatSorter /Volumes/SDCARD --log-file fat-sorter.log --yes
        """;

    public static CliParseResult Parse(IReadOnlyList<string> args)
    {
        if (args.Count is 0)
        {
            return new CliParseResult(null, false, "Error: Missing required directory argument.");
        }

        string? directoryPath = null;
        var verbose = false;
        var assumeYes = false;
        string? logFilePath = null;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];

            switch (argument)
            {
                case "-h":
                case "--help":
                    return new CliParseResult(null, true, null);
                case "-v":
                case "--verbose":
                    verbose = true;
                    break;
                case "-y":
                case "--yes":
                    assumeYes = true;
                    break;
                case "-l":
                case "--log-file":
                    if (index + 1 >= args.Count)
                    {
                        return new CliParseResult(null, false, "Error: Missing value for --log-file.");
                    }

                    logFilePath = args[++index];
                    break;
                default:
                    if (argument.StartsWith("-", StringComparison.Ordinal))
                    {
                        return new CliParseResult(null, false, $"Error: Unknown option '{argument}'.");
                    }

                    if (directoryPath is not null)
                    {
                        return new CliParseResult(null, false, "Error: Only one directory argument is supported.");
                    }

                    directoryPath = argument;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return new CliParseResult(null, false, "Error: Missing required directory argument.");
        }

        return new CliParseResult(
            new CliOptions(directoryPath, verbose, logFilePath, assumeYes),
            false,
            null);
    }
}
