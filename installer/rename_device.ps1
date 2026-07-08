# Auto-elevate to admin if needed
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Start-Process PowerShell -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Start-Sleep -Seconds 3

$nameKey = "{a45c254e-df1c-4efd-8020-67d146a850e0},2"

# Rinomina il dispositivo di riproduzione CABLE Input -> ThePixelSoundboard Audio
$active = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render"
Get-ChildItem $active | ForEach-Object {
    $path = Join-Path $_.PSPath "Properties"
    if (Test-Path $path) {
        try {
            $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
            if ($name -like "*CABLE Input*") {
                Set-ItemProperty -Path $path -Name $nameKey -Value "ThePixelSoundboard Audio"
                Write-Output "Rinominato Render: $name -> ThePixelSoundboard Audio"
            }
        } catch {}
    }
}

# Rinomina il dispositivo di registrazione CABLE Output -> ThePixelSoundboard Mic
$capture = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture"
Get-ChildItem $capture | ForEach-Object {
    $path = Join-Path $_.PSPath "Properties"
    if (Test-Path $path) {
        try {
            $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
            if ($name -like "*CABLE Output*") {
                Set-ItemProperty -Path $path -Name $nameKey -Value "ThePixelSoundboard Mic"
                Write-Output "Rinominato Capture: $name -> ThePixelSoundboard Mic"
            }
        } catch {}
    }
}

Write-Output "Operazione completata! Riavvia il PC per vedere i nuovi nomi."
