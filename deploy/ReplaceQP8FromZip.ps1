param(
  [String] $name='QP8',
  [String] $source='',
  [Bool] $transform=$true,
  [String] $config='Default',
  [String] $configFiles='Web,NLog',
  [String] $backupPath = ''
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
$dir = Split-Path $path -Leaf
$isRootSites = ($dir -eq "sites")

if ($p) { $pluginsPath = $p.PhysicalPath }
if ($b) { $backendPath = $b.PhysicalPath }
if ($w) { $winlogonPath = $w.PhysicalPath }

$backendSource = Join-Path $source "Backend.zip"
$winLogonSource = Join-Path $source "WinLogon.zip"
$pluginsSource = Join-Path $source "plugins.zip"
$sitesSource = Join-Path $source "sites.zip"

if ([string]::IsNullOrEmpty($pluginsPath)) { throw [System.ArgumentException] "Virtual directory 'plugins' is not found"}
if ([string]::IsNullOrEmpty($backendPath)) { throw [System.ArgumentException] "Web application 'backend' is not found"}
if ([string]::IsNullOrEmpty($winlogonPath)) { throw [System.ArgumentException] "Web application 'Backend/Winlogon' is not found"}

if (-not(Test-Path $backendSource)) { throw [System.ArgumentException] "File $backendSource not exists"}
if (-not(Test-Path $winLogonSource)) { throw [System.ArgumentException] "File $winLogonSource not exists"}
if (-not(Test-Path $pluginsSource)) { throw [System.ArgumentException] "File $pluginsSource not exists"}
if (-not(Test-Path $sitesSource)) { throw [System.ArgumentException] "File $sitesSource not exists"}

$p = Get-Item "IIS:\AppPools\$($s.applicationPool)"

if ($p.State -ne "Stopped"){
  $p.Stop()

  while($p.State -ne "Stopped"){
    Write-Verbose "AppPool $($s.applicationPool) is stopping..." -Verbose
    Start-Sleep -Milliseconds 500
  }
}

Write-Verbose "Stopped" -Verbose

Write-Verbose "Checking backup path for $backupPath..." -Verbose
if ([String]::IsNullOrEmpty($backupPath)) {
  $backupPath = if ($isRootSites) { (Split-Path $path -Parent)} Else { $path }
}
Write-Verbose "Done" -Verbose

if (-not(Test-Path $backupPath)) {
  Write-Verbose "Directory $backupPath not found. Creating..." -Verbose
  New-Item -Force $backupPath -ItemType Directory
  Write-Verbose "Done" -Verbose
}

Write-Verbose "Preparing zip-files to $backupPath..." -Verbose
$command = "CreateBackup.ps1 -sourcePath ""$backendPath"" -name Backend -path ""$backupPath"" -separateFolder `$false
            CreateBackup.ps1 -sourcePath ""$winlogonPath"" -name WinLogon -path ""$backupPath"" -separateFolder `$false
            CreateBackup.ps1 -sourcePath ""$pluginsPath"" -name plugins -path ""$backupPath"" -separateFolder `$false"
Invoke-Expression -command $command
if ($isRootSites)
{
  $command = "CreateBackup.ps1 -sourcePath ""$path"" -name sites -path ""$backupPath"" -separateFolder `$false"
  Invoke-Expression -command $command
}
Write-Verbose "Done" -Verbose

if ($isRootSites)
{
  Write-Verbose "Removing files for $name from $path..." -Verbose
  Get-ChildItem -Path $path -Recurse | Remove-Item -force -recurse
  Write-Verbose "Done" -Verbose
}

Write-Verbose "Removing files for $name from $backendPath..." -Verbose
Get-ChildItem -Path $backendPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done" -Verbose

Write-Verbose "Removing files for $name from $winlogonPath..." -Verbose
Get-ChildItem -Path $winlogonPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done" -Verbose

Write-Verbose "Removing files for $name from $pluginsPath..." -Verbose
Get-ChildItem -Path $pluginsPath -Recurse | Remove-Item -force -recurse
Write-Verbose "Done" -Verbose

if ($isRootSites)
{
  Write-Verbose "Unarchiving zip-files to $path..." -Verbose
  Expand-Archive -LiteralPath $sitesSource -DestinationPath $path
  Write-Verbose "Done" -Verbose
}

Write-Verbose "Unarchiving zip-files to $backendPath..." -Verbose
Expand-Archive -LiteralPath $backendSource -DestinationPath $backendPath
Write-Verbose "Done" -Verbose

Write-Verbose "Unarchiving zip-files to $winLogonPath..." -Verbose
Expand-Archive -LiteralPath $winLogonSource -DestinationPath $winLogonPath
Write-Verbose "Done" -Verbose

Write-Verbose "Unarchiving zip-files to $pluginsPath..." -Verbose
Expand-Archive -LiteralPath $pluginsSource -DestinationPath $pluginsPath
Write-Verbose "Done" -Verbose

if ($transform)
{
    $command = "TransformSite.ps1 -source ""$backendPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command

    $command = "TransformSite.ps1 -source ""$winLogonPath"" -config ""$config"" -configFiles ""$configFiles"""
    Invoke-Expression -command $command
}

Write-Verbose "AppPool $($s.applicationPool) is starting" -Verbose
$p.Start()
