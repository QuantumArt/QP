param(
  [String] $source = '',
  [String] $config = 'Release'
)

if ([string]::IsNullOrEmpty($source)) { throw [System.ArgumentException] "Parameter 'source' is empty"}
if (-not(Test-Path $source)) { throw [System.ArgumentException] "Folder $source not exists"}

$backendSource = Join-Path $source "siteMvc"
$winLogonSource = Join-Path $source "WinLogonMvc"
$schedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.ArticleScheduler.WinService\bin\$config"
$commonSchedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.Scheduler.Service\bin\$config"
$pluginsSource = Join-Path $source "plugins"
$sitesSource = Join-Path $source "sites"
$qaSource = Join-Path $source "QA"

$installQp8Source = Join-Path $source "deploy\InstallQP8.ps1"
$replacelQp8Source = Join-Path $source "deploy\ReplaceQP8FromZip.ps1"
$currentSqlSource = Join-Path $source "dal\scripts\current.sql"
$installSchedulerSource = Join-Path $source "deploy\InstallArticleScheduler.ps1"
$installCommonSchedulerSource = Join-Path $source "deploy\InstallCommonScheduler.ps1"

$parentSource = Join-Path $source "zip"
New-Item $parentSource -type directory

if (-not(Test-Path $backendSource)) { throw [System.ArgumentException] "Folder $backendSource not exists"}
if (-not(Test-Path $winLogonSource)) { throw [System.ArgumentException] "Folder $winLogonSource not exists"}
if (-not(Test-Path $parentSource)) { throw [System.ArgumentException] "Folder $parentSource not exists"}
if (-not(Test-Path $schedulerSource)) { throw [System.ArgumentException] "Folder $schedulerSource not exists"}
if (-not(Test-Path $pluginsSource)) { throw [System.ArgumentException] "Folder $pluginsSource not exists"}
if (-not(Test-Path $sitesSource)) { throw [System.ArgumentException] "Folder $sitesSource not exists"}
if (-not(Test-Path $qaSource)) { throw [System.ArgumentException] "Folder $qaSource not exists"}
if (-not(Test-Path $installQp8Source)) { throw [System.ArgumentException] "File $installQp8Source not exists"}
if (-not(Test-Path $replacelQp8Source)) { throw [System.ArgumentException] "File $replacelQp8Source not exists"}
if (-not(Test-Path $installSchedulerSource)) { throw [System.ArgumentException] "File $installSchedulerSource not exists"}
if (-not(Test-Path $currentSqlSource)) { throw [System.ArgumentException] "File $currentSqlSource not exists"}

Write-Output "Removing sources for $backendSource"
Invoke-Expression "CleanSource.ps1 -source '$backendSource' -removeViews `$true"
Write-Output "Done"

Write-Output "Removing sources for $winLogonSource"
Invoke-Expression "CleanSource.ps1 -source '$winLogonSource' -removeViews `$true"
Write-Output "Done"

Invoke-Expression "7za.exe a -r -y ""$parentSource\Backend.zip"" ""$backendSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\Winlogon.zip"" ""$winLogonSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\ArticleScheduler.zip"" ""$schedulerSource\*.*"""
if (Test-Path $commonSchedulerSource)
{
  Invoke-Expression "7za.exe a -r -y ""$parentSource\CommonScheduler.zip"" ""$commonSchedulerSource\*.*"""
}
Invoke-Expression "7za.exe a -r -y ""$parentSource\plugins.zip"" ""$pluginsSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\sites.zip"" ""$sitesSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\qa.zip"" ""$qaSource\*.*"""

Copy-Item $installQp8Source $parentSource
Copy-Item $replacelQp8Source $parentSource
Copy-Item $installSchedulerSource $parentSource
Copy-Item $currentSqlSource $parentSource
if (Test-Path $installCommonSchedulerSource)
{
  Copy-Item $installCommonSchedulerSource $parentSource
}
