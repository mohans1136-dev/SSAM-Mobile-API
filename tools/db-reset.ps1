# Drops and rebuilds the local dev database in SQL Server LocalDB, then seeds it.
# Usage:  powershell -ExecutionPolicy Bypass -File tools\db-reset.ps1
$ErrorActionPreference = 'Stop'
$root     = Split-Path -Parent $PSScriptRoot
$instance = '(localdb)\MSSQLLocalDB'
$dbName   = 'SsamMobileApiLocal'

Write-Host "Starting LocalDB..."
sqllocaldb start MSSQLLocalDB | Out-Null

Write-Host "Dropping $dbName if it exists..."
sqlcmd -S $instance -E -C -b -Q @"
IF DB_ID('$dbName') IS NOT NULL
BEGIN
    ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$dbName];
END
"@

foreach ($script in @('01-schema.sql', '02-seed.sql')) {
    $path = Join-Path $root "db\$script"
    Write-Host "Running $script..."
    sqlcmd -S $instance -E -C -b -i $path
}

Write-Host ""
Write-Host "Done. Connection string:"
Write-Host "  Server=(localdb)\MSSQLLocalDB;Database=$dbName;Trusted_Connection=True;TrustServerCertificate=True;"
