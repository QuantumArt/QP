param(
  [Parameter(Mandatory = $true)]
  [String] $sqlUserId = '',

  [Parameter(Mandatory = $true)]
  [String] $sqlPassword = '',

  [Parameter(Mandatory = $true)]
  [String] $serverInstance = '',

  [Parameter(Mandatory = $true)]
  [String] $domainUserToGrant = 'ARTQ\tfsservice',

  [String] $uniqueId = '_12345',
  [String] $sourceDbName = 'qp8_test',
  [String] $destinationDbName = 'qp8_test_ci',
  [String] $backupFolderPhysicalPath = 'c:\qp\ci\',
  [String] $backupFileNamePhysicalPath = 'qp8_test_ci',
  [Bool] $isDebug = $false
)

$uniqueDestinationDbName = $destinationDbName + $uniqueId
$uniqueBakPath = $backupFolderPhysicalPath + $backupFileNamePhysicalPath + $uniqueId + '.bak'
$uniqueMdfPath = $backupFolderPhysicalPath + $backupFileNamePhysicalPath + $uniqueId + '.mdf'
$uniqueNdfPath = $backupFolderPhysicalPath + $backupFileNamePhysicalPath + $uniqueId + '.ndf'
$uniqueLdfPath = $backupFolderPhysicalPath + $backupFileNamePhysicalPath + $uniqueId + '.ldf'

$securePassword = ConvertTo-SecureString $sqlPassword -AsPlainText -Force
$sqlCredential = New-Object System.Management.Automation.PSCredential ($sqlUserId, $securePassword)

$backupScript = "
  BACKUP DATABASE [$sourceDbName] TO DISK = N'$uniqueBakPath'
  WITH FORMAT, STATS
  GO
"

$restoreScript = "
  RESTORE DATABASE [$uniqueDestinationDbName] FROM
  DISK = N'$uniqueBakPath'
  WITH REPLACE, STATS,
  MOVE N'publishing_Data' TO N'$uniqueMdfPath',
  MOVE N'ftrow_QPublishingFullTextCatalog' TO N'$uniqueNdfPath',
  MOVE N'publishing_Log' TO N'$uniqueLdfPath'
  GO
"

$accessScript = "
  USE [$uniqueDestinationDbName];
  GRANT CONNECT TO [$domainUserToGrant];
  ALTER ROLE db_owner ADD MEMBER [$domainUserToGrant];
  GO
"

$queryToRun = "
  USE master;
  IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$uniqueDestinationDbName')
    ALTER DATABASE [$uniqueDestinationDbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  GO

  $backupScript
  $restoreScript
  $accessScript

  USE master;
  ALTER DATABASE [$uniqueDestinationDbName] SET MULTI_USER WITH ROLLBACK IMMEDIATE;
  GO
"

if ($isDebug) {
  Write-Output "`r`nFinal query result: "
  Write-Output $queryToRun
}

sqlcmd -S $serverInstance -b -Q $queryToRun -U $sqlUserId -P $sqlPassword
