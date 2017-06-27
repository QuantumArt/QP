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

  $p = Get-Item "IIS:\sites\$name\Plugins" -ErrorAction SilentlyContinue
  $b = Get-Item "IIS:\sites\$name\Backend" -ErrorAction SilentlyContinue
  $w = Get-Item "IIS:\sites\$name\Backend\Winlogon" -ErrorAction SilentlyContinue

$path = $s.PhysicalPath

if ($p) { $pluginsPath = $p.PhysicalPath }
if ($b) { $backendPath = $b.PhysicalPath }
if ($w) { $winlogonPath = $w.PhysicalPath }

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
    Write-Verbose "AppPool $($s.applicationPool) is stopping..."
    Start-Sleep -Milliseconds 500
  }
}

Write-Verbose "Stopped"

Write-Verbose "Creating backup files ..."
Compress-Archive -Path $backendPath\*.* -DestinationPath $path\Backend_old.zip
Compress-Archive -Path $winlogonPath\*.* -DestinationPath $path\Winlogon_old.zip
Compress-Archive -Path $pluginsPath\*.* -DestinationPath $path\plugins_old.zip
Write-Verbose "Done"

Write-Verbose "Removing files for $name from $backendPath..."
Get-ChildItem -Path $backendPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done"

Write-Verbose "Removing files for $name from $winlogonPath..."
Get-ChildItem -Path $winlogonPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done"

Write-Verbose "Removing files for $name from $pluginsPath..."
Get-ChildItem -Path $pluginsPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done"

Write-Verbose "Unarchiving zip-files to $backendPath..."
Expand-Archive -LiteralPath $backendSource -DestinationPath $backendPath 
Write-Verbose "Done"

Write-Verbose "Unarchiving zip-files to $winLogonPath..."
Expand-Archive -LiteralPath $winLogonSource -DestinationPath $winLogonPath
Write-Verbose "Done"

Write-Verbose "Unarchiving zip-files to $pluginsPath..."
Expand-Archive -LiteralPath $pluginsSource -DestinationPath $pluginsPath
Write-Verbose "Done"

if ($transform)
{
    $command = "TransformSite.ps1 -source ""$backendPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command

    $command = "TransformSite.ps1 -source ""$winLogonPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command
}

Write-Verbose "AppPool $($s.applicationPool) is starting"
$p.Start()
