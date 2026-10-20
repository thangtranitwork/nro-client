using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorTexture : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;
        textureImporter.textureFormat = TextureImporterFormat.DXT5Crunched;
        textureImporter.compressionQuality = 100;
        textureImporter.mipmapEnabled = false;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.maxTextureSize = 16384; 
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(File.ReadAllBytes(textureImporter.assetPath));
        texture.Apply();

        int w = texture.width;
        int h = texture.height;
        bool ok = false;
        while (w % 4 != 0)
        {
            w += 1;
            ok = true;
        }
        while (h % 4 != 0)
        {
            h += 1;
            ok = true;
        }
        if (ok)
        {
            Texture2D texture1 = new Texture2D(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    texture1.SetPixel(i, j, Color.clear);
                }
            }

            texture1.SetPixels(0, 0, texture.width, texture.height, texture.GetPixels());
            texture1.Apply();
            File.WriteAllBytes(textureImporter.assetPath, texture1.EncodeToPNG());
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("resize " + textureImporter.assetPath);
        }
    }


}
