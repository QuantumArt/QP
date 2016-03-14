echo Copy to BACKEND02

ncftpput.exe -u qp8deploy -p UqCDXMPm -F -R backend02.qpublishing.ru / ./app/*.*

echo Copy to BACKEND01

ncftpput.exe -u qp8deploy -p XxBfjX18 -F -R backend01.quantumart.ru /  ./app/*.*

echo Copy to SPBDEV02
xcopy ".\app\*.*" "\\spbdev02\c$\inetpub\wwwroot\QP8\" /y /r /h /e
if %errorlevel% neq 0 exit /b %errorlevel%

echo Copy to MSCDEV01
xcopy ".\app\*.*" "\\mscdev01\c$\inetpub\wwwroot\QP8\" /y /r /h /e
if %errorlevel% neq 0 exit /b %errorlevel%
