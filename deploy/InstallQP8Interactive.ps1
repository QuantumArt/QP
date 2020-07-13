# restart as admin
If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    $arguments = "-noexit & '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}


function Read-ValueOrDefault([string] $message, [string] $defaultValue) {
    $value = Read-Host "$message (default - $defaultValue)"
    $value = if ($value) { $value } else { $defaultValue }
    return $value
}

function Read-IntValueOrDefault([string] $message, [int] $defaultValue) {
  $intValue = 0;
  $value = Read-Host "$message (default - $defaultValue)"
  $intValue = if ([int32]::TryParse($value, [ref] $intValue)) { $intValue } else { $defaultValue }
  return $intValue
}

$name = Read-ValueOrDefault "Please enter site name to install" "QP8"
$port = Read-IntValueOrDefault "Please enter port for site $name" 89
$configFile = Read-Host "Please enter path to existing configuration file or leave empty for new or service configuration"
if (!$configFile) {
    $configUrl = Read-Host "Please enter configuration service URL or leave empty for local configuration"
    if (!$configUrl) {
        $configDir = Read-ValueOrDefault  "Please enter configuration directory" "C:\QA"
    } else {
        $configToken = Read-Host "Please enter token for configuration service URL"
    }
}

$tempDir = Read-ValueOrDefault  "Please enter directory for temp files" "C:\Temp"
$logDir = Read-ValueOrDefault  "Please enter directory for log files" "C:\Logs"

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$scriptName = Join-Path $currentPath "InstallQP8.ps1"
Invoke-Expression "$scriptName -name $name -port $port -configUrl '$configUrl' -configToken '$configToken' -configDir '$configDir' -tempDir '$tempDir' -logDir '$logDir'"


