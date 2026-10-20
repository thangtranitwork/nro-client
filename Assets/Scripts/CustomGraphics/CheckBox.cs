using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomGraphics
{
    class CheckBox
    {
        public int x, y, w, h;
        public bool isCheck;
        public Action eventAction;
        public string caption;
        public CheckBox(int x, int y, int w, int h, Action action, string caption)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.eventAction = action;
            this.caption = caption;
        }
        public void updateKey()
        {
            if (GameCanvas.isPointerHoldIn(x, y, w, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
            {
                GameCanvas.clearAllPointerEvent();
                try
                {
                    setCheck(!isCheck);
                    if (eventAction != null)
                    {
                        eventAction?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
        }
        public void paint(mGraphics g)
        {
            g.setColor(!isCheck ? 11837316 : 7040779);
            g.fillRect(x, y, w, h, 5);
            g.setColor(Color.white);
            g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
            if (isCheck)
            {
                mFont.tahoma_7b_dark.drawString(g, "X", x + w / 2, y + 5, mFont.CENTER);
            }
            mFont.tahoma_7b_dark.drawString(g, caption, x + w + 5, y + 5, mFont.LEFT);
        }
        public void setCheck(bool set)
        {
            isCheck = set;
        }
    }
}