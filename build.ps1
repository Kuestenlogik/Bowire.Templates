# Reads the newest entry from RELEASE_NOTES.md and injects its version +
# body into Kuestenlogik.Bowire.Templates.csproj so `dotnet pack` produces a package
# tagged with the right VersionPrefix and PackageReleaseNotes. Run before
# `dotnet pack` in both local dev and CI.

. "$PSScriptRoot/scripts/getReleaseNotes.ps1"
. "$PSScriptRoot/scripts/bumpVersion.ps1"

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$releaseNotes = Get-ReleaseNotes -MarkdownFile (Join-Path $PSScriptRoot "RELEASE_NOTES.md")

UpdateVersionAndReleaseNotes `
    -ReleaseNotesResult $releaseNotes `
    -XmlFilePath (Join-Path $PSScriptRoot "Kuestenlogik.Bowire.Templates.csproj")

Write-Output "Injected release notes for version $($releaseNotes.Version) ($($releaseNotes.Date))"
