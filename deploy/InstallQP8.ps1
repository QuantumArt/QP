﻿param(
    ## QP site name
    [string] $name = 'QP8',
    ## QP site port
    [int] $port = 89,
    ## existing QP configuration  file
    [string] $configFile = '',
    ## QP configuration service URL
    [string] $configServiceUrl = '',
    ## QP configuration service token
    [string] $configServiceToken = '',
    ## QP configuration directory
    [string] $configDir = 'C:\QA',
    ## QP directory for temporary files
    [string] $tempDir = 'C:\Temp',
    ## QP directory for logs
    [string] $logDir = 'C:\Logs',
    ## Use (or not) windows authentication
    [bool] $useWinAuth = $false,
    ## Use (or not) article scheduler
    [bool] $enableArticleScheduler = $false,
     ## Use (or not) common scheduler
    [bool] $enableCommonScheduler = $false
)

function Give-Access ([String] $name, [String] $path, [String] $permission)
{
    Write-Host "Giving '$name' '$permission' permissions to '$path'"

    $Ar = New-Object System.Security.AccessControl.FileSystemAccessRule("$name", "$permission", 'ContainerInherit,ObjectInherit', 'None', 'Allow')
    $Acl = (Get-Item $path).GetAccessControl('Access')
    $Acl.SetAccessRule($Ar)
    Set-Acl -path $path -AclObject $Acl

    Write-Host "Done"
}

if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

$s = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue
if ($s) { throw "Site $name already exists"}

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$connected = $false
Try { $connected = (New-Object System.Net.Sockets.TcpClient('localhost', $port)).Connected } Catch { }
If ($connected) { throw "$port is busy"}

$p = Get-Item "IIS:\AppPools\$name" -ErrorAction SilentlyContinue

if (!$p) {

    Write-Host "Creating application pool $name..."

    $p = New-Item –Path "IIS:\AppPools\$name"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value "v4.0"
    $p | Set-ItemProperty -Name startMode -Value "AlwaysRunning"
    $p | Set-ItemProperty -Name processModel.idleTimeout -value ( [TimeSpan]::FromMinutes(0))

    Write-Host "Done"
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$sourcePath = Split-Path -parent $currentPath
$backendPath = Join-Path $sourcePath "backend"
$qaPath = Join-Path $currentPath "QA"

$def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
if (!$def) {
    throw "Default Web site not found"
}
$siteRoot = $def.PhysicalPath -replace "%SystemDrive%", $env:SystemDrive
$qp8Path = Join-Path $siteRoot $name

Write-Host "Creating site folder for $name..."
New-Item -Path $qp8Path -ItemType Directory -Force | Out-Null
Copy-Item "$backendPath\*" -Destination $qp8Path -Force -Recurse
Write-Host "Done"

Write-Host "Unlocking configuration..."
Invoke-Expression "$env:SystemRoot\system32\inetsrv\APPCMD unlock config /section:""system.webServer/security/authentication/anonymousAuthentication""";
Invoke-Expression "$env:SystemRoot\system32\inetsrv\APPCMD unlock config /section:""system.webServer/security/authentication/windowsAuthentication""";
Write-Host "Done"


if (!$configServiceUrl -and !$configServiceToken -and !$configFile) {
    Write-Host "Creating configuration directory..."

    if (-not(Test-Path $configDir -PathType Container))
    {
        New-Item $configDir -ItemType Directory | Out-Null
    }

    Copy-Item "$qaPath\*" -Destination $configDir -Force
    $qaConfig = Join-Path $configDir "config.xml"
    if (-not(Test-Path($qaConfig))) {
        Rename-Item -Path (Join-Path $configDir "sample_config.xml") -NewName "config.xml"
    }
    Set-ItemProperty $qaConfig -name IsReadOnly -value $false
    Give-Access "IIS AppPool\$name" $configDir 'ReadAndExecute'

    $sdk1path = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.1 Tools"
    $sdk2path = "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools"
    $sdk3path = "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools"

    if (Test-Path $sdk1path -PathType Container)
    {
        Copy-Item "$sdk1path\sqlmetal.*" $configDir
    }
    elseif (Test-Path $sdk2path -PathType Container)
    {
        Copy-Item "$sdk2path\sqlmetal.*" $configDir
    }
    elseif (Test-Path $sdk3path -PathType Container)
    {
        Copy-Item "$sdk3path\sqlmetal.*" $configDir
    }
    Write-Host "Done"
}


if (-not(Test-Path $tempDir -PathType Container))
{
    Write-Host "Creating temporary directory $tempDir..."
    New-Item $tempDir -ItemType Directory | Out-Null
    Give-Access 'Everyone' $tempDir 'Modify'
    Write-Host "Done"
}

if (-not(Test-Path $logDir -PathType Container))
{
    Write-Host "Creating directory for logs $logDir..."
    New-Item $logDir -ItemType Directory
    Give-Access 'Everyone' $logDir 'Modify'
    Write-Host "Done"
}


$nLogPath = Join-Path $qp8Path "NLog.config"
$nlog = [xml](Get-Content -Path $nLogPath)

$nlog.nlog.internalLogFile = [string](Join-Path $logDir "internal-log.txt")
($nlog.nlog.variable | Where-Object {$_.name -eq 'defaultLogDir'}).value = [string](Join-Path $logDir $name)
($nlog.nlog.rules.logger | Where-Object {$_.name -eq 'Quantumart.QP8.BLL.Services.MultistepActions.Import*'}).writeTo = "csvImport"
($nlog.nlog.rules.logger | Where-Object {$_.name -eq '*'}).writeTo = "default"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)


$appSettingsPath = Join-Path $qp8Path "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json
$properties = $json.Properties

if ($configServiceUrl -and $configServiceToken) {
    $properties | Add-Member NoteProperty "QpConfigUrl" $configServiceUrl -Force
    $properties | Add-Member NoteProperty "QpConfigToken" $configServiceToken -Force
}
else {
    $properties | Add-Member NoteProperty "QpConfigPath" $qaConfig -Force
}

if ($enableArticleScheduler) {
    $properties | Add-Member NoteProperty "EnableArticleScheduler" $true -Force
}

if ($enableCommonScheduler) {
    $properties | Add-Member NoteProperty "EnableCommonScheduler" $true -Force
}

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath


Write-Host "Creating site $name..."
$s = New-Item "IIS:\sites\$name" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $qp8Path
$s | Set-ItemProperty -Name applicationPool -Value $name
$s | Set-ItemProperty -Name applicationDefaults.preloadEnabled -Value True
if ($useWinAuth) {
    Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" -Name Enabled -Value $false -PSPath "IIS:\Sites\$name"
}
Write-Host "Done"

Write-Host "Giving $name pool user access to $siteRoot..."
if ($siteRoot -and (Test-Path $siteRoot))
{
    Give-Access "IIS AppPool\$name" $siteRoot 'Modify'
}
Write-Host "Done"
