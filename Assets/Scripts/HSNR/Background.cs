//using UnityEngine;
//using System.IO;
//using System.Collections;
//namespace Assets.Scripts.Assembly_CSharp.HSNR
//{
//    public class Background : IActionListener
//    {
//        public bool isShow;
//        public Texture2D texture;
//        private Image img;
//        private static Background instance;
//        public static Background gI()
//        {
//            if (instance == null)
//            {
//                instance = new Background();
//            }
//            return instance;
//        }
//        public void OpenGallery()
//        {
//            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((paths) =>
//            {
//                if (paths == null)
//                {
//                    Debug.Log("No images selected.");
//                    return;
//                }
//                Debug.Log("Selected image: " + paths);
//                Main.main.StartCoroutine(LoadImage(paths));
//            }, "Select images", "image/jpg");

//            if (permission == NativeGallery.Permission.Denied)
//            {
//                Debug.Log("Permission denied.");
//            }
//            else if (permission == NativeGallery.Permission.ShouldAsk)
//            {
//                Debug.Log("Need to request permission.");
//            }
//        }

//        private IEnumerator LoadImage(string path)
//        {
//            using (WWW www = new WWW("file:///" + path))
//            {
//                yield return www;

//                if (www.error != null)
//                {
//                    Debug.LogError("Error loading image: " + www.error);
//                }
//                else
//                {
//                    Texture2D texture = www.texture;
//                    this.texture = texture;
//                }
//            }
//        }
//        public void Paint(mGraphics g)
//        {
//            if (isShow)
//            {
//                if (Main.isPC)
//                {
//                    if (img == null)
//                    {
//                        g.setColor(999999999);
//                        g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
//                    }
//                    else
//                    {
//                        g.drawImageScale(img, 0, 0, GameCanvas.w, GameCanvas.h, 0);
//                    }
//                }
//                else
//                {
//                    if (texture == null)
//                    {
//                        g.setColor(999999999);
//                        g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
//                    }
//                    else
//                    {
//                        GUI.DrawTexture(new Rect(0, 0, ScaleGUI.WIDTH, ScaleGUI.HEIGHT), texture);
//                    }

//                }
//            }
//        }
//        public void Update()
//        {
//            if (!isShow) return;
//            if (img == null)
//            {
//                if (Main.isPC)
//                {
//                    img = Image.createImage(File.ReadAllBytes("Backgrounds\\1.png"));
//                }
//            }
//        }

//        public void perform(int idAction, object p)
//        {
//            switch (idAction)
//            {
//                case 1:
//                    OpenGallery();
//                    break;
//            }
//        }
//    }
//}
