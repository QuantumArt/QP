REM Получает publishing.bak из VSS и восстанавливает базу publishing на локальном сервере

SET OUTPUTPATH=.\dbback
SET SERVER=.
IF NOT "%1" == "" (SET "SERVER=%1")
SET DBNAME=publishing
IF NOT "%2" == "" (SET "DBNAME=%2")

@echo %OUTPUTPATH%
IF NOT EXIST "%OUTPUTPATH%" mkdir "%OUTPUTPATH%"

SET SSDIR=\\storage\vss
SET PROGFPATH=
if "%programfiles(x86)%" == "" (SET "PROGFPATH=%programfiles%") ELSE (SET "PROGFPATH=%programfiles(x86)%")
SET "VS100IDE=%VS100COMNTOOLS%/../IDE"

"%PROGFPATH%\Microsoft Visual SourceSafe\ss.exe" GET "$/QPublishingASP/Installer/Files/English/Database/publishing.bak" -Gl"%OUTPUTPATH%" -W -Q
if %errorlevel% neq 0 exit /b %errorlevel%

sqlcmd -S %SERVER% -I -Q "ALTER DATABASE [%DBNAME%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; restore database %DBNAME% from DISK = '%CD%\dbback\publishing.bak';"
if %errorlevel% neq 0 exit /b %errorlevel%
sqlcmd -S %SERVER% -d %DBNAME% -I -Q "if exists (select * from sys.sysusers where name='publishing') DROP USER publishing; CREATE USER publishing FOR LOGIN publishing WITH DEFAULT_SCHEMA = publishing; EXEC sp_addrolemember 'db_owner', 'publishing';"
if %errorlevel% neq 0 exit /b %errorlevel%