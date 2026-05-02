# Parses RELEASE_NOTES.md and returns the newest entry as a PSCustomObject
# with Version, Date and ReleaseNotes (bullet list body) fields.
function Get-ReleaseNotes {
    param (
        [Parameter(Mandatory=$true)]
        [string]$MarkdownFile
    )

    $content = Get-Content $MarkdownFile -Raw

    # Match the first #### heading block: "#### <version> <date> ####" followed
    # by the body, stopping at the next #### or end-of-file.
    if ($content -notmatch '(?s)####\s+(\S+)\s+(.+?)\s+####\s*\r?\n(.+?)(?=\r?\n####|\z)') {
        throw "Could not parse latest release notes entry from $MarkdownFile"
    }

    $version = $Matches[1].Trim()
    $date = $Matches[2].Trim()
    $body = $Matches[3].Trim()

    return [PSCustomObject]@{
        Version      = $version
        Date         = $date
        ReleaseNotes = $body
    }
}
