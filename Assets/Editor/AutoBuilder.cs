using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;

public class AutoBuilder
{
    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }

    [MenuItem("Build/Auto Build Windows")]
    public static void BuildWindows()
    {
        string buildPath = "Builds/Windows/HUNR_Client.exe";
        Directory.CreateDirectory("Builds/Windows");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"[AutoBuilder] Build Windows Succeeded: {summary.totalSize} bytes");
        else
            Debug.LogError($"[AutoBuilder] Build Windows Failed");
    }

    [MenuItem("Build/Auto Build Android")]
    public static void BuildAndroid()
    {
        string buildPath = "Builds/Android/HUNR_Client.apk";
        Directory.CreateDirectory("Builds/Android");

        // Optional: Keystore setup (assuming it's configured in Project Settings already or set it here)
        // PlayerSettings.Android.keystoreName = "path";
        // PlayerSettings.Android.keystorePass = "pass";
        // PlayerSettings.Android.keyaliasName = "alias";
        // PlayerSettings.Android.keyaliasPass = "pass";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"[AutoBuilder] Build Android Succeeded: {summary.totalSize} bytes");
        else
            Debug.LogError($"[AutoBuilder] Build Android Failed");
    }

    [MenuItem("Build/Auto Build iOS")]
    public static void BuildIOS()
    {
        string buildPath = "Builds/iOS"; 
        Directory.CreateDirectory("Builds/iOS");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenes();
        buildPlayerOptions.locationPathName = buildPath; // Output is a folder for Xcode
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"[AutoBuilder] Build iOS Succeeded. Xcode project generated at {buildPath}");
        else
            Debug.LogError($"[AutoBuilder] Build iOS Failed");
    }
}
