# Run this OUTSIDE the AVD, on a machine that can reach nuget.org.
# It downloads every NuGet package the solution needs into a folder and zips it
# with a ready-to-use nuget.config, so the AVD can restore/build with no network.
#
#   powershell -ExecutionPolicy Bypass -File tools\pack-offline-nuget.ps1
#
# Then copy offline-nuget.zip into the AVD and extract it into the solution
# folder (P:\Core\SSAMMobileApp\). See docs/migrate-to-avd.md.

$ErrorActionPreference = 'Stop'
$root    = Split-Path -Parent $PSScriptRoot
$pkgDir  = Join-Path $root '_offline-nuget'
$cfg     = Join-Path $root 'nuget.config'
$zip     = Join-Path $root 'offline-nuget.zip'

Push-Location $root
try {
    if (Test-Path $pkgDir) { Remove-Item $pkgDir -Recurse -Force }
    if (Test-Path $zip)    { Remove-Item $zip -Force }

    Write-Host "Restoring all packages into $pkgDir ..."
    dotnet restore SSAMMobileApp.sln --packages $pkgDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

    # nuget.config that makes restore use ONLY the carried-in folder.
    @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <!-- Use the packages carried in from outside the AVD. -->
    <add key="globalPackagesFolder" value="_offline-nuget" />
  </config>
  <packageSources>
    <!-- No online sources. Everything must already be in _offline-nuget. -->
    <clear />
  </packageSources>
</configuration>
'@ | Set-Content -Path $cfg -Encoding utf8

    $tempCfg = Join-Path $pkgDir '..\nuget.config'
    Compress-Archive -Path $pkgDir, $cfg -DestinationPath $zip
    Remove-Item $cfg -Force   # keep the repo clean; it's zipped, not committed

    $mb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
    $count = (Get-ChildItem $pkgDir -Directory).Count
    Write-Host ""
    Write-Host "Wrote $zip  ($mb MB, $count packages)"
    Write-Host "Copy it into the AVD and extract into P:\Core\SSAMMobileApp\"
}
finally { Pop-Location }
