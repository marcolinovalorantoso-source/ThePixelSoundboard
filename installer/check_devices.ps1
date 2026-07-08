$active = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render"
$items = Get-ChildItem $active
foreach ($item in $items) {
    $path = Join-Path $item.PSPath "Properties"
    if (Test-Path $path) {
        $allProps = Get-ItemProperty -Path $path
        Write-Output "--- RENDER DEVICE ---"
        $allProps.PSObject.Properties | Where-Object { $_.Value -is [string] } | Select-Object Name, Value | Format-List
    }
}

$capture = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture"
$items2 = Get-ChildItem $capture
foreach ($item in $items2) {
    $path = Join-Path $item.PSPath "Properties"
    if (Test-Path $path) {
        $allProps = Get-ItemProperty -Path $path
        Write-Output "--- CAPTURE DEVICE ---"
        $allProps.PSObject.Properties | Where-Object { $_.Value -is [string] } | Select-Object Name, Value | Format-List
    }
}
