call nuget pack edtFTPnetPRO.dll.nuspec
IF %ERRORLEVEL% NEQ 0 GOTO :FAIL
call nuget push edtFTPnet-PRO.8.6.3.nupkg
IF %ERRORLEVEL% NEQ 0 GOTO :FAIL
GOTO :SUCCESS

:FAIL
echo Release failed!
EXIT /B 1

:SUCCESS
echo Release successful!
EXIT /B 0



