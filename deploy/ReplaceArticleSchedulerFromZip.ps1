param(
  [ValidateNotNullOrEmpty()]
  [String] $name='ArticleSchedulerService',
  [ValidatePattern("\d{2}:\d{2}:\d{2}")]
  [String] $timeout = "00:05:00",
  [ValidateNotNullOrEmpty()]
  [String] $config='Default',
  [ValidateNotNullOrEmpty()]
  [String] $backupPath='c:\temp\backups'

)

Replace-WinService -source "$PSScriptRoot\ArticleScheduler.zip" -name $name -timeout $timeout -config $config `
  -configFiles 'App,NLog' -backupPath $backupPath -replaceFromBin $true -buildConfig 'Release' -Verbose

