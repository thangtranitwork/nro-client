using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ReplaceMResources : MonoBehaviour
{
    [ContextMenu("Replace mResources with LanguageManager.getString")]
    public void ReplaceAllMResources()
    {
        string scriptsPath = Application.dataPath + "/Scripts";
        string[] files = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        
        int totalReplacements = 0;
        int filesProcessed = 0;
        
        foreach (string file in files)
        {
            // Skip this file itself
            if (file.Contains("ReplaceMResources.cs"))
                continue;
                
            string content = File.ReadAllText(file);
            string originalContent = content;
            
            // Replace mResources. with LanguageManager.getString(
            // This regex finds mResources. followed by a word and replaces it
            content = Regex.Replace(content, @"mResources\.(\w+)", "LanguageManager.getString(\"$1\")");
            
            if (content != originalContent)
            {
                File.WriteAllText(file, content);
                int replacements = Regex.Matches(originalContent, @"mResources\.\w+").Count;
                totalReplacements += replacements;
                filesProcessed++;
                Debug.Log($"Processed {file}: {replacements} replacements");
            }
        }
        
        Debug.Log($"Total files processed: {filesProcessed}");
        Debug.Log($"Total replacements: {totalReplacements}");
    }
}
