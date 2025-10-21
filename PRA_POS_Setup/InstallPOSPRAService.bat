@echo off
SET ServiceName=POSPRAWorker
SET ExePath=%~dp0POSPRA.Worker.exe

:: Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: Check if service exists
sc query "%ServiceName%" >nul 2>&1
if %errorlevel%==0 (
    echo Service "%ServiceName%" already exists.
    sc stop "%ServiceName%" >nul 2>&1
    timeout /t 2 >nul
    sc start "%ServiceName%" >nul 2>&1
) else (
    echo Installing %ServiceName% ...
    sc create "%ServiceName%" binPath= "\"%ExePath%\"" start= auto DisplayName= "POSPRA Worker Service"
    sc description "%ServiceName%" "POSPRA Worker Service"
    sc start "%ServiceName%"
    echo Service installed and started successfully.
)
exit
