# Push-Location $MyInvocation.MyCommand.Path
# [Environment]::CurrentDirectory = $PWD
$rootPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Push-Location ($rootPath)

$command = 'robocopy . projectstructure *.csproj *.sln /s /XD "node_modules" "bin" "obj" "wwwroot" "binaries" "projectstructure" "packages" "Content" ".git"'
Invoke-Expression $command

Add-Type -AssemblyName System.Text.Encoding
Add-Type -AssemblyName System.IO.Compression.FileSystem

class FixedEncoder : System.Text.UTF8Encoding {
  FixedEncoder() : base($true) { }

  [byte[]] GetBytes([string] $s)
  {
      $s = $s.Replace("\", "/");
      return ([System.Text.UTF8Encoding]$this).GetBytes($s);
  }
}

$destination = $rootPath + '\projectstructure.zip'
If(Test-Path $destination) {Remove-Item $destination}
$source = $rootPath + '\projectstructure'
[System.IO.Compression.ZipFile]::CreateFromDirectory($source, $destination, [System.IO.Compression.CompressionLevel]::Optimal, $false, [FixedEncoder]::new())

Remove-Item $source -Force -Recurse

Pop-Location



