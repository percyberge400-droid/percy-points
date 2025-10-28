@echo off
setlocal enabledelayedexpansion

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
:: Step 1 — Call UpdateConfigurationFlag(false)
:: ==========================================
set "ServiceExe=%~dp0POSPRAWorker.exe"
if exist "%ServiceExe%" (
    echo Updating configuration flag to false...
    "%ServiceExe%" update-config-flag false
    if %errorlevel% neq 0 (
        echo [WARNING] UpdateConfigurationFlag(false) returned a non-zero code.
    )
) else (
    echo [WARNING] POSPRAWorker.exe not found at "%ServiceExe%".
)

:: ==========================================
:: Step 2 — Uninstall the service
:: ==========================================
set "ServiceName=POSPRAWorker"

echo Checking if service "%ServiceName%" exists...
sc query "%ServiceName%" >nul 2>&1
if %errorlevel%==0 (
    echo Stopping service "%ServiceName%" ...
    sc stop "%ServiceName%" >nul 2>&1
    timeout /t 3 /nobreak >nul

    echo Deleting service "%ServiceName%" ...
    sc delete "%ServiceName%" >nul 2>&1
    echo Service "%ServiceName%" uninstalled successfully.
) else (
    echo Service "%ServiceName%" does not exist.
)

exit /b 0
