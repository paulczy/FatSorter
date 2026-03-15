namespace FatSorter;

public sealed record CliOptions(
    string DirectoryPath,
    bool Verbose,
    string? LogFilePath,
    bool AssumeYes);

public sealed record CliParseResult(
    CliOptions? Options,
    bool ShowHelp,
    string? ErrorMessage);
