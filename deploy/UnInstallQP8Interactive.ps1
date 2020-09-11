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
}

$name = Read-ValueOrDefault "Please enter site name to remove" "QP8"
Delete-Site $name

