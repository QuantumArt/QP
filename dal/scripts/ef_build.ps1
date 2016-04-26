$dir = Convert-Path .
$in = Join-Path $dir "\EFTablesViews\*.sql"
$out = Join-Path $dir "\EFTablesViews.sql"
Get-ChildItem $in | Sort Name | Get-Content | Set-Content -Encoding UTF8 -Path $out