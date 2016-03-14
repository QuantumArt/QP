echo Copy to MSCDEV01
xcopy ".\app\*.*" "\\mscdev01\c$\inetpub\wwwroot\QP8.dev\" /y /r /h /e
if %errorlevel% neq 0 exit /b %errorlevel%