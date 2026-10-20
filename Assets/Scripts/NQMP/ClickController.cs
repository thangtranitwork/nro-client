using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace NQMP
{
   public class ClickController
    {
        public static Image btnClick1 = GameCanvas.loadImage("/mainImage/setclick1.png");
        public static Image btnClick2 = GameCanvas.loadImage("/mainImage/setclick2.png");
        public static int selected;
        public static bool isClick()
        {
            return !Main.isPC && Panel.isShowPhimTat;
        }

        public static void paintClick(mGraphics g, int x, int y, string clickCaption)
        {
            if (isClick())
            {
                if (!GameCanvas.panel.isShow && GameCanvas.currentDialog == null && ChatPopup.currChatPopup == null && ChatPopup.serverChatPopUp == null)
                {
                    int w = btnClick1.getWidth();
                    int h = btnClick1.getHeight();
                    g.drawImage(btnClick1, x, y);
                    mFont.tahoma_7b_dark.drawString(g, clickCaption,x+(w/2), y+(h/2)-5, mFont.CENTER);
                }
            }
        }
      
    }
}
