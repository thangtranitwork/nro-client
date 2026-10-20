using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorGame : EditorWindow
{
    [MenuItem("Editor/Xoa DL")]
    public static void clear()
    {
        Rms.clearAll();
    }
}
