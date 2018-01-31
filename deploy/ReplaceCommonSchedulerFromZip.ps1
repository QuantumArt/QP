param(
  [ValidateNotNullOrEmpty()]
  [String] $name='qp8.notification',
  [ValidatePattern("\d{2}:\d{2}:\d{2}")]
  [String] $timeout = "00:05:00",
  [ValidateNotNullOrEmpty()]
  [String] $config='Default',
  [ValidateNotNullOrEmpty()]
  [String] $backupPath='c:\temp\backups'
)

$files = 'App,NLog,NLog.Users,NLog.Interface.Notifications,NLog.Interface.Notifications.Cleanup,NLog.System.Notifications,NLog.System.Notifications.Cleanup'

Replace-WinService -source "$PSScriptRoot\CommonScheduler.zip" -name $name -timeout $timeout -config $config `
  -configFiles $files -backupPath $backupPath -replaceFromBin $true -buildConfig 'Release' -dependentServices qp8.users -Verbose
