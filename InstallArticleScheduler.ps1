If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$name = "ArticleSchedulerService"
$timeout = "00:03:00"

$s = Get-Service $name

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

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$projectName = "Quantumart.QP8.ArticleScheduler.WinService"

$schedulerFolder = Join-Path $currentPath "$projectName\bin\Debug"
$schedulerPath = Join-Path $schedulerFolder "$projectName.exe"

if (-not(Test-Path $schedulerFolder) -or -not(Test-Path $schedulerPath))
{
    throw "You should build service $projectName in Debug configuration first";
}

$defaultInstallRoot = "C:\QA\ArticleScheduler"
$installRoot = Read-Host "Please specify folder to install service (default - $defaultInstallRoot)"
if ([string]::IsNullOrEmpty($installRoot))
{
    $installRoot = $defaultInstallRoot
}

if (-not(Test-Path $installRoot))
{
    New-Item $installRoot -ItemType Directory
}

Copy-Item "$schedulerFolder\*" "$installRoot" -Force -Recurse

$login = "NT AUTHORITY\NETWORK SERVICE"
$password = "dummy"
$secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ($login, $secpasswd)
$description = "QP8 Article Scheduler Service"

Write-Host "Installing service: $name"
New-Service -name $name -binaryPathName "$installRoot\$projectName.exe" -Description $description -displayName $name -startupType Automatic -credential $mycreds
Write-Host "Installation completed: $name"

Write-Host "Waiting for a while..."
Start-Sleep -s 5
Write-Host "Done"

$s = Get-Service $name

if ( $s.Status -eq "Stopped")
{
    Write-Output "Starting service $name..."
    $s.Start()
}

try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Output "$name Running"