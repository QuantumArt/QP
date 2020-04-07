param(
    [String] $name='',
    [Bool] $outputToFile=$true
)

$dir = split-path -parent $MyInvocation.MyCommand.Definition

if ([string]::IsNullOrEmpty($name))
{
    $name = Read-Host "Please enter folder name with scripts to combine"
}

if (-not([string]::IsNullOrEmpty($name)))
{
  $path = Join-Path $dir $name
  if (Test-Path($path))
  {
    $in = Join-Path $path "*.sql"
    $out = Join-Path $dir "$name.sql"
    if ($outputToFile)
    {
      Get-ChildItem $in | Sort Name | Get-Content -Encoding UTF8 | Set-Content -Encoding UTF8 -Path $out
    }
    else
    {
      Get-ChildItem $in | Sort Name | Get-Content -Encoding UTF8 | Write-Output
    }
  }
  else
  {
    throw "Folder $name not found"
  }
}
else
{
  throw "folder name is empty"
}
