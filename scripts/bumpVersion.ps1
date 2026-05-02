# Writes VersionPrefix and PackageReleaseNotes from the parsed RELEASE_NOTES.md
# entry into the given MSBuild project file.
function UpdateVersionAndReleaseNotes {
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$ReleaseNotesResult,

        [Parameter(Mandatory=$true)]
        [string]$XmlFilePath
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load($XmlFilePath)

    $versionPrefix = $xml.SelectSingleNode("//VersionPrefix")
    if ($null -eq $versionPrefix) {
        throw "Missing <VersionPrefix> element in $XmlFilePath"
    }
    $versionPrefix.InnerText = $ReleaseNotesResult.Version

    $releaseNotes = $xml.SelectSingleNode("//PackageReleaseNotes")
    if ($null -eq $releaseNotes) {
        throw "Missing <PackageReleaseNotes> element in $XmlFilePath"
    }
    $releaseNotes.InnerText = $ReleaseNotesResult.ReleaseNotes

    $xml.Save($XmlFilePath)
}
