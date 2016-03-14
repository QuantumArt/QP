set argC=0
for %%x in (%*) do Set /A argC+=1
if "%argC%" == "0" goto main
if "%argC%" == "1" goto main
if not "%argC%" == "2" goto usage


:main

SET "OUTPUTPATH=.\qp8release"
IF NOT "%1" == "" (SET "OUTPUTPATH=%1")

SET GETDBBAK=""
IF NOT "%2" == "" (SET "GETDBBAK=%2")

rem Creating Folders

IF EXIST "%OUTPUTPATH%" rd "%OUTPUTPATH%" /s /q
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir "%OUTPUTPATH%"
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir "%OUTPUTPATH%\database"
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir "%OUTPUTPATH%\temp"
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir "%OUTPUTPATH%\app"
if %errorlevel% neq 0 exit /b %errorlevel%

rem SET SSDIR=\\storage\vss
SET PROGFPATH=
if "%programfiles(x86)%" == "" (SET "PROGFPATH=%programfiles%") ELSE (SET "PROGFPATH=%programfiles(x86)%")
SET "VS110IDE=%VS110COMNTOOLS%\..\IDE"

rem getting latest version from  TFS
tfsexport.exe /collection:http://tfs:8080/tfs/quantumartcollection /serverpath:"$/QP8/Maintenance/Releases/Release 6.0" /localpath:"%OUTPUTPATH%\temp" /removebindings
if %errorlevel% neq 0 exit /b %errorlevel%

rem preparing DB update scripts
xcopy "%OUTPUTPATH%\temp\dal\scripts\fix_dbo.sql" "%OUTPUTPATH%\database" /I /E /Y
if %errorlevel% neq 0 exit /b %errorlevel%

rem Compilation
call "%PROGFPATH%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
_nuget.exe restore "%OUTPUTPATH%\temp\QP8.sln"
call "%PROGFPATH%\MSBuild\14.0\Bin\msbuild.exe" /property:Configuration=Release;BuildWithScript=true "%OUTPUTPATH%\temp\QP8.sln"
if %errorlevel% neq 0 exit /b %errorlevel%

rem Release Transformation
attrib -R "%OUTPUTPATH%\temp\siteMvc\Web.config"
ctt.exe s:"%OUTPUTPATH%\temp\siteMvc\Web.config" t:"%OUTPUTPATH%\temp\siteMvc\Web.Release.config" d:"%OUTPUTPATH%\temp\siteMvc\Web.config"

rem Remove unnecessary files
del /F /S /Q "%OUTPUTPATH%\temp\*.cs"
del /F /S /Q "%OUTPUTPATH%\temp\*.resx"

rem archiving
7za.exe a -r -y "%OUTPUTPATH%\app\Backend.zip" "%OUTPUTPATH%\temp\siteMvc\*.*"
if %errorlevel% neq 0 exit /b %errorlevel%

7za.exe a -r -y "%OUTPUTPATH%\app\Winlogon.zip" "%OUTPUTPATH%\temp\winLogonMvc\*.*"
if %errorlevel% neq 0 exit /b %errorlevel%

7za.exe a -r -y "%OUTPUTPATH%\app\ArticleScheduler.zip" "%OUTPUTPATH%\temp\Quantumart.QP8.ArticleScheduler.WinService\bin\Release\*.*"
if %errorlevel% neq 0 exit /b %errorlevel%

rem copping
xcopy "%OUTPUTPATH%\temp\package\pack\7za.exe" "%OUTPUTPATH%\app" /Y
if %errorlevel% neq 0 exit /b %errorlevel%

xcopy "%OUTPUTPATH%\temp\package\unpack\deploy.cmd" "%OUTPUTPATH%\app" /Y
if %errorlevel% neq 0 exit /b %errorlevel%

xcopy "%OUTPUTPATH%\temp\package\unpack\*.*" "%OUTPUTPATH%" /Y
if %errorlevel% neq 0 exit /b %errorlevel%

xcopy "%OUTPUTPATH%\temp\Quantumart.QP8.CutFixDbo\bin\Release\CutFixDbo.exe" "%OUTPUTPATH%" /Y
if %errorlevel% neq 0 exit /b %errorlevel%

xcopy "%OUTPUTPATH%\temp\Quantumart.QP8.Deploy.RemoteBatch.Client\bin\Release\RemoteBatchClient.exe" "%OUTPUTPATH%" /Y
if %errorlevel% neq 0 exit /b %errorlevel%

rem removing temporaries
IF EXIST "%OUTPUTPATH%\temp" RMDIR "%OUTPUTPATH%\temp" /S /Q

goto :eof

:usage
echo Error in script usage. The correct usage is:
echo     %0 [output dir path] [get db back file flag]
echo For example:
echo     %0 c:\qp8\release
goto :eof
