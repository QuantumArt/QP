echo Copy to SPBDEV02
xcopy ".\app\*.*" "\\spbdev02\c$\inetpub\wwwroot\QP8.dev\" /y /r /h /e
if %errorlevel% neq 0 exit /b %errorlevel%