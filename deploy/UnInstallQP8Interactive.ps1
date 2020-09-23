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
    else {
        Write-Host "Site $name not found"
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

Import-Module WebAdministration
$name = Read-ValueOrDefault "Please enter site name to remove" "QP8"
Delete-Site $name
