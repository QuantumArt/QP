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
$cdcElasticSchedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.CdcDataImport.Elastic"
$cdcTarantoolSchedulerSource = Join-Path $source "QuantumArt.Schedulers\Quantumart.QP8.CdcDataImport.Tarantool"

$installSchedulerSource = Join-Path $source "deploy\InstallArticleScheduler.ps1"
$installCommonSchedulerSource = Join-Path $source "deploy\InstallCommonScheduler.ps1"
$installElasticCdcSchedulerSource = Join-Path $source "deploy\InstallCdcElasticService.ps1"
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
if (-not(Test-Path $cdcElasticSchedulerSource)) { throw [System.ArgumentException] "Folder $cdcElasticSchedulerSource not exists"}
if (-not(Test-Path $cdcTarantoolSchedulerSource)) { throw [System.ArgumentException] "Folder $cdcTarantoolSchedulerSource not exists"}

if (-not(Test-Path $installSchedulerSource)) { throw [System.ArgumentException] "File $installSchedulerSource not exists"}
if (-not(Test-Path $installCommonSchedulerSource)) { throw [System.ArgumentException] "File $installCommonSchedulerSource not exists"}
if (-not(Test-Path $installElasticCdcSchedulerSource)) { throw [System.ArgumentException] "File $installElasticCdcSchedulerSource not exists"}
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

Write-Output "Removing sources for $cdcElasticSchedulerSource"
Invoke-Expression "CleanSource.ps1 -source '$cdcElasticSchedulerSource'"
Write-Output "Done"

Write-Output "Removing sources for $cdcTarantoolSchedulerSource"
Invoke-Expression "CleanSource.ps1 -source '$cdcTarantoolSchedulerSource'"
Write-Output "Done"

Compress-Archive -Path $backendSource\* -DestinationPath $parentSource\Backend.zip
Compress-Archive -Path $winLogonSource\* -DestinationPath $parentSource\Winlogon.zip
Compress-Archive -Path $pluginsSource\* -DestinationPath $parentSource\plugins.zip
Compress-Archive -Path $sitesSource\* -DestinationPath $parentSource\sites.zip
Compress-Archive -Path $qaSource\* -DestinationPath $parentSource\qa.zip

Compress-Archive -Path $schedulerSource\* -DestinationPath $parentSource\ArticleScheduler.zip
Compress-Archive -Path $commonSchedulerSource\* -DestinationPath $parentSource\CommonScheduler.zip
Compress-Archive -Path $cdcElasticSchedulerSource\* -DestinationPath $parentSource\CdcElasticScheduler.zip
Compress-Archive -Path $cdcTarantoolSchedulerSource\* -DestinationPath $parentSource\CdcTarantoolScheduler.zip

Copy-Item $installQp8Source $parentSource
Copy-Item $replaceQp8Source $parentSource
Copy-Item $currentSqlSource $parentSource

Copy-Item $installSchedulerSource $parentSource
Copy-Item $installCommonSchedulerSource $parentSource
Copy-Item $installElasticCdcSchedulerSource $parentSource
Copy-Item $installTarantoolCdcSchedulerSource $parentSource
