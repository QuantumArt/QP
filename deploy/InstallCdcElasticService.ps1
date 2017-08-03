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

function Install-Service([string] $name, [string] $exe)
{
  Write-Host "Installing service: $name"
  Invoke-Expression "$exe install --autostart"

  Write-Host "Installation completed: $name"
  Write-Host "Waiting for a while..."
  Start-Sleep -s 5
  Write-Host "Done"
}

$serviceName = "qp8.cdc.elastic"
Stop-And-Remove-Service $serviceName

$projectName = "Quantumart.QP8.CdcDataImport.Elastic"

$defaultInstallRoot = "C:\QA\$projectName"
$installRoot = Read-Host "Please specify folder to install service (default - $defaultInstallRoot)"
if ([string]::IsNullOrEmpty($installRoot))
{
    $installRoot = $defaultInstallRoot
}
if (-not(Test-Path $installRoot)) { New-Item $installRoot -ItemType Directory }

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$schedulerFolder = Join-Path $currentPath "..\QuantumArt.Schedulers\$projectName\bin\Debug"
$schedulerPath = Join-Path $schedulerFolder "$projectName.exe"
$schedulerConfigPath = Join-Path $schedulerFolder "$projectName.exe.config"
$schedulerZipPath = Join-Path $currentPath "CdcElasticScheduler.zip"

if ((Test-Path $schedulerZipPath))
{
    $schedulerFolder = $schedulerZipPath.Replace(".zip", "")
    Write-Host "Zip file found. Unpacking..."
    Invoke-Expression "7za.exe x -r -y -o""$schedulerFolder"" ""$schedulerZipPath"""
    $schedulerPath = Join-Path $schedulerFolder "bin\Release\$projectName.exe"
    $schedulerFolder = $schedulerFolder + "\bin\Release"
}

if (-not(Test-Path $schedulerFolder) -or -not(Test-Path $schedulerPath))
{
    throw "You should build service $projectName in Debug configuration first";
}

Copy-Item "$schedulerFolder\*" "$installRoot" -Force -Recurse
Install-Service $serviceName, ""$installRoot\$projectName.exe""
Start-Service-With-Timeout $serviceName
