# restart as admin
If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "-noexit & '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
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
    $p | Set-ItemProperty -Name startMode -Value AlwaysRunning
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$rootPath = Join-Path $currentPath "sites"
$pluginsPath = Join-Path $currentPath "plugins"
$BackendPath = Join-Path $currentPath "siteMvc"
$qaPath = Join-Path $currentPath "QA"
$WinlogonPath = Join-Path $currentPath "WinLogonMvc"
$contentPath = Join-Path $BackendPath "Content"
$scriptsPath = Join-Path $BackendPath "Scripts"

$BackendZipPath = Join-Path $currentPath "Backend.zip"
$WinlogonZipPath = Join-Path $currentPath "WinLogon.zip"
$pluginsZipPath = Join-Path $currentPath "plugins.zip"
$sitesZipPath = Join-Path $currentPath "sites.zip"
$qaZipPath = Join-Path $currentPath "qa.zip"


if (Test-Path($BackendZipPath))
{
    Write-Host "Zip files found. Unpacking..."
    Invoke-Expression "7za.exe x -r -y -o""$BackendZipPath"" ""$BackendPath"""
    Invoke-Expression "7za.exe x -r -y -o""$WinlogonZipPath"" ""$WinlogonPath"""
    Invoke-Expression "7za.exe x -r -y -o""$pluginsZipPath"" ""$pluginsPath"""
    Invoke-Expression "7za.exe x -r -y -o""$sitesZipPath"" ""$rootPath"""
    Invoke-Expression "7za.exe x -r -y -o""$qaZipPath"" ""$qaPath"""
}

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
if ([string]::IsNullOrEmpty($siteRoot))
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


Write-Host "Creating temporary directory..."

if (-not(Test-Path $tempDir -PathType Container))
{
    New-Item $tempDir -ItemType Directory 
}

$Acl = (Get-Item $tempDir).GetAccessControl('Access')
$Acl.SetAccessRule($Ar)
Set-Acl -path $tempDir -AclObject $Acl

Write-Host "Done"


Write-Host "Creating directory for logs..."

if (-not(Test-Path $logDir -PathType Container))
{
    New-Item $logDir -ItemType Directory 
}

$Acl = (Get-Item $logDir).GetAccessControl('Access')
$Acl.SetAccessRule($Ar)
Set-Acl -path $logDir -AclObject $Acl

Write-Host "Done"


$defaultSiteRoot = "c:\inetpub\wwwroot"
$siteRoot = Read-Host "Please enter site root to give access (default - $defaultSiteRoot)"
if ([string]::IsNullOrEmpty($siteRoot))
{
    $siteRoot = $defaultSiteRoot
}


Write-Host "Giving 'IIS AppPool\$name' user access to '$siteRoot'"

$Ar = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$name", 'Modify', 'ContainerInherit,ObjectInherit', 'None', 'Allow')
$Acl = (Get-Item $siteRoot).GetAccessControl('Access')
$Acl.SetAccessRule($Ar)
Set-Acl -path $siteRoot -AclObject $Acl

Write-Host "Done"
