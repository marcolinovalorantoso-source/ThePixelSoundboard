# Auto-elevate to admin if needed
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Start-Process PowerShell -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$nameKey = "{a45c254e-df1c-4efd-8020-67d146a850e0},2"
$active = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render"
$capture = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture"

# Controlla se è già rinominato per evitare di attendere inutilmente
$alreadyRenamed = $false
if (Test-Path $active) {
    Get-ChildItem $active | ForEach-Object {
        $path = Join-Path $_.PSPath "Properties"
        if (Test-Path $path) {
            $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
            if ($name -eq "ThePixelSoundboard Audio") {
                $alreadyRenamed = $true
            }
        }
    }
}

if (-not $alreadyRenamed) {
    # Loop di attesa breve (fino a 10 secondi) finché non viene installato e registrato VB-Cable
    $found = $false
    for ($i = 0; $i -lt 5; $i++) {
        if (Test-Path $active) {
            $items = Get-ChildItem $active
            foreach ($item in $items) {
                $path = Join-Path $item.PSPath "Properties"
                if (Test-Path $path) {
                    $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
                    if ($name -like "*CABLE Input*") {
                        $found = $true
                        break
                    }
                }
            }
        }
        if ($found) { break; }
        Start-Sleep -Seconds 2
    }

    # Attesa finale prima delle modifiche
    Start-Sleep -Seconds 1

    # Rinomina il dispositivo di riproduzione CABLE Input -> ThePixelSoundboard Audio
    if (Test-Path $active) {
        Get-ChildItem $active | ForEach-Object {
            $path = Join-Path $_.PSPath "Properties"
            if (Test-Path $path) {
                try {
                    $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
                    if ($name -like "*CABLE Input*") {
                        Set-ItemProperty -Path $path -Name $nameKey -Value "ThePixelSoundboard Audio"
                    }
                } catch {}
            }
        }
    }

    # Rinomina il dispositivo di registrazione CABLE Output -> ThePixelSoundboard Mic
    if (Test-Path $capture) {
        Get-ChildItem $capture | ForEach-Object {
            $path = Join-Path $_.PSPath "Properties"
            if (Test-Path $path) {
                try {
                    $name = (Get-ItemProperty -Path $path -Name $nameKey -ErrorAction SilentlyContinue).$nameKey
                    if ($name -like "*CABLE Output*") {
                        Set-ItemProperty -Path $path -Name $nameKey -Value "ThePixelSoundboard Mic"
                    }
                } catch {}
            }
        }
    }
}
