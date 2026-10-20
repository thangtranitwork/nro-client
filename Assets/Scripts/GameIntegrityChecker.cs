using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class GameIntegrityChecker
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private static readonly HashSet<string> validDLLs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "baselib.dll",
        "GameAssembly.dll",
        "UnityPlayer.dll",
        "AccountManager.dll"

    };

    private static readonly HashSet<string> validFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "SieuSaoNgocRong.exe",
        "UnityCrashHandler32.exe",
        "AccountManager.exe"
        
    };

    private static bool enable;
    private static long lastCheck = 0;
    private static readonly long checkInterval = 5000;
    private static bool isChecking = false;
    private static string gameDirectory = "";
    private static CancellationTokenSource cancellationTokenSource;

    public static string isDetect = "";

    public static void StartChecking()
    {
        enable = true;
        lastCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        isDetect = "";
        isChecking = false;

        // Tạo mới CancellationTokenSource
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        // Cache game directory trên main thread
        gameDirectory = Application.dataPath + "/../";
    }

    public static void StopChecking()
    {
        enable = false;

        // Hủy tất cả các task đang chạy
        cancellationTokenSource?.Cancel();
    }

    public static void Update()
    {
        if (!enable || isChecking) return;

        // Kiểm tra ngay lập tức nếu gameStarted = false
        if (!GameCanvas.gameStarted)
        {
            StopChecking();
            return;
        }

        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (currentTime - lastCheck >= checkInterval)
        {
           // Debug.LogError("update");
            _ = CheckAsync();
            lastCheck = currentTime;
        }
    }

    private static async System.Threading.Tasks.Task CheckAsync()
    {
        if (isChecking || cancellationTokenSource == null) return;

        isChecking = true;

        try
        {
            await System.Threading.Tasks.Task.Run(() => {
                CheckFiles(cancellationTokenSource.Token);
            }, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Task đã bị hủy, không làm gì cả
            Debug.Log("Game integrity check was cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in CheckAsync: {ex}");
        }
        finally
        {
            isChecking = false;
        }
    }

    private static void CheckFiles(CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra cancellation token trước khi bắt đầu
            cancellationToken.ThrowIfCancellationRequested();

            // Sử dụng cached directory thay vì Application.dataPath
            if (string.IsNullOrEmpty(gameDirectory))
            {
                return; // Không có directory để check
            }

            // Kiểm tra lại gameStarted trước khi thực hiện công việc nặng
            if (!GameCanvas.gameStarted)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            string[] files = Directory.GetFiles(gameDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (string filePath in files)
            {
                // Kiểm tra cancellation token trong vòng lặp
                cancellationToken.ThrowIfCancellationRequested();

                // Kiểm tra lại gameStarted trong vòng lặp
                if (!GameCanvas.gameStarted)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(fileName).ToLower();

                if (extension == ".dll" && !validDLLs.Contains(fileName))
                {
                    HandleThreat(fileName);
                    return;
                }

                if (extension == ".exe" && !validFiles.Contains(fileName))
                {
                    HandleThreat(fileName);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Task đã bị hủy, không làm gì cả
            throw; // Re-throw để được xử lý ở CheckAsync
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in CheckFiles: {ex}");
        }
    }

    private static void HandleThreat(string fileName)
    {
        isDetect = $"Phát hiện có file không hợp lệ: {fileName}";
        enable = false;

        // Dừng tất cả checking khi phát hiện threat
        StopChecking();
    }

    public static bool HasThreat()
    {
        return !string.IsNullOrEmpty(isDetect);
    }

    public static void ClearThreat()
    {
        isDetect = "";
    }

    // Cleanup method để gọi khi game shutdown
    public static void Cleanup()
    {
        StopChecking();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

#else
    public static string isDetect = "";
    
    public static void StartChecking() { }
    public static void StopChecking() { }
    public static void Update() { }
    public static bool HasThreat() { return false; }
    public static void ClearThreat() { }
    public static void Cleanup() { }
#endif
}