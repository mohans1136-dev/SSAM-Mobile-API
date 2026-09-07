# Creates a clean zip of the repo (no bin/obj/.git/.vs) for copying into the AVD.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out  = Join-Path $root 'connector-distributer-api.zip'
if (Test-Path $out) { Remove-Item $out }

$exclude = @('\\bin\\', '\\obj\\', '\\.git\\', '\\.vs\\', '\\.idea\\', 'connector-distributer-api\.zip')
$files = Get-ChildItem -Path $root -Recurse -File |
    Where-Object { $p = $_.FullName; -not ($exclude | Where-Object { $p -match $_ }) }

Push-Location $root
try {
    Compress-Archive -Path ($files | ForEach-Object { $_.FullName.Substring($root.Length + 1) }) -DestinationPath $out
}
finally { Pop-Location }

Write-Host "Wrote $out ($([math]::Round((Get-Item $out).Length / 1KB)) KB, $($files.Count) files)"
