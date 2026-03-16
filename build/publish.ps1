param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$OutputRoot = 'publish'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$projectPath = Join-Path $repoRoot 'src\FatSorter\FatSorter.csproj'
$publishRoot = Join-Path $repoRoot $OutputRoot
$runtimeIdentifiers = @(
    'win-x86',
    'win-x64',
    'win-arm64',
    'linux-x64',
    'linux-arm64',
    'osx-x64',
    'osx-arm64'
)

foreach ($runtimeIdentifier in $runtimeIdentifiers) {
    $outputPath = Join-Path $publishRoot $runtimeIdentifier

    dotnet publish $projectPath `
        --configuration $Configuration `
        --runtime $runtimeIdentifier `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        --output $outputPath

    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for runtime '$runtimeIdentifier'."
    }
}

Write-Host "Published self-contained artifacts to $publishRoot"
