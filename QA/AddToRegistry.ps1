param(
  [String] $configName='config.xml'
)


If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$registryPath1 = "hklm:\Software\Quantum Art"
$registryPath2 = Join-Path $registryPath1 "Q-Publishing"
$registryPath3 = "hklm:\Software\Wow6432Node\Quantum Art"
$registryPath4 = Join-Path $registryPath3 "Q-Publishing"


if (-not(Test-Path $registryPath1))
{
    New-Item -Path $registryPath1
}

if (-not(Test-Path $registryPath2))
{
    New-Item -Path $registryPath2
}

if (-not(Test-Path $registryPath3))
{
    New-Item -Path $registryPath3
}

if (-not(Test-Path $registryPath4))
{
    New-Item -Path $registryPath4
}

$configPath = Join-Path $currentPath $configName
Set-ItemProperty -path $registryPath2 -name "Configuration file" -value $configPath
Set-ItemProperty -path $registryPath4 -name "Configuration file" -value $configPath