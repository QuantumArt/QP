param(
  [ValidateNotNullOrEmpty()]
  [String] $name='QP8',
  [Parameter(Mandatory = $true)]
  [ValidateScript({Test-Path $_ -PathType Container})]
  [String] $source,
  [ValidateNotNullOrEmpty()]
  [String] $config='Default',
  [ValidateNotNullOrEmpty()]
  [String] $configFiles='Web,NLog',
  [ValidateNotNullOrEmpty()]
  [ValidateScript({Test-Path $_ -PathType Container})]
  [String] $backupPath = 'c:\temp\backups',
  [bool] $transform
)

$s = Get-SiteOrApplication -name $name -Verbose
$path = $s.PhysicalPath
$isRootSites = ((Split-Path $path -Leaf) -eq "sites")

$plugins = Get-SiteOrApplication -name $name -application "Plugins" -Verbose
$pluginsPath = if ($plugins.GetType().ToString() -eq "System.IO.DirectoryInfo") { $plugins.FullName } else { $plugins.PhysicalPath }
$backendPath = (Get-SiteOrApplication -name $name -application "Backend" -Verbose).PhysicalPath
$winlogonPath = (Get-SiteOrApplication -name $name -application "Backend\Winlogon" -Verbose).PhysicalPath

$backendSource = Join-Path $source "Backend.zip"
$winLogonSource = Join-Path $source "WinLogon.zip"
$pluginsSource = Join-Path $source "plugins.zip"
$sitesSource = Join-Path $source "sites.zip"

if (-not(Test-Path $backendSource)) { throw [System.ArgumentException] "File $backendSource not exists"}
if (-not(Test-Path $winLogonSource)) { throw [System.ArgumentException] "File $winLogonSource not exists"}
if (-not(Test-Path $pluginsSource)) { throw [System.ArgumentException] "File $pluginsSource not exists"}
if (-not(Test-Path $sitesSource)) { throw [System.ArgumentException] "File $sitesSource not exists"}

Stop-AppPool $s.applicationPool -Verbose

$pluginsBackupName = Get-SiteOrApplicationBackupName -name $name -application "Plugins"
$backendBackupName = Get-SiteOrApplicationBackupName -name $name -application "Backend"
$winlogonBackupName = Get-SiteOrApplicationBackupName -name $name -application "Backend\Winlogon"

Create-Backup -sourcePath $backendPath -name $backendBackupName -path $backupPath
Create-Backup -sourcePath $winlogonPath -name $winlogonBackupName -path $backupPath
Create-Backup -sourcePath $pluginsPath -name $pluginsBackupName -path $backupPath

if ($isRootSites)
{
  Create-Backup -sourcePath $path -name "sites" -path $backupPath
}

Replace-FolderContents $backendPath $backendSource -Verbose
Replace-FolderContents $winlogonPath $winLogonSource -Verbose
Replace-FolderContents $pluginsPath $pluginsSource -Verbose

if ($isRootSites)
{
  Replace-FolderContents $path $sitesSource -Verbose
}

Transform-Configuration -source $backendPath -config $config -configFiles $configFiles
Transform-Configuration -source $winLogonPath -config $config -configFiles $configFiles

Start-AppPool $s.applicationPool -Verbose
