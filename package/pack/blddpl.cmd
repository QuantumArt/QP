call mkpkg.cmd
if %errorlevel% neq 0 exit /b %errorlevel%

cd .\qp8release
call updatedb.cmd sqlcluster03.qpublishing.ru qp8_publishing
if %errorlevel% neq 0 exit /b %errorlevel%

call updatedb.cmd sqlcluster03.qpublishing.ru qp8_mts_main
if %errorlevel% neq 0 exit /b %errorlevel%

call cpysites.cmd
if %errorlevel% neq 0 exit /b %errorlevel%

RemoteBatchClient.exe http://updateadmin8.qpublishing.ru

cd ..\