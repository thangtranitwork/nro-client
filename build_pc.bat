@echo off
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"
set PROJECT_PATH=%~dp0
set OUTPUT_LOG=%~dp0build_pc_log.txt

echo [AutoBuild] Starting Windows PC Build...
%UNITY_PATH% -quit -batchmode -projectPath "%PROJECT_PATH:~0,-1%" -executeMethod AutoBuilder.BuildWindows -logFile "%OUTPUT_LOG%"

echo [AutoBuild] Windows Build Process Completed!
echo Check build_pc_log.txt for detailed logs.
pause
