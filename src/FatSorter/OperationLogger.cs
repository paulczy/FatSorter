namespace FatSorter;

public sealed class OperationLogger : IDisposable
{
    private readonly bool _verbose;
    private readonly StreamWriter? _logWriter;

    public OperationLogger(bool verbose, string? logFilePath)
    {
        _verbose = verbose;

        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            return;
        }

        try
        {
            var fullPath = Path.GetFullPath(logFilePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _logWriter = new StreamWriter(fullPath, append: true)
            {
                AutoFlush = true
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Warning: Could not create log file '{logFilePath}': {exception.Message}");
        }
    }

    public void Info(string message) => Write("INFO", message, writeToConsole: _verbose, standardError: false);

    public void Warning(string message) => Write("WARN", message, writeToConsole: true, standardError: true);

    public void Error(string message) => Write("ERROR", message, writeToConsole: true, standardError: true);

    public void Dispose() => _logWriter?.Dispose();

    private void Write(string level, string message, bool writeToConsole, bool standardError)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

        if (writeToConsole)
        {
            if (standardError)
            {
                Console.Error.WriteLine(line);
            }
            else
            {
                Console.WriteLine(line);
            }
        }

        _logWriter?.WriteLine(line);
    }
}
