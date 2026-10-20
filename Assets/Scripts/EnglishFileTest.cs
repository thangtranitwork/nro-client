//using UnityEngine;

//public class EnglishFileTest : MonoBehaviour
//{
//    void Start()
//    {
//        // Test file english.txt mới với format string và string[]
//        TestEnglishFile();
//    }
    
//    void TestEnglishFile()
//    {
//        Debug.Log("=== TEST FILE ENGLISH.TXT MỚI ===");
        
//        // Load ngôn ngữ
//        LanguageManager.LoadLanguage();
//        Debug.Log($"Đã load {LanguageManager.GetStringCount()} chuỗi ngôn ngữ");
        
//        // Test các string cơ bản
//        Debug.Log("=== TEST STRING CƠ BẢN ===");
//        Debug.Log($"learnSkill: {LanguageManager.getString("learnSkill")}");
//        Debug.Log($"login: {LanguageManager.getString("login")}");
//        Debug.Log($"register: {LanguageManager.getString("register")}");
//        Debug.Log($"MENU: {LanguageManager.getString("MENU")}");
//        Debug.Log($"YES: {LanguageManager.getString("YES")}");
//        Debug.Log($"NO: {LanguageManager.getString("NO")}");
        
//        // Test các string array
//        Debug.Log("=== TEST STRING ARRAYS ===");
        
//        // Test LOGINLABELS
//        string[] loginLabels = LanguageManager.getStringArray("LOGINLABELS");
//        if (loginLabels != null)
//        {
//            Debug.Log($"LOGINLABELS array length: {loginLabels.Length}");
//            for (int i = 0; i < loginLabels.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{loginLabels[i]}'");
//            }
//        }
        
//        // Test MENUME
//        string[] menuMe = LanguageManager.getStringArray("MENUME");
//        if (menuMe != null)
//        {
//            Debug.Log($"MENUME array length: {menuMe.Length}");
//            for (int i = 0; i < menuMe.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{menuMe[i]}'");
//            }
//        }
        
//        // Test key_skill_qwerty
//        string[] keySkillQwerty = LanguageManager.getStringArray("key_skill_qwerty");
//        if (keySkillQwerty != null)
//        {
//            Debug.Log($"key_skill_qwerty array length: {keySkillQwerty.Length}");
//            for (int i = 0; i < keySkillQwerty.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{keySkillQwerty[i]}'");
//            }
//        }
        
//        // Test key_skill
//        string[] keySkill = LanguageManager.getStringArray("key_skill");
//        if (keySkill != null)
//        {
//            Debug.Log($"key_skill array length: {keySkill.Length}");
//            for (int i = 0; i < keySkill.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{keySkill[i]}'");
//            }
//        }
        
//        // Test CHAR_ORDER
//        string[] charOrder = LanguageManager.getStringArray("CHAR_ORDER");
//        if (charOrder != null)
//        {
//            Debug.Log($"CHAR_ORDER array length: {charOrder.Length}");
//            for (int i = 0; i < charOrder.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{charOrder[i]}'");
//            }
//        }
        
//        // Test tips
//        string[] tips = LanguageManager.getStringArray("tips");
//        if (tips != null)
//        {
//            Debug.Log($"tips array length: {tips.Length}");
//            for (int i = 0; i < tips.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{tips[i]}'");
//            }
//        }
        
//        // Test mainTab1
//        string[] mainTab1 = LanguageManager.getStringArray("mainTab1");
//        if (mainTab1 != null)
//        {
//            Debug.Log($"mainTab1 array length: {mainTab1.Length}");
//            for (int i = 0; i < mainTab1.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{mainTab1[i]}'");
//            }
//        }
        
//        // Test mainTab2
//        string[] mainTab2 = LanguageManager.getStringArray("mainTab2");
//        if (mainTab2 != null)
//        {
//            Debug.Log($"mainTab2 array length: {mainTab2.Length}");
//            for (int i = 0; i < mainTab2.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{mainTab2[i]}'");
//            }
//        }
        
//        // Test petMainTab
//        string[] petMainTab = LanguageManager.getStringArray("petMainTab");
//        if (petMainTab != null)
//        {
//            Debug.Log($"petMainTab array length: {petMainTab.Length}");
//            for (int i = 0; i < petMainTab.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{petMainTab[i]}'");
//            }
//        }
        
//        // Test petMainTab2
//        string[] petMainTab2 = LanguageManager.getStringArray("petMainTab2");
//        if (petMainTab2 != null)
//        {
//            Debug.Log($"petMainTab2 array length: {petMainTab2.Length}");
//            for (int i = 0; i < petMainTab2.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{petMainTab2[i]}'");
//            }
//        }
        
//        // Test DIES
//        string[] dies = LanguageManager.getStringArray("DIES");
//        if (dies != null)
//        {
//            Debug.Log($"DIES array length: {dies.Length}");
//            for (int i = 0; i < dies.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{dies[i]}'");
//            }
//        }
        
//        // Test SYNTHESIS
//        string[] synthesis = LanguageManager.getStringArray("SYNTHESIS");
//        if (synthesis != null)
//        {
//            Debug.Log($"SYNTHESIS array length: {synthesis.Length}");
//            for (int i = 0; i < synthesis.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{synthesis[i]}'");
//            }
//        }
        
//        // Test DES_TASK
//        string[] desTask = LanguageManager.getStringArray("DES_TASK");
//        if (desTask != null)
//        {
//            Debug.Log($"DES_TASK array length: {desTask.Length}");
//            for (int i = 0; i < desTask.Length; i++)
//            {
//                Debug.Log($"  [{i}]: '{desTask[i]}'");
//            }
//        }
        
//        Debug.Log("=== HOÀN THÀNH TEST ===");
//    }
//}






