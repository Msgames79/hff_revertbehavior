Set-Location $PSScriptRoot

dotnet build
if ($?) {
    $ErrorActionPreference = 'Continue'
    if (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
        "Waiting HFF to dddClose"
        Stop-Process -Name "Human", "dnSpy" -ErrorAction SilentlyContinue
        while (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
            Start-Sleep -Seconds 1
        }
    }
    do {
        Remove-Item "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\hff_RevertBehavior.dll" -Force
    } while (Test-Path "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\hff_RevertBehavior.dll")
    do {
        Copy-Item "bin\Debug\net46\hff_RevertBehavior.dll" "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\hff_RevertBehavior.dll" -Force
    } until (Test-Path "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\hff_RevertBehavior.dll")
    Start-Process "steam://rungameid/477160"
} else {
    "Build failed"
}