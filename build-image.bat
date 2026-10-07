@echo off
echo Building Docker image doanson44/thaix:latest...

docker build -f src\ThaiX.Presentation\Dockerfile -t doanson44/thaix:latest .

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Docker build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo [SUCCESS] Docker build completed successfully.
echo.
echo If you want to push the image to Docker Hub, run:
echo docker push doanson44/thaix:latest
echo.
pause
