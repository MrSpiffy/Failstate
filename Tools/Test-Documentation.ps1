[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$required = @(
    'README.md',
    'Documentation/ARCHITECTURE.md',
    'Documentation/DEVELOPMENT.md',
    'Documentation/STATUS.md',
    'Documentation/PLAYTESTING.md',
    'Documentation/ATTRIBUTION.md',
    'Documentation/CAPTURES.md',
    'Documentation/SUBMISSION_REVIEW.md'
)
$failures = [System.Collections.Generic.List[string]]::new()
$checkedLinks = 0

foreach ($relative in $required) {
    if (!(Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf)) {
        $failures.Add("Required document missing: $relative")
    }
}

$documents = @()
$readme = Join-Path $root 'README.md'
if (Test-Path -LiteralPath $readme) {
    $documents += Get-Item -LiteralPath $readme
}
foreach ($folder in @('Documentation', '.github')) {
    $path = Join-Path $root $folder
    if (Test-Path -LiteralPath $path) {
        $documents += Get-ChildItem -LiteralPath $path -Filter '*.md' -Recurse -File
    }
}

# Check file targets only; URL availability and Markdown anchors need separate review.
$linkPattern = '\]\((?<target><[^>]+>|[^\s)]+)(?:\s+"[^"]*")?\)'
foreach ($document in $documents) {
    $content = Get-Content -LiteralPath $document.FullName -Raw
    $content = [regex]::Replace($content, '(?ms)^ {0,3}(?<fence>`{3,}|~{3,})[^\r\n]*\r?\n.*?^ {0,3}\k<fence>[ \t]*(?:\r?\n|$)', '')
    $content = [regex]::Replace($content, '(?s)<!--.*?-->', '')
    foreach ($match in [regex]::Matches($content, $linkPattern)) {
        $target = $match.Groups['target'].Value.Trim('<', '>')
        if ($target -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|#)') {
            continue
        }
        $target = [Uri]::UnescapeDataString(($target -split '[?#]', 2)[0])
        if ([string]::IsNullOrWhiteSpace($target)) {
            continue
        }
        if ([IO.Path]::IsPathRooted($target)) {
            $failures.Add("Machine-specific link in $($document.Name): $target")
            continue
        }
        $resolved = Join-Path $document.DirectoryName $target
        $checkedLinks++
        if (!(Test-Path -LiteralPath $resolved)) {
            $failures.Add("Broken link in $($document.Name): $target")
        }
    }
}

if ($failures.Count -gt 0) {
    throw ($failures -join [Environment]::NewLine)
}
Write-Output "Documentation checks passed: $($documents.Count) Markdown files, $checkedLinks local links."
