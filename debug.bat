@echo off
setlocal
cd /d "%~dp0"

echo ===================================================
echo [1/3] Restoring client libraries (LibMan)...
echo ===================================================
cd /d "%~dp0src\ThaiX.Client"
call dotnet tool restore
if %errorlevel% neq 0 (
    echo [ERROR] Failed to restore dotnet tools in ThaiX.Client.
    pause
    exit /b %errorlevel%
)
call dotnet libman restore
if %errorlevel% neq 0 (
    echo [ERROR] Failed to restore client libraries via LibMan.
    pause
    exit /b %errorlevel%
)

echo.
echo ===================================================
echo [2/3] Building ThaiX.Presentation and dependencies...
echo ===================================================
cd /d "%~dp0"
call dotnet build src\ThaiX.Presentation\ThaiX.Presentation.csproj -c Debug
if %errorlevel% neq 0 (
    echo [ERROR] Build failed. Please fix compilation errors before running watch.
    pause
    exit /b %errorlevel%
)

echo.
echo ===================================================
echo [3/3] Starting watch mode and launching browser...
echo URL: https://localhost:7129 (HTTP: http://localhost:5005)
echo ===================================================
echo.

set ASPNETCORE_ENVIRONMENT=Development

REM Launch browser in background after short delay to allow server startup
start "" cmd /c "ping 127.0.0.1 -n 4 >nul & start https://localhost:7129"

cd /d "%~dp0src\ThaiX.Presentation"
set ARGS=%*
if "%ARGS%"=="" set ARGS=--launch-profile https
dotnet watch run %ARGS%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] dotnet watch exited with code %errorlevel%.
    pause
    exit /b %errorlevel%
)
