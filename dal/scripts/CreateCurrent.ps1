$dir = split-path -parent $MyInvocation.MyCommand.Definition

$out = Join-Path $dir "current.sql"
$list = Join-Path $dir "current.txt"

if (Test-Path $out) { Remove-Item $out -Force }

ForEach ($elem in Get-Content $list)
{
    $folderToCombine = Join-Path $dir $elem
	if (Test-Path($folderToCombine))
	{
        $script = Join-Path $dir "CombineSql.ps1"
        if (-not([string]::IsNullOrEmpty($elem)))
        {
            $command = $script + " -name ""$elem"" -outputToFile " + '$false'
            Invoke-Expression -command $command | Add-Content -Encoding UTF8 -Path $out
			Add-Content -Value "`nGO`n" -Encoding UTF8 -Path $out
        }   
	}    
}

