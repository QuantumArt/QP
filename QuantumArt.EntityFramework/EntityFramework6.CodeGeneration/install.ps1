param($installPath, $toolsPath, $package, $project)

foreach ($item in $project.ProjectItems["QPDataContextInclude"].ProjectItems)
{
	$item.Properties.Item("BuildAction").Value = [int]0
	Write-Host "Set"$item.Name"Build Action to None"
}