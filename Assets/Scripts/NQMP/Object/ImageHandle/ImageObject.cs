using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HairMod.Object.ImageHandle
{
   internal class ImageObject :  IChatable, IActionListener
    {
        public static ImageObject gI;
        public static string[] arrimageUrl = new string[]{
        "Image Url",
        "Nhập Đường Dẫn Đến Url Ảnh:"
        };
        public static string imageUrl;
        public static bool isBG;
        public static Texture2D textureBG;
        public static Image imgBg;
        private void Start()
        {
            if(gI == null)
            {
                gI = this;
            }
            gI = this;
           
        }
        static ImageObject()
        {
            
        }
       public static IEnumerator LoadImage()
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://images6.alphacoders.com/108/1083121.png"))
            {
                yield return www.SendWebRequest();
                textureBG = DownloadHandlerTexture.GetContent(www);
                imgBg = Image.createImage(textureBG.EncodeToPNG());
                
            }
           
        }
        public void perform(int idAction, object p)
        {

        }
        public void update()
        {

        }
        private static void ResetTF()
        {
            ChatTextField.gI().isShow = false;
            ChatTextField.gI().strChat = "Chat";
            ChatTextField.gI().tfChat.name = "chat";
            ChatTextField.gI().parentScreen = GameScr.gI();
        }
        public void onChatFromMe(string text, string to)
        {
          
        }
        public void onCancelChat() { }
    }
}
