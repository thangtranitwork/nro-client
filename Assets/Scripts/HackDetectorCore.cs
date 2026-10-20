#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

internal static class HackDetectorCore
{
    /* paths & log */
    private static readonly string LOG_PATH =
        Path.Combine(Application.persistentDataPath, "logInject.txt");
    internal static void Log(string m)
    {
        try { File.AppendAllText(LOG_PATH, $"{DateTime.Now:s} {m}{Environment.NewLine}"); } catch { }
        Debug.LogError(m);
    }

    /* trusted vendors */
    internal static readonly string[] TRUSTED_VENDOR =
        { "microsoft","nvidia","advanced micro devices","amd","intel","unity technologies" };

    /* helper caches */
    private static readonly Dictionary<string, bool> _sigCache =
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    internal static bool IsTrustedSignedCached(string path)
    {
        bool ok; if (_sigCache.TryGetValue(path, out ok)) return ok;
        ok = false;
        try
        {
            var c = X509Certificate.CreateFromSignedFile(path);
            if (c != null)
            {
                var x = new X509Certificate2(c);
                string i = x.Issuer.ToLower(), s = x.Subject.ToLower();
                foreach (var v in TRUSTED_VENDOR)
                    if (i.Contains(v) || s.Contains(v)) { ok = true; break; }
            }
        }
        catch { }
        _sigCache[path] = ok; return ok;
    }

    /* flag & quit */
    private static volatile bool _reported;
    private static SynchronizationContext _ctx;
    internal static void RegisterContext(SynchronizationContext c) => _ctx ??= c;

    internal static async void TriggerDetection(string who, string detail)
    {
        if (_reported) return;
        _reported = true;
        HackDetectorCore.Log($"[!!!] {who} HACK: {detail}");

        async void QuitAsync()
        {
            if (who != "Process")
            {
                try
                {
                    Service.gI().detectHacking(who, detail);
                    await System.Threading.Tasks.Task.Delay(200);// nhưng tối đa 3 s
                }
                catch { /* swallow */ }
            }
            Session_ME.gI().close();
            Session_ME2.gI().close();
            Application.Quit();
        }

        if (_ctx != null) _ctx.Post(_ => QuitAsync(), null);
        else QuitAsync();
    }

    /* whitelist  – c?p nh?t b?i server */
    internal static readonly HashSet<string> White =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static void UpdateWhiteList(IEnumerable<string> names)
    {
        White.Clear();
        foreach (var n in names)
            if (!string.IsNullOrWhiteSpace(n))
                White.Add(n.Trim().ToLowerInvariant());
        Log($"[Whitelist] Server update: {White.Count} DLL");
    }
}
#endif
