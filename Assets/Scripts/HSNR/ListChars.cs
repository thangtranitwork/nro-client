using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class ListChars
    {
        public int widthRect;
        private static ListChars instance;
        public int heightRect;
        public List<Char> listCharsInMap = new List<Char>();
        public bool isShow;
        public static ListChars gI()
        {
            return (instance == null) ? (instance = new ListChars()) : instance;
        }
        public void paintListCharsInMap(mGraphics g)
        {
            if (!isShow) return;
            int num = ((Boss.gI().isShow) ? 96 : 37);
            widthRect = 142;
            heightRect = 9;
            for (int i = 0; i < listCharsInMap.Count; i++)
            {
                Char @char = listCharsInMap[i];
                if (@char.cFlag != 0)
                {
                    g.setColor(getFlagColor(@char));
                    g.fillRect(GameCanvas.w - widthRect - 10, num + 2, 7, 8);
                }
                g.setColor(2721889, 0.5f);
                g.fillRect(GameCanvas.w - widthRect, num + 2, widthRect - 2, heightRect);
                if (@char.cName != null && @char.cName != "" && !@char.isPet && !@char.isMiniPet && !@char.cName.StartsWith("#") && !@char.cName.StartsWith("$") && @char.cName != "Trọng tài")
                {
                    string text = string.Concat(new object[4]
                    {
                        @char.cName,
                        " [",
                        NinjaUtil.getMoneys(@char.cHP),
                        "]"
                    });
                    bool flag;
                    if (!(flag = isBoss(@char)))
                        text = string.Concat(new object[6]
                        {
                            @char.cName,
                            " [",
                            NinjaUtil.getMoneys(@char.cHP),
                            " - ",
                            @char.getGender(),
                            "]"
                        });
                    if (Char.myCharz().charFocus != null && Char.myCharz().charFocus.cName == @char.cName)
                    {
                        g.setColor(14155776);
                        g.drawLine(Char.myCharz().cx - GameScr.cmx, Char.myCharz().cy - GameScr.cmy + 1, @char.cx - GameScr.cmx, @char.cy - GameScr.cmy);
                        mFont.tahoma_7_red.drawString(g, i + 1 + ". " + text, GameCanvas.w - widthRect + 2, num, 0);
                    }
                    else if (flag)
                    {
                        g.setColor(16383818);
                        g.drawLine(Char.myCharz().cx - GameScr.cmx, Char.myCharz().cy - GameScr.cmy + 1, @char.cx - GameScr.cmx, @char.cy - GameScr.cmy);
                        mFont.tahoma_7_yellow.drawString(g, i + 1 + ". " + text, GameCanvas.w - widthRect + 2, num, 0);
                    }
                    else if (@char.cHPFull > 100000000 && @char.cHP > 0)
                    {
                        mFont.tahoma_7_red.drawString(g, i + 1 + ". " + text, GameCanvas.w - widthRect + 2, num, 0);
                    }
                    else
                    {
                        mFont.tahoma_7.drawString(g, i + 1 + ". " + text, GameCanvas.w - widthRect + 2, num, 0);
                    }
                    num += heightRect + 1;
                }
            }
        }
        private Color getFlagColor(Char @char)
        {
            switch (@char.cFlag)
            {
                case 1:
                    return Color.cyan;
                case 2:
                    return Color.red;
                case 3:
                    return new Color(0.56f, 0.19f, 0.77f);
                case 4:
                    return Color.yellow;
                case 5:
                    return Color.green;
                case 6:
                    return Color.magenta;
                case 7:
                    return new Color(1f, 0.5f, 0f);
                case 8:
                    return new Color(0.18f, 0.18f, 0.18f);
                case 9:
                    return Color.blue;
                case 10:
                    return Color.red;
                case 11:
                    return Color.blue;
                case 12:
                    return Color.white;
                case 13:
                    return Color.black;
                default:
                    return Color.clear;
            }
        }
        public void UpdateKey()
        {
            for (int i = 0; i < ListChars.gI().listCharsInMap.Count; i++)
            {
                if ((Boss.gI().isShow) ? GameCanvas.isPointerHoldIn(GameCanvas.w - ListChars.gI().widthRect, 96 + (ListChars.gI().heightRect + 1) * i, ListChars.gI().widthRect, ListChars.gI().heightRect) :
                    GameCanvas.isPointerHoldIn(GameCanvas.w - ListChars.gI().widthRect, 37 + (ListChars.gI().heightRect + 1) * i, ListChars.gI().widthRect, ListChars.gI().heightRect))
                {
                    GameCanvas.isPointerJustDown = false;
                    GameScr.gI().isPointerDowning = false;
                    if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        global::Char @char = ListChars.gI().listCharsInMap[i];
                        if (global::Char.myCharz().charFocus != null && global::Char.myCharz().charFocus.cName == @char.cName)
                        {
                            GameUtils.gI().FocusTo(@char.charID);
                        }
                        else
                        {
                            GameUtils.gI().FocusTo(@char.charID);
                        }
                        global::Char.myCharz().currentMovePoint = null;
                        GameCanvas.clearAllPointerEvent();
                        return;
                    }
                }
            }
        }
        public bool isBoss(Char ch)
        {
            if (ch.cName != null && ch.cName != "" && !ch.isPet && !ch.isMiniPet && char.IsUpper(char.Parse(ch.cName.Substring(0, 1))) && ch.cName != "Trọng tài" && !ch.cName.StartsWith("#"))
                return !ch.cName.StartsWith("$");
            return false;
        }
        public void Update()
        {
            if (isShow)
            {
                listCharsInMap.Clear();
                for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                {
                    Char @char = (Char)GameScr.vCharInMap.elementAt(i);
                    if (@char.cName != null && @char.cName != "" && !@char.isPet && !@char.isMiniPet && !@char.cName.StartsWith("#") && !@char.cName.StartsWith("$") && @char.cName != "Trọng tài")
                        listCharsInMap.Add(@char);
                }
            }
        }
    }
}
