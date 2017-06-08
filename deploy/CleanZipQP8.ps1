param(
  [String] $source = '',
  [String] $config = 'Release'
)

if ([string]::IsNullOrEmpty($source)) { throw [System.ArgumentException] "Parameter 'source' is empty"}
if (-not(Test-Path $source)) { throw [System.ArgumentException] "Folder $source not exists"}

$backendSource = Join-Path $source "siteMvc"
$winLogonSource = Join-Path $source "WinLogonMvc"
$pluginsSource = Join-Path $source "plugins"
$sitesSource = Join-Path $source "sites"
$qaSource = Join-Path $source "QA"

$installQp8Source = Join-Path $source "deploy\InstallQP8.ps1"
$replaceQp8Source = Join-Path $source "deploy\ReplaceQP8FromZip.ps1"
$currentSqlSource = Join-Path $source "dal\scripts\current.sql"

$schedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.ArticleScheduler.WinService"
$commonSchedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.Scheduler.Service"
$cdcTarantoolSchedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.CdcDataImport.Tarantool"
$installSchedulerSource = Join-Path $source "deploy\InstallArticleScheduler.ps1"
$installCommonSchedulerSource = Join-Path $source "deploy\InstallCommonScheduler.ps1"
$installTarantoolCdcSchedulerSource = Join-Path $source "deploy\InstallCdcTarantoolService.ps1"

$parentSource = Join-Path $source "zip"
New-Item $parentSource -type directory

if (-not(Test-Path $backendSource)) { throw [System.ArgumentException] "Folder $backendSource not exists"}
if (-not(Test-Path $winLogonSource)) { throw [System.ArgumentException] "Folder $winLogonSource not exists"}
if (-not(Test-Path $parentSource)) { throw [System.ArgumentException] "Folder $parentSource not exists"}
if (-not(Test-Path $pluginsSource)) { throw [System.ArgumentException] "Folder $pluginsSource not exists"}
if (-not(Test-Path $sitesSource)) { throw [System.ArgumentException] "Folder $sitesSource not exists"}
if (-not(Test-Path $qaSource)) { throw [System.ArgumentException] "Folder $qaSource not exists"}
if (-not(Test-Path $installQp8Source)) { throw [System.ArgumentException] "File $installQp8Source not exists"}
if (-not(Test-Path $replaceQp8Source)) { throw [System.ArgumentException] "File $replaceQp8Source not exists"}
if (-not(Test-Path $currentSqlSource)) { throw [System.ArgumentException] "File $currentSqlSource not exists"}

if (-not(Test-Path $schedulerSource)) { throw [System.ArgumentException] "Folder $schedulerSource not exists"}
if (-not(Test-Path $commonSchedulerSource)) { throw [System.ArgumentException] "Folder $commonSchedulerSource not exists"}
if (-not(Test-Path $cdcTarantoolSchedulerSource)) { throw [System.ArgumentException] "Folder $cdcTarantoolSchedulerSource not exists"}
if (-not(Test-Path $installSchedulerSource)) { throw [System.ArgumentException] "File $installSchedulerSource not exists"}
if (-not(Test-Path $installCommonSchedulerSource)) { throw [System.ArgumentException] "File $installCommonSchedulerSource not exists"}
if (-not(Test-Path $installTarantoolCdcSchedulerSource)) { throw [System.ArgumentException] "File $installTarantoolCdcSchedulerSource not exists"}

Write-Output "Removing sources for $backendSource"
Invoke-Expression "CleanSource.ps1 -source '$backendSource' -removeViews `$true"
Write-Output "Done"

Write-Output "Removing sources for $winLogonSource"
Invoke-Expression "CleanSource.ps1 -source '$winLogonSource' -removeViews `$true"
Write-Output "Done"

Write-Output "Removing sources for $schedulerSource"
Invoke-Expression "CleanSource.ps1 -source '$schedulerSource'"
Write-Output "Done"

Write-Output "Removing sources for $commonSchedulerSource"
Invoke-Expression "CleanSource.ps1 -source '$commonSchedulerSource'"
Write-Output "Done"

Write-Output "Removing sources for $cdcTarantoolSchedulerSource"
Invoke-Expression "CleanSource.ps1 -source '$cdcTarantoolSchedulerSource'"
Write-Output "Done"

Invoke-Expression "7za.exe a -r -y ""$parentSource\Backend.zip"" ""$backendSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\Winlogon.zip"" ""$winLogonSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\plugins.zip"" ""$pluginsSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\sites.zip"" ""$sitesSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\qa.zip"" ""$qaSource\*.*"""

Invoke-Expression "7za.exe a -r -y ""$parentSource\ArticleScheduler.zip"" ""$schedulerSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\CommonScheduler.zip"" ""$commonSchedulerSource\*.*"""
Invoke-Expression "7za.exe a -r -y ""$parentSource\CdcTarantoolScheduler.zip"" ""$cdcTarantoolSchedulerSource\*.*"""

Copy-Item $installQp8Source $parentSource
Copy-Item $replaceQp8Source $parentSource
Copy-Item $currentSqlSource $parentSource

Copy-Item $installSchedulerSource $parentSource
Copy-Item $installCommonSchedulerSource $parentSource
Copy-Item $installTarantoolCdcSchedulerSource $parentSource
