namespace FatSorter.Tests;

public sealed class CliParserTests
{
    [Test]
    public async Task Parse_WhenValidArgumentsProvided_ReturnsOptions()
    {
        var result = CliParser.Parse(["E:\\", "--verbose", "--log-file", "fat-sorter.log", "--yes"]);

        await Assert.That(result.ShowHelp).IsFalse();
        await Assert.That(result.ErrorMessage).IsNull();
        await Assert.That(result.Options).IsNotNull();
        await Assert.That(result.Options!.DirectoryPath).IsEqualTo("E:\\");
        await Assert.That(result.Options.Verbose).IsTrue();
        await Assert.That(result.Options.LogFilePath).IsEqualTo("fat-sorter.log");
        await Assert.That(result.Options.AssumeYes).IsTrue();
    }

    [Test]
    public async Task Parse_WhenNoArgumentsProvided_ReturnsError()
    {
        var result = CliParser.Parse([]);

        await Assert.That(result.ErrorMessage).IsEqualTo("Error: Missing required directory argument.");
    }

    [Test]
    public async Task Parse_WhenHelpFlagProvided_ReturnsShowHelp()
    {
        var result = CliParser.Parse(["--help"]);

        await Assert.That(result.ShowHelp).IsTrue();
        await Assert.That(result.Options).IsNull();
        await Assert.That(result.ErrorMessage).IsNull();
    }

    [Test]
    public async Task Parse_WhenLogFileMissingValue_ReturnsError()
    {
        var result = CliParser.Parse(["E:\\", "--log-file"]);

        await Assert.That(result.ErrorMessage).IsEqualTo("Error: Missing value for --log-file.");
    }

    [Test]
    public async Task Parse_WhenDuplicateDirectoryProvided_ReturnsError()
    {
        var result = CliParser.Parse(["E:\\", "F:\\"]);

        await Assert.That(result.ErrorMessage).IsEqualTo("Error: Only one directory argument is supported.");
    }

    [Test]
    public async Task Parse_WhenUnknownOptionProvided_ReturnsError()
    {
        var result = CliParser.Parse(["E:\\", "--wat"]);

        await Assert.That(result.ErrorMessage).IsEqualTo("Error: Unknown option '--wat'.");
    }
}
