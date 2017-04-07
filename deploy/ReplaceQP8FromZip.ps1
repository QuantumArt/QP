param(
  [String] $name='QP8',
  [String] $source='',
  [Bool] $transform=$true,
  [String] $config='Default',
  [String] $configFiles='Web,NLog'
)

Import-Module WebAdministration

if ([string]::IsNullOrEmpty($source)) { throw [System.ArgumentException] "Parameter 'source' is empty"}
if ([string]::IsNullOrEmpty($name)) { throw [System.ArgumentException] "Parameter 'name' is empty"}
if ([string]::IsNullOrEmpty($config) -and $transform) { throw [System.ArgumentException] "Parameter 'config' is empty while 'transform' is true"}
if ([string]::IsNullOrEmpty($configFiles) -and $transform) { throw [System.ArgumentException] "Parameter 'configFiles' is empty while 'transform' is true"}

if (-not(Test-Path $source)) { throw [System.ArgumentException] "Folder $source not exists"}

try {
  $s = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue

} catch {
  # http://help.octopusdeploy.com/discussions/problems/5172-error-using-get-website-in-predeploy-because-of-filenotfoundexception
  $s = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue
}

if (!$s) { throw "Site $name not found"}

$path = $s.PhysicalPath

$pluginsPath = Join-Path $path "Plugins"
$p = Get-WebVirtualDirectory -Site $name -Name "Plugins" -ErrorAction SilentlyContinue
if ($p) { $pluginsPath = $p.PhysicalPath }

$backendPath = (Get-WebApplication -Site $name -Name "Backend").PhysicalPath
$winlogonPath = (Get-WebApplication -Site $name -Name "Backend/Winlogon").PhysicalPath

$backendSource = Join-Path $source "Backend.zip"
$winLogonSource = Join-Path $source "WinLogon.zip"


$pluginsSource = Join-Path $source "plugins.zip"

if ([string]::IsNullOrEmpty($pluginsPath)) { throw [System.ArgumentException] "Virtual directory 'plugins' is not found"}
if ([string]::IsNullOrEmpty($backendPath)) { throw [System.ArgumentException] "Web application 'backend' is not found"}
if ([string]::IsNullOrEmpty($winlogonPath)) { throw [System.ArgumentException] "Web application 'Backend/Winlogon' is not found"}


if (-not(Test-Path $backendSource)) { throw [System.ArgumentException] "File $backendSource not exists"}
if (-not(Test-Path $winLogonSource)) { throw [System.ArgumentException] "File $winLogonSource not exists"}
if (-not(Test-Path $pluginsSource)) { throw [System.ArgumentException] "File $pluginsSource not exists"}


$p = Get-Item "IIS:\AppPools\$($s.applicationPool)"

if ($p.State -ne "Stopped"){
  $p.Stop()

  while($p.State -ne "Stopped"){
    Write-Output "AppPool $($s.applicationPool) is stopping..."
    Start-Sleep -Milliseconds 500
  }
}

Write-Output "Stopped"

Write-Output "Creating backup files ..."
Invoke-Expression "7za.exe a -r -y ""$path\Backend_old.zip"" ""$backendPath\*.*"""
Invoke-Expression "7za.exe a -r -y ""$path\Winlogon_old.zip"" ""$winlogonPath\*.*"""
Invoke-Expression "7za.exe a -r -y ""$path\plugins_old.zip"" ""$pluginsPath\*.*"""
Write-Output "Done"

Write-Output "Removing files for $name from $backendPath..."
Get-ChildItem -Path $backendPath -Recurse | Remove-Item -force -recurse
Write-Output "Done"

Write-Output "Removing files for $name from $winlogonPath..."
Get-ChildItem -Path $winlogonPath -Recurse | Remove-Item -force -recurse
Write-Output "Done"

Write-Output "Removing files for $name from $pluginsPath..."
Get-ChildItem -Path $pluginsPath -Recurse | Remove-Item -force -recurse
Write-Output "Done"

Write-Output "Unarchiving zip-files to $backendPath..."
Invoke-Expression "7za.exe x -r -y -o""$backendPath"" ""$backendSource"""
Write-Output "Done"

Write-Output "Unarchiving zip-files to $winLogonPath..."
Invoke-Expression "7za.exe x -r -y -o""$winLogonPath"" ""$winLogonSource"""
Write-Output "Done"

Write-Output "Unarchiving zip-files to $pluginsPath..."
Invoke-Expression "7za.exe x -r -y -o""$pluginsPath"" ""$pluginsSource"""
Write-Output "Done"

if ($transform)
{
    $command = "TransformSite.ps1 -source ""$backendPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command

    $command = "TransformSite.ps1 -source ""$winLogonPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command
}

Write-Output "AppPool $($s.applicationPool) is starting"
$p.Start()
