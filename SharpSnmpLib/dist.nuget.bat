del SharpSnmpLib\bin\Release\*.nupkg
del SharpSnmpLib\bin\Release\*.snupkg
del *.nupkg
del *.snupkg
call build.release.bat
IF %ERRORLEVEL% NEQ 0 exit /b 1
call sign3.bat
IF %ERRORLEVEL% NEQ 0 exit /b 1
copy SharpSnmpLib\bin\Release\*.nupkg .
copy SharpSnmpLib\bin\Release\*.snupkg .

powershell -ExecutionPolicy Bypass -file sign.nuget.ps1
@IF %ERRORLEVEL% NEQ 0 exit /b 1
