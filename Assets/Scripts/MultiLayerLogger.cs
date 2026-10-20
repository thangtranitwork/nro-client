using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class MultiLayerLogger
{
    public static bool EnableLogging = true;

    private static readonly object fileLock = new object();
    private static string logPath;

    static MultiLayerLogger()
    {
        try
        {
            string dir = Application.persistentDataPath;
            logPath = "MultiLayerLog.txt";
        }
        catch
        {
            logPath = "MultiLayerLog.txt";
        }
    }

    public static void Log(string message)
    {
        if (!EnableLogging) return;
        try
        {
            string line = $"[{DateTime.UtcNow:O}] {message}";
            lock (fileLock)
            {
                File.AppendAllText(logPath, line + "\n", Encoding.UTF8);
            }
        }
        catch { }
    }
}


