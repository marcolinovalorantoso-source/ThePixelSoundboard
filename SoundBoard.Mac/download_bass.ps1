# PowerShell script to download BASS native libraries for Windows and macOS

$AppDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($AppDir)) {
    $AppDir = Get-Location
}

# Create a Temp directory
$TempDir = Join-Path $AppDir "TempBass"
if (Test-Path $TempDir) {
    Remove-Item -Path $TempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

Write-Host "Downloading BASS for Windows..."
$WinUrl = "https://www.un4seen.com/files/bass24.zip"
$WinZip = Join-Path $TempDir "bass24.zip"
Invoke-WebRequest -Uri $WinUrl -OutFile $WinZip

Write-Host "Downloading BASS for macOS..."
$MacUrl = "https://www.un4seen.com/files/bass24-osx.zip"
$MacZip = Join-Path $TempDir "bass24-osx.zip"
Invoke-WebRequest -Uri $MacUrl -OutFile $MacZip

Write-Host "Extracting Windows BASS..."
$WinExtract = Join-Path $TempDir "win"
Expand-Archive -Path $WinZip -DestinationPath $WinExtract -Force
# Copy bass.dll (64-bit is in x64 subfolder)
$WinDll64 = Join-Path $WinExtract "x64\bass.dll"
Copy-Item -Path $WinDll64 -Destination (Join-Path $AppDir "bass.dll") -Force

Write-Host "Extracting macOS BASS..."
$MacExtract = Join-Path $TempDir "mac"
Expand-Archive -Path $MacZip -DestinationPath $MacExtract -Force
# Copy libbass.dylib
$MacDylib = Join-Path $MacExtract "libbass.dylib"
Copy-Item -Path $MacDylib -Destination (Join-Path $AppDir "libbass.dylib") -Force

# Clean up Temp directory
Remove-Item -Path $TempDir -Recurse -Force

Write-Host "BASS libraries downloaded and copied successfully!"
Write-Host "- Windows 64-bit: bass.dll"
Write-Host "- macOS: libbass.dylib"
