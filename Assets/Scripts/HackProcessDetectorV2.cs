
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
public static class HackProcessDetectorV2
{
    public static  List<string> BL = new List<string>
   {
        "360auto"
   };

    private const int FIRST_DELAY_MS = 1000;
    private const int INTERVAL_MS = 5000;
    public static bool isWork = true;
    public static void init()
    {
        HackDetectorCore.RegisterContext(SynchronizationContext.Current);
        new Thread(Worker) {  Name = "ProcDetectorv2" }.Start();
        HackDetectorCore.Log("[ProcDetectorv2] Thread started");
    }

    private static void Worker()
    {
        while (isWork && GameCanvas.gameStarted)
        {
            if (ScanProcesses()) { }
            Thread.Sleep(INTERVAL_MS);
        }
    }

    private static bool ScanProcesses()
    {
        try
        {
            CheckProcess();
        }
        catch { }
        finally { }
        return false;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private static readonly EnumWindowsProc _enumCb = EnumWindowsCallback;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private static readonly List<string> _windowTitles = new();

    [AOT.MonoPInvokeCallback(typeof(EnumWindowsProc))]
    private static bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
    {
        if (!IsWindowVisible(hWnd)) return true;

        var sb = new StringBuilder(256);
        if (GetWindowText(hWnd, sb, sb.Capacity) > 0)
        {
            string title = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(title))
                _windowTitles.Add(title);
        }
        return true;
    }
    public static string isDetect = "";
    public static bool CheckProcess()
    {
        try
        {
            _windowTitles.Clear();
            EnumWindows(_enumCb, IntPtr.Zero);

            foreach (string title in _windowTitles)
            {
                string lower = title.ToLower();
                foreach (string bad in BL)
                {
                    if (lower.Contains(bad))
                    {
                        isDetect = $"Phát hiện ứng dụng không hợp lệ : {bad}";
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi checkProcess: " + ex);
        }
        return false;
    }
}


#else
public static class HackProcessDetectorV2
{
 public static  List<string> BL = new List<string>
   {
        "360auto"
   };
public static bool isWork = false;
   public static void init(){}
    public static string isDetect = "";
   public static bool CheckProcess() { return false; }
}
#endif