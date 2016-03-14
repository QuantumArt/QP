set argC=0
for %%x in (%*) do Set /A argC+=1
if "%argC%" == "0" goto main
if "%argC%" == "1" goto main
if "%argC%" == "2" goto main
if "%argC%" == "3" goto main
if "%argC%" == "4" goto main
if not "%argC%" == "5" goto usage

:main
SET SERVER=sqlcluster03.qpublishing.ru
IF NOT "%1" == "" (SET "SERVER=%1")

SET DBNAME=qp8_publishing
IF NOT "%2" == "" (SET "DBNAME=%2")

SET FILEDIR=%CD%\database
IF NOT "%3" == "" (SET "FILEDIR=%3")

SET BAKFILE=publishing.bak
IF NOT "%4" == "" (SET "BAKFILE=%4")

SET RESTOREDB=""
IF NOT "%5" == "" (SET "RESTOREDB=%4")

SET SQLUSR=qp8deploy
SET SQLPWD=U68quhmA



if %RESTOREDB% == "" goto l1
echo --- Restore DB ---
sqlcmd -U %SQLUSR% -P %SQLPWD% -S %SERVER% -I -Q "ALTER DATABASE [%DBNAME%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; restore database %DBNAME% from DISK = '%FILEDIR%\%BAKFILE%';"
if %errorlevel% neq 0 exit /b %errorlevel%
sqlcmd -U %SQLUSR% -P %SQLPWD% -S %SERVER% -d %DBNAME% -I -Q "if exists (select * from sys.sysusers where name='publishing') DROP USER publishing; CREATE USER publishing FOR LOGIN publishing WITH DEFAULT_SCHEMA = publishing; EXEC sp_addrolemember 'db_owner', 'publishing';"
if %errorlevel% neq 0 exit /b %errorlevel%

:l1
echo --- Get DB version ---
for /f %%a in ('sqlcmd -U %SQLUSR% -P %SQLPWD% -S %SERVER% -d %DBNAME% -I -Q "declare @v char(16); set @v=(select TOP 1 field_value From system_info order by dbo.qp_version_weight(field_value) desc); print @v;"') do SET LVN=%%a
if %errorlevel% neq 0 exit /b %errorlevel%

echo --- processing fix_dbo.sql ---
IF EXIST %FILEDIR%\cutted_fix_dbo.sql DEL "%FILEDIR%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%
CutFixDbo.exe %LVN% "%FILEDIR%\fix_dbo.sql" >> "%FILEDIR%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%

echo --- executing fix_dbo.sql
sqlcmd -U %SQLUSR% -P %SQLPWD% -S %SERVER% -d %DBNAME% -I -i "%FILEDIR%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%

echo --- Remove temporary
IF EXIST %FILEDIR%\cutted_fix_dbo.sql DEL "%FILEDIR%\cutted_fix_dbo.sql"

goto :eof

:usage
echo Error in script usage. The correct usage is:
echo     %0 [server] [dn name] [input file dir path] [input file name] [restore db flag]
echo For example:
echo     %0 localhost\sql2008R2 publishing c:\qp8\release\database publishing.bak false
goto :eof