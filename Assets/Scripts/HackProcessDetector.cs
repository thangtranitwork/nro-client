/* HackProcessDetector.cs  –  Process blacklist (ToolHelp32 + Hex-Base64 keywords)
 * • Biên dịch khi UNITY_EDITOR || UNITY_STANDALONE_WIN
 * • Không dùng System.Diagnostics.Process trong Player ⇒ IL2CPP an toàn
 * • Phát hiện keyword (ida, hxd64, dnspy …) trong tiêu đề cửa sổ / tên exe
 */

#if UNITY_STANDALONE_WIN
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public static class HackProcessDetector
{
    /* ---------- Blacklist keyword (HexASCII of Base64) ---------- */
    private static readonly string[] HEX_B64 = {
        /* nromessagelogger                */ "626e4a766257567a6332466e5a5778765a32646c63673d3d",
        /* cheat engine                    */ "5932686c595851675a57356e6157356c",
        /* ida / ida64                     */ "61575268","615752684e6a513d",
        /* ollydbg                         */ "62327873655752695a773d3d",
        /* x32dbg / x64dbg                 */ "65444d795a474a6e","654459305a474a6e",
        /* hxd / hxd32 / hxd64 / hxdcmp    */ "6148686b","6148686b4d7a493d","6148686b4e6a513d","6148686b59323177",
        /* hexeditor (string literal)      */ "614756345a57527064473979",
        /* process hacker                  */ "5a3268705a484a68",
        /* reclass                          */ "636d566a6247467a63773d3d",
        /* cheat-o-matic                   */ "5932686c59585174627931745958527059773d3d",
        /* dnspy                           */ "5A47357A6348486B3D",
        /* "HxD Hex Editor" full caption   */ "6148686b4947686c6543426c5a476c306233493d",
        "61577779593342775a48567463475679","6157777959334277494752316258426c63673d3d"
    };

    private static readonly HashSet<string> BL =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /* ---------- Win32 ToolHelp32 declarations ---------- */
    private const uint SNAP_PROC = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PROCESSENTRY32
    {
        public uint dwSize, cntUsage, th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID, cntThreads, th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }
    //android lam gi co kernel32.dll ma dung cai nay
    [DllImport("kernel32.dll")] private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessId);
    [DllImport("kernel32.dll")] private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
    [DllImport("kernel32.dll")] private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr hObject);

    /* ---------- Timing ---------- */
    private const int FIRST_DELAY_MS = 1000;   // 1 s
    private const int INTERVAL_MS = 1000;   // 5 s
    public static bool isWork = true;
    /* ---------- Static ctor → build keyword set & start thread ---------- */
    static HackProcessDetector()
    {

    }
    public static void init()
    {
        foreach (var hx in HEX_B64)
        {
            try
            {
                string kw = Encoding.UTF8.GetString(
                                Convert.FromBase64String(
                                    Encoding.ASCII.GetString(HexToBytes(hx))))
                            .ToLowerInvariant();
                BL.Add(kw);
            }
            catch { }
        }

        HackDetectorCore.RegisterContext(SynchronizationContext.Current);
        new Thread(Worker) {  Name = "ProcDetector" }.Start();
        HackDetectorCore.Log("[ProcDetector] Thread started");
    }

    /* ---------- Worker loop ---------- */
    private static void Worker()
    {
        //  Thread.Sleep(FIRST_DELAY_MS);
        while (isWork && GameCanvas.gameStarted)
        {
            if (ScanProcesses()) { /* Trigger done inside */ }
            Thread.Sleep(INTERVAL_MS);
        }
    }

    /* ---------- Main scan ---------- */
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
        return true; // tiếp tục liệt kê
    }
    public static bool CheckProcess()
    {
        try
        {
            _windowTitles.Clear();
            // Gọi EnumWindows – IL2CPP sẽ reverse-P/Invoke vào hàm static bên dưới
            EnumWindows(_enumCb, IntPtr.Zero);

            foreach (string title in _windowTitles)
            {
                string lower = title.ToLower();
                foreach (string bad in BL)
                {
                    if (lower.Contains(bad))
                    {
                        HackDetectorCore.TriggerDetection("Process", $"{bad}");
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

    /* ---------- utils ---------- */
    private static byte[] HexToBytes(string h)
    {
        var b = new byte[h.Length / 2];
        for (int i = 0; i < b.Length; i++)
            b[i] = Convert.ToByte(h.Substring(i * 2, 2), 16);
        return b;
    }
}
#endif

/* ---------- Stub for other platforms ---------- */
#if !UNITY_STANDALONE_WIN
public static class HackProcessDetector
{
    public static bool isWork = true;
    public static void Dummy() { }
    public static void init(){}
}
#endif
