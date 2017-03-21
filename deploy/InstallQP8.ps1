# restart as admin
If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "-noexit & '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

function Give-Access ([String] $name, [String] $path, [String] $permission)
{
    Write-Host "Giving '$name' '$permission' permissions to '$path'"

    $Ar = New-Object System.Security.AccessControl.FileSystemAccessRule("$name", "$permission", 'ContainerInherit,ObjectInherit', 'None', 'Allow')
    $Acl = (Get-Item $path).GetAccessControl('Access')
    $Acl.SetAccessRule($Ar)
    Set-Acl -path $path -AclObject $Acl

    Write-Host "Done"
}

Import-Module WebAdministration

$defaultName = "QP8"
$name = Read-Host "Please enter site name to install (default - $defaultName)"
if ([string]::IsNullOrEmpty($name))
{
    $name = $defaultName
}

$s = Get-Item "IIS:\sites\$name" -ErrorAction SilentlyContinue
if ($s) { throw "Site $name already exists"}

$defaultPort = "90"
$port = Read-Host "Please enter port for site (default - $defaultPort)"
$intPort = 0;
if ([string]::IsNullOrEmpty($port) -or -not([int32]::TryParse($port, [ref] $intPort)))
{
    Write-Host "Used default value for port"
    $port = $defaultPort
}

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$p = Get-Item "IIS:\AppPools\$name" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool..."

    $p = New-Item –Path "IIS:\AppPools\$name"
    $p.startMode = "AlwaysRunning"
    $p.managedRuntimeVersion = "v4.0"
    Set-Item –Path "IIS:\AppPools\$name" $p

    Write-Host "Done"
}

$defaultcurrentPath = split-path -parent $MyInvocation.MyCommand.Definition

$BackendZipPath = Join-Path $defaultcurrentPath "Backend.zip"
$WinlogonZipPath = Join-Path $defaultcurrentPath "WinLogon.zip"
$pluginsZipPath = Join-Path $defaultcurrentPath "plugins.zip"
$sitesZipPath = Join-Path $defaultcurrentPath "sites.zip"
$qaZipPath = Join-Path $defaultcurrentPath "qa.zip"

if (Test-Path($BackendZipPath))
{
    $def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
    if ($def)
    {
        $siteRoot = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
        $currentPath = Join-Path $siteRoot $name
    }
    else
    {
        $currentPath = Read-Host "Please enter path to install QP8 (default - $defaultcurrentPath)"
        if ([string]::IsNullOrEmpty($currentPath)) { $currentPath = $defaultcurrentPath }
    }
    
    if (-not(Test-Path($currentPath))) { New-Item $currentPath -ItemType Directory }
}

else
{
    $currentPath = $defaultcurrentPath
}

$rootPath = Join-Path $currentPath "sites"
$pluginsPath = Join-Path $currentPath "plugins"
$BackendPath = Join-Path $currentPath "siteMvc"
$qaPath = Join-Path $currentPath "QA"
$WinlogonPath = Join-Path $currentPath "WinLogonMvc"
$contentPath = Join-Path $BackendPath "Content"
$scriptsPath = Join-Path $BackendPath "Scripts"


if (Test-Path($BackendZipPath))
{
    Write-Host "Zip files found. Unpacking..."
    if (-not(Test-Path($BackendPath))) { New-Item $BackendPath -ItemType Directory}
    if (-not(Test-Path($WinlogonPath))) { New-Item $WinlogonPath -ItemType Directory}
    if (-not(Test-Path($pluginsPath))) { New-Item $pluginsPath -ItemType Directory}
    if (-not(Test-Path($rootPath))) { New-Item $rootPath -ItemType Directory}
    if (-not(Test-Path($qaPath))) { New-Item $qaPath -ItemType Directory}

    Invoke-Expression "7za.exe x -r -y -o""$BackendPath"" ""$BackendZipPath"""
    Invoke-Expression "7za.exe x -r -y -o""$WinlogonPath"" ""$WinlogonZipPath"""
    if ((Test-Path($pluginsZipPath))) { Invoke-Expression "7za.exe x -r -y -o""$pluginsPath"" ""$pluginsZipPath""" }
    Invoke-Expression "7za.exe x -r -y -o""$rootPath"" ""$sitesZipPath"""
    Invoke-Expression "7za.exe x -r -y -o""$qaPath"" ""$qaZipPath"""
}

Give-Access "IIS AppPool\$name" $currentPath 'ReadAndExecute'

Write-Host "Creating site, applications and virtual directories..."

$s = New-Item "IIS:\sites\$name" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $rootPath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $name

New-Item "IIS:\sites\$name\Backend" -physicalPath $backendPath -type Application
Set-ItemProperty -Path "IIS:\sites\$name\Backend" -Name applicationPool -Value $name
Set-ItemProperty -Path "IIS:\sites\$name\Backend" -Name serviceAutoStartEnabled -Value $true

New-Item "IIS:\sites\$name\Backend\WinLogon" -physicalPath $winlogonPath -type Application
Set-ItemProperty -Path "IIS:\sites\$name\Backend\WinLogon"  -Name applicationPool -Value $name
Set-ItemProperty -Path "IIS:\sites\$name\Backend\WinLogon"  -Name serviceAutoStartEnabled -Value $true

New-Item "IIS:\sites\$name\Backend\WinLogon\Content" -physicalPath $contentPath -type VirtualDirectory

New-Item "IIS:\sites\$name\plugins" -physicalPath $pluginsPath -type VirtualDirectory

New-Item "IIS:\sites\$name\Backend\WinLogon\Scripts" -physicalPath $scriptsPath -type VirtualDirectory

Write-Host "Done"


Write-Host "Unlocking configuration..."
Invoke-Expression "$env:SystemRoot\system32\inetsrv\APPCMD unlock config /section:""system.webServer/security/authentication/anonymousAuthentication""";
Invoke-Expression "$env:SystemRoot\system32\inetsrv\APPCMD unlock config /section:""system.webServer/security/authentication/windowsAuthentication""";
Write-Host "Done"


$defaultConfigDir = "C:\QA"
$configDir = Read-Host "Please enter configuration directory (default - $defaultConfigDir)"
if ([string]::IsNullOrEmpty($configDir))
{
    $configDir = $defaultConfigDir
}

Write-Host "Creating configuration directory..."

$tempDir = "C:\temp"
$logDir = "C:\logs"
$configPath = Join-Path $configDir "config.xml"
$psPath = Join-Path $configDir "AddToRegistry.ps1"
$sourceConfigPath = Join-Path $qaPath "sample_config.xml"
$sourcePsPath = Join-Path $qaPath "AddToRegistry.ps1"

if (-not(Test-Path $configDir -PathType Container))
{
    New-Item $configDir -ItemType Directory 
}

if (-not(Test-Path $configPath -PathType Leaf))
{
    Copy-Item $sourceConfigPath $configPath

    Invoke-Expression "attrib -r ""$configPath"""
}

if (-not(Test-Path $psPath -PathType Leaf))
{
    Copy-Item $sourcePsPath $psPath
    Invoke-Expression "attrib -r ""$psPath"""
}

Give-Access "IIS AppPool\$name" $configDir 'ReadAndExecute'

$sdk1path = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools"
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


$Ar = New-Object System.Security.AccessControl.FileSystemAccessRule('Everyone', 'Modify', 'ContainerInherit,ObjectInherit', 'None', 'Allow')

if (-not(Test-Path $tempDir -PathType Container))
{
    Write-Host "Creating temporary directory..."
    
    New-Item $tempDir -ItemType Directory

    Give-Access 'Everyone' $tempDir 'Modify'
}


if (-not(Test-Path $logDir -PathType Container))
{
    Write-Host "Creating directory for logs..."

    New-Item $logDir -ItemType Directory
    
    Give-Access 'Everyone' $logDir 'Modify'
}


Import-Module WebAdministration

$siteRoot = "c:\inetpub\wwwroot"
$def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
if ($def) { $siteRoot = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive }


if (Test-Path $siteRoot)
{
    Give-Access "IIS AppPool\$name" $siteRoot 'Modify'
}



