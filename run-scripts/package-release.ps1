param([string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repository = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
# An isolated output avoids packaging local settings, logs, cached tiles, or obsolete executables.
$staging = Join-Path $repository ('artifacts/package-' + [Guid]::NewGuid().ToString('N'))
$output = Join-Path $staging 'app'
dotnet build (Join-Path $repository 'MissionPlanner.csproj') -c $Configuration "-p:OutputPath=$output/"
if ($LASTEXITCODE -ne 0) { throw 'Native Release build failed.' }
foreach ($required in @('Fly.io-GCS.exe','Fly.io-GCS.exe.config','Updater.exe')) {
 if (!(Test-Path -LiteralPath (Join-Path $output $required))) { throw "Missing release file: $required" }
}
Copy-Item -LiteralPath (Join-Path $repository 'COPYING.txt') -Destination $output
Copy-Item -LiteralPath (Join-Path $repository 'NOTICE-Fly.io-GCS.txt') -Destination $output
Copy-Item -LiteralPath (Join-Path $repository 'README.md') -Destination $output
$licenseRoot = Join-Path $output 'third-party-notices'
Get-ChildItem -LiteralPath (Join-Path $repository 'ExtLibs') -Recurse -File |
 Where-Object { $_.Name -match '^(LICENSE|LICENCE|COPYING|NOTICE|COPYRIGHT)([.-]|$)' -and $_.FullName -notmatch '\\(bin|obj|packages)\\' } |
 ForEach-Object {
  $relative = $_.FullName.Substring($repository.Length + 1)
  $destination = Join-Path $licenseRoot $relative
  New-Item -ItemType Directory -Force -Path (Split-Path $destination) | Out-Null
  Copy-Item -LiteralPath $_.FullName -Destination $destination
 }
$zip = Join-Path $repository 'artifacts/Fly.io-GCS-Windows.zip'
if (Test-Path -LiteralPath $zip) { throw "Package already exists; move it aside before creating a new package: $zip" }
Compress-Archive -Path (Join-Path $output '*') -DestinationPath $zip
Write-Output "Prepared $zip (not published). Staged files: $output"
