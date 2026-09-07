# Creates a clean zip of the repo for copying into the AVD.
# Uses `git archive`, so it contains exactly the committed files (correct folder
# structure, no bin/obj/.git, nothing gitignored). Commit first.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out  = Join-Path $root 'ssam-mobile-api.zip'
if (Test-Path $out) { Remove-Item $out }

Push-Location $root
try {
    $dirty = git status --porcelain
    if ($dirty) { Write-Warning "Uncommitted changes will NOT be in the zip:`n$dirty" }
    git archive --format=zip --prefix=SSAM-Mobile-API/ -o $out HEAD
}
finally { Pop-Location }

Write-Host "Wrote $out ($([math]::Round((Get-Item $out).Length / 1KB)) KB)"
