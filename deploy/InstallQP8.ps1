param(
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
    ## Make this QP instance global or not
    [bool] $makeGlobal = $true,
    ## Use (or not) windows authentication
    [bool] $useWinAuth = $false,
    ## Use (or not) article scheduler
    [bool] $enableArticleScheduler = $false,
    ## Use (or not) common scheduler
    [bool] $enableCommonScheduler = $false,
    ## Clean old installation
    [bool] $cleanOld = $true
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

function Register-Global([string] $configPath) {
    $path1 = "hklm:\Software\Quantum Art\Q-Publishing"
    $path2 = "hklm:\Software\Wow6432Node\Quantum Art\Q-Publishing"
    if (-not(Test-Path $path1)) { New-Item -Path $path1 -Force | Out-Null }
    if (-not(Test-Path $path2)) { New-Item -Path $path2 -Force | Out-Null }

    $prop1 = Get-ItemProperty -path $path1 -name "Configuration file" -ErrorAction SilentlyContinue
    $prop2 = Get-ItemProperty -path $path2 -name "Configuration file" -ErrorAction SilentlyContinue
    $prop = if ($prop1) { $prop1 } else { $prop2 }
    $oldConfigPath = $prop.'Configuration file'
    $oldConfigPath
    Test-Path $oldConfigPath
    Test-Path $configPath
    if ($oldConfigPath -and (Test-Path $oldConfigPath) -and -not(Test-Path $configPath)) {
        Write-Host "Copying old configuration file $oldConfigPath..."
        Copy-Item $oldConfigPath $configPath
        Write-Host "Done"
    }

    Set-ItemProperty -path $path1 -name "Configuration file" -value $configPath
    Set-ItemProperty -path $path2 -name "Configuration file" -value $configPath
}

function Delete-Site([string] $name)
{
    $app = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue

    if ($app) {
        $path =  $app.PhysicalPath
        $poolName = $app.applicationPool

        if ($poolName) {
            Stop-AppPool $poolName | Out-Null
            Remove-Item "IIS:\AppPools\$poolName" -Recurse -Force
            Write-Host "pool $poolName deleted"
        }

        Remove-Item "IIS:\sites\$name" -Recurse -Force
        Write-Host "Site $name deleted"

        if (Test-Path $path){
            Remove-Item $path -Recurse -Force
            Write-Host "files of site $name deleted"
        }
    }
}

function Stop-AppPool([string] $name)
{
    $s = Get-Item "IIS:\AppPools\$name" -ErrorAction SilentlyContinue

    if ($s -and $s.State -ne "Stopped")
    {
        Write-Verbose "Stopping AppPool $name..." -Verbose
        $s.Stop()
        $endTime = $(get-date).AddMinutes('1')
        while($(get-date) -lt $endtime)
        {
            Start-Sleep -Seconds 1
            if ($s.State -ne "Stopping")
            {
                if ($s.State -eq "Stopped") {
                    Write-Verbose "Stopped" -Verbose
                }
                break
            }
        }
    }

    return $s.State -eq "Stopped"
}


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

if ($cleanOld) {
    Delete-Site $name
}
else {
    $s = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue
    if ($s) { throw "Site $name already exists"}
}

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$connected = $false
Try { $connected = (New-Object System.Net.Sockets.TcpClient('localhost', $port)).Connected } Catch { }
If ($connected) { throw "$port is busy"}

$requiredRuntime = "3.1.[456]"
$runtimes = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.NETCore.App')).Name
if (-not([bool]($runtimes -match $requiredRuntime))) {
    throw ".Net Core Runtime (min 3.1.4) is not found"
}

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

if (!$configServiceUrl -and !$configServiceToken) {
    if ($configFile -and (Test-Path $configFile)) {
        $qaConfig = $configFile
    }
    else {

        Write-Host "Setting up configuration directory $configDir..."

        if (-not(Test-Path $configDir -PathType Container))
        {
            New-Item $configDir -ItemType Directory | Out-Null
        }

        Copy-Item "$qaPath\*" -Destination $configDir -Force
        $qaConfig = Join-Path $configDir "config.xml"

        if ($makeGlobal) {
            Register-Global $qaConfig
        }

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

if ($siteRoot -and (Test-Path $siteRoot))
{
    Give-Access "IIS AppPool\$name" $siteRoot 'Modify'
}
