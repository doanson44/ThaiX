@echo off
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File ".github/skills/publish-code/publish.ps1" -Mode Package -Runtime win-x86 -SelfContained true
pause
