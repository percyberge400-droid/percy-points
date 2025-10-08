@echo off
:: ==========================================
:: Ensure script runs as administrator
:: ==========================================
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo This script requires administrator privileges.
    echo Requesting elevation...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: ==========================================
:: Service Uninstallation Logic
:: ==========================================
SET ServiceName=POSPRAWorker

:: Check if service exists
sc query "%ServiceName%" >nul 2>&1
if %errorlevel%==0 (
    echo Stopping service "%ServiceName%" ...
    sc stop "%ServiceName%" >nul 2>&1
    timeout /t 3 /nobreak >nul

    echo Deleting service "%ServiceName%" ...
    sc delete "%ServiceName%"
    echo Service "%ServiceName%" uninstalled successfully.
) else (
    echo Service "%ServiceName%" does not exist.
)

pause
