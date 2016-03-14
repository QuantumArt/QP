@setlocal enableextensions
@cd /d "%~dp0"

%systemroot%\system32\inetsrv\APPCMD stop site /site.name:qp8

7za.exe x -r -y -oBackend Backend.zip
7za.exe x -r -y -oWinlogon Winlogon.zip

%systemroot%\system32\inetsrv\APPCMD start site /site.name:qp8