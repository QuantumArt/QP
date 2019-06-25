$pathToSourceDir = split-path -parent $MyInvocation.MyCommand.Definition

$pathToResultFileSql = Join-Path $pathToSourceDir "current.sql"
$pathToFoldersListTxt = Join-Path $pathToSourceDir "current.txt"
$pathToCombineSqlPs = Join-Path $pathToSourceDir "CombineSql.ps1"

if (Test-Path $pathToResultFileSql)
{
  Remove-Item $pathToResultFileSql -Force
}

$resultSql = ""
$sqlData = ""
ForEach ($pathToFolder in Get-Content $pathToFoldersListTxt)
{
  $folderToCombine = Join-Path $pathToSourceDir $pathToFolder
  if ((Test-Path($folderToCombine)) -and (-not([string]::IsNullOrEmpty($pathToFolder))))
  {
    $command = $pathToCombineSqlPs + " -name ""$pathToFolder"" -outputToFile " + '$false'
    Invoke-Expression -command $command | Out-String | % { $resultSql += $_ + "`r`n" }
  }
}

[IO.File]::WriteAllLines($pathToResultFileSql, $resultSql)
