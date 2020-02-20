param(
    [Parameter(Mandatory = $true)]
    [string] $agent,
    [Parameter(Mandatory = $true)]
    [string] $server,
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password,
    [Parameter()]
    [String] $project
)
$db = "qp8_test_ci_" + $agent.ToLowerInvariant()

$expr = "nunit3-console --testparam:qp8_test_ci_dbname=$db --testparam:qp8_test_ci_dbserver=$server " +
  "--testparam:qp8_test_ci_pg_login=$login --testparam:qp8_test_ci_pg_password=$password $project.dll --trace=VERBOSE"
Write-Verbose "Running tests for $project"
Invoke-Expression $expr
Write-Verbose "Done"
