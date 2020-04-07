SET OUTPUTPATH=.\tfs
SET SERVER=.
IF NOT "%1" == "" (SET "SERVER=%1")
SET DBNAME=publishing
IF NOT "%2" == "" (SET "DBNAME=%2")

IF NOT EXIST "%OUTPUTPATH%" mkdir "%OUTPUTPATH%"


echo --- Get DB version ---
for /f %%a in ('sqlcmd -S %SERVER% -d %DBNAME% -I -Q "declare @v char(16); set @v=(select TOP 1 field_value From system_info order by dbo.qp_version_weight(field_value) desc); print @v;"') do SET LVN=%%a
if %errorlevel% neq 0 exit /b %errorlevel%

echo --- processing fix_dbo.sql ---
IF EXIST "%OUTPUTPATH%\cutted_fix_dbo.sql" DEL "%OUTPUTPATH%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%
..\..\Quantumart.QP8.CutFixDbo\bin\Debug\CutFixDbo.exe %LVN% fix_dbo.sql >> "%OUTPUTPATH%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%

echo --- executing fix_dbo.sql
sqlcmd -S %SERVER% -d %DBNAME% -I -i "%OUTPUTPATH%\cutted_fix_dbo.sql"
if %errorlevel% neq 0 exit /b %errorlevel%

