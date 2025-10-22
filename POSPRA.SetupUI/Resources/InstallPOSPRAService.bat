@echo off
SET ServiceName=POSPRAWorker
SET ExePath=%~dp0POSPRA.Worker.exe

:: ==========================================
:: Ensure script runs as Administrator
:: ==========================================
net session >nul 2>&1
if %errorlevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs -WindowStyle Hidden"
    exit /b
)

:: ==========================================
:: Check if service exists
:: ==========================================
sc query "%ServiceName%" >nul 2>&1
if %errorlevel%==0 (
    :: Service exists — silently restart
    sc stop "%ServiceName%" >nul 2>&1
    timeout /t 2 >nul
    sc start "%ServiceName%" >nul 2>&1
) else (
    :: Install and start new service
    sc create "%ServiceName%" binPath= "\"%ExePath%\"" start= auto DisplayName= "POSPRA Worker Service" >nul 2>&1
    sc description "%ServiceName%" "POSPRA Worker Service" >nul 2>&1
    sc start "%ServiceName%" >nul 2>&1
)

exit
