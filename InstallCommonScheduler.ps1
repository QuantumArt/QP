If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

function Stop-And-Remove-Service([string] $name, [string] $timeout = "00:03:00")
{
    $s = Get-Service $name -ErrorAction SilentlyContinue

    if ($s) { 
        
    
        if ( $s.Status -eq "Running")
        {
            Write-Host "Stopping service $name..."
            $s.Stop()
            try { $s.WaitForStatus("Stopped", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been stopped in '$timeout' interval" } 
            Write-Host "$name Stopped"
      
            Write-Host "Waiting for a while..."
            Start-Sleep -s 3
            Write-Host "Done"
        }
    
        Write-Host "Removing service $name..."
        $serviceToRemove = Get-WmiObject -Class Win32_Service -Filter "name='$name'"
        $serviceToRemove.delete()
        Write-Host "Removed"
    }
}

function Start-Service-With-Timeout([string] $name, [string] $timeout = "00:03:00")
{
    $s = Get-Service $name
    
    if ( $s.Status -eq "Stopped")
    {
        Write-Output "Starting service $name..."
        $s.Start()
    }
    
    try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
    Write-Output "$name Running"

}

$name1 = "qp8.users"
$name2 = "qp8.notification"

Stop-And-Remove-Service $name1
Stop-And-Remove-Service $name2

$projectName = "Quantumart.QP8.Scheduler.Service"

$defaultInstallRoot = "C:\QA\$projectName"
$installRoot = Read-Host "Please specify folder to install service (default - $defaultInstallRoot)"
if ([string]::IsNullOrEmpty($installRoot))
{
    $installRoot = $defaultInstallRoot
}
if (-not(Test-Path $installRoot)) { New-Item $installRoot -ItemType Directory }

$logsRoot = "C:\Logs"
if (-not(Test-Path $logsRoot)) { New-Item $logsRoot -ItemType Directory }
$logsDir = Join-Path $logsRoot $projectName
if (-not(Test-Path $logsDir)) { New-Item $logsDir -ItemType Directory }

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition


$schedulerFolder = Join-Path $currentPath "$projectName\bin\Debug"
$schedulerPath = Join-Path $schedulerFolder "$projectName.exe"
$schedulerConfigPath = Join-Path $schedulerFolder "$projectName.exe.config"
$schedulerZipPath = Join-Path $currentPath "CommonScheduler.zip"

if ((Test-Path $schedulerZipPath))
{
    Write-Host "Zip file found. Unpacking..."
    Invoke-Expression "7za.exe x -r -y -o""$installRoot"" ""$schedulerZipPath"""
}
else
{
    if (-not(Test-Path $schedulerFolder) -or -not(Test-Path $schedulerPath))
    {
        throw "You should build service $projectName in Debug configuration first";
    }
    
    [xml]$config = Get-Content -Path $schedulerConfigPath
    $add = $config.configuration.'system.diagnostics'.switches.add | where {$_.name -eq 'debug'}
    $add.value = "All"

    Copy-Item "$schedulerFolder\*" "$installRoot" -Force -Recurse
}

Write-Host "Installing service: $name"
$frameworkDir = $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory())
Invoke-Expression "$frameworkDir\installutil ""$installRoot\$projectName.exe"""
Write-Host "Installation completed: $name"

Write-Host "Waiting for a while..."
Start-Sleep -s 5
Write-Host "Done"

Set-Service $name1 -startuptype "Automatic"
Set-Service $name2 -startuptype "Automatic"

Start-Service-With-Timeout $name1
Start-Service-With-Timeout $name2