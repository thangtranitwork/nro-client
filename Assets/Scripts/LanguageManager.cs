using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LanguageManager
{
    private static Dictionary<string, string> languageStrings = new Dictionary<string, string>();
    private static bool isInitialized = false;
    
    /// <summary>
    /// Load tất cả các file .txt trong thư mục Resources/Language
    /// </summary>
    public static void LoadLanguage()
    {
        languageStrings.Clear();
        
        // Load tất cả file .txt trong thư mục Language
        TextAsset[] languageFiles = Resources.LoadAll<TextAsset>("Language");
        
        foreach (TextAsset file in languageFiles)
        {
            if (file != null)
            {
                ParseLanguageFile(file.text, file.name);
            }
        }
        
        isInitialized = true;
        Debug.Log($"Đã load {languageStrings.Count} chuỗi ngôn ngữ từ {languageFiles.Length} file");
    }
    
    /// <summary>
    /// Parse nội dung file ngôn ngữ và thêm vào dictionary
    /// </summary>
    /// <param name="content">Nội dung file</param>
    /// <param name="fileName">Tên file</param>
    private static void ParseLanguageFile(string content, string fileName)
    {
        string[] lines = content.Split('\n');
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // Bỏ qua dòng trống hoặc comment
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;
            
            // Tìm vị trí dấu hai chấm
            int colonIndex = trimmedLine.IndexOf(':');
            if (colonIndex > 0)
            {
                string key = trimmedLine.Substring(0, colonIndex).Trim();
                string value = trimmedLine.Substring(colonIndex + 1).Trim();
                
                // Loại bỏ dấu ngoặc kép nếu có
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                // Thêm vào dictionary
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    languageStrings[key] = value;
                }
            }
        }
    }
    
    /// <summary>
    /// Lấy chuỗi ngôn ngữ theo key
    /// </summary>
    /// <param name="keyValue">Key cần lấy</param>
    /// <returns>Chuỗi ngôn ngữ tương ứng, hoặc key nếu không tìm thấy</returns>
    public static string getString(string keyValue)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("LanguageManager chưa được khởi tạo. Vui lòng gọi LoadLanguage() trước.");
            return keyValue;
        }
        
        if (languageStrings.ContainsKey(keyValue))
        {
            return languageStrings[keyValue];
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy key: {keyValue}");
            return keyValue; // Trả về key nếu không tìm thấy
        }
    }
    
    /// <summary>
    /// Kiểm tra xem key có tồn tại không
    /// </summary>
    /// <param name="keyValue">Key cần kiểm tra</param>
    /// <returns>True nếu key tồn tại</returns>
    public static bool HasKey(string keyValue)
    {
        return languageStrings.ContainsKey(keyValue);
    }
    
    /// <summary>
    /// Lấy tất cả các key đã load
    /// </summary>
    /// <returns>Danh sách tất cả key</returns>
    public static string[] GetAllKeys()
    {
        string[] keys = new string[languageStrings.Count];
        languageStrings.Keys.CopyTo(keys, 0);
        return keys;
    }
    
    /// <summary>
    /// Lấy số lượng chuỗi đã load
    /// </summary>
    /// <returns>Số lượng chuỗi</returns>
    public static int GetStringCount()
    {
        return languageStrings.Count;
    }
}
