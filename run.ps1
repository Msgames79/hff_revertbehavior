Set-Location $PSScriptRoot
$pluginName = "hff_RevertBehavior"

dotnet build
if ($?) {
    dotnet build -c Release
    $ErrorActionPreference = 'Continue'
    if (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
        "Waiting HFF to dddClose"
        Stop-Process -Name "Human"<#, "dnSpy"#> -ErrorAction SilentlyContinue
        while (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
            Start-Sleep -Seconds 1
        }
    }
    do {
        Remove-Item "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\${pluginName}.dll" -Force
    } while (Test-Path "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\${pluginName}.dll")
    do {
        Copy-Item "bin\Debug\net46\${pluginName}.dll" "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\${pluginName}.dll" -Force
    } until (Test-Path "C:\Program Files (x86)\Steam\steamapps\common\Human Fall Flat\BepInEx\plugins\${pluginName}.dll")
    Start-Process "steam://rungameid/477160"
} else {
    "Build failed"
}