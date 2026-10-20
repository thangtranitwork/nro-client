@echo off
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"
set PROJECT_PATH=%~dp0
set OUTPUT_LOG=%~dp0build_ios_log.txt

echo [AutoBuild] Starting iOS Xcode Build...
%UNITY_PATH% -quit -batchmode -projectPath "%PROJECT_PATH:~0,-1%" -executeMethod AutoBuilder.BuildIOS -logFile "%OUTPUT_LOG%"

echo [AutoBuild] iOS Build Process Completed!
echo Check build_ios_log.txt for detailed logs.
pause
