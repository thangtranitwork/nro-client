using System;
using System.Collections.Generic;
using NQMP;
using UnityEngine;

public class AutoPro
{
    public static AutoPro instance = new AutoPro();

    private bool isAutoAttack;

    private long[] lastTimeAutoAttack = new long[10];

    private bool isAutoPick;

    private long lastTimePick;

    private bool isCanAutoPlay;

    private long lastTimeMoveAutoPlay;

    private bool isAutoFocusBoss;

    private bool isAutoChat;

    private string contentChat;

    private long lastTimeChat;

    public List<ChatVip> chatVips = new List<ChatVip>();

    public bool isPaintBossInfo;

    public bool isPaintCharInfo;

    private int wPaintChar = 142;

    private int hPaintChar = 10;

    public void Update()
    {
        NQMP.NQMPMain.aaMod.Update();
        if (mSystem.currentTimeMillis() - Service.gI().lastRequestPing >= 1000L)
        {
            Service.gI().pingToServer();
        }
        if (GameCanvas.gameTick % 30 == 0 && MoreSetting.autoUpdateArea)
        {
            Service.gI().openUIZone();
        }
        //if (isAutoAttack)
        //{
        //    AutoAttack();
        //}
        //if (isAutoPick && !GameScr.canAutoPlay)
        //{
        //    SearchItem();
        //    if (Char.myCharz().itemFocus != null)
        //    {
        //        PickItem();
        //    }
        //}
        //if (GameScr.isAutoPlay && (GameScr.canAutoPlay || isCanAutoPlay))
        //{
        //    if (Char.myCharz().mobFocus != null && !GameScr.canAutoPlay)
        //    {
        //        if (Char.myCharz().mobFocus.x > Char.myCharz().cx && Char.myCharz().cdir == 1 && TileMap.tileTypeAt(Char.myCharz().cx + Char.myCharz().chw, Char.myCharz().cy - Char.myCharz().chh, 4))
        //        {
        //            Char.myCharz().currentMovePoint = new MovePoint(Char.myCharz().cx + 20, GetYSd(Char.myCharz().cx + 20));
        //        }
        //        if (Char.myCharz().mobFocus.x < Char.myCharz().cx && Char.myCharz().cdir == -1 && TileMap.tileTypeAt(Char.myCharz().cx - Char.myCharz().chw - 1, Char.myCharz().cy - Char.myCharz().chh, 8))
        //        {
        //            Char.myCharz().currentMovePoint = new MovePoint(Char.myCharz().cx - 20, GetYSd(Char.myCharz().cx - 20));
        //        }
        //    }
        //    if (GameCanvas.gameTick % 20 == 0)
        //    {
        //        AutoPlay();
        //    }
        //}
        //if (isAutoFocusBoss)
        //{
        //    AutoFocusBoss();
        //}
        //if (isAutoChat && !string.IsNullOrEmpty(contentChat))
        //{
        //    long now = mSystem.currentTimeMillis();
        //    if (now - lastTimeChat > 5000)
        //    {
        //        lastTimeChat = now;
        //        Service.gI().chat(contentChat);
        //    }
        //}
    }

    public void Paint(mGraphics g)
    {
        NQMPMain.Paint(g);
        NQMPMain.aaMod.Paint(g);
        int num = 110;
        if (isAutoAttack)
        {
            mFont.tahoma_7.drawString(g, "Tự đánh: on", 25, num, 0);
            num += 10;
        }
        if (isAutoPick)
        {
            mFont.tahoma_7.drawString(g, "Auto nhặt: on", 25, num, 0);
            num += 10;
        }
        if (isCanAutoPlay)
        {
            mFont.tahoma_7.drawString(g, "Tàn sát: on", 25, num, 0);
            num += 10;
        }
        if (isAutoChat)
        {
            mFont.tahoma_7.drawString(g, "Auto chat: on", 25, num, 0);
            num += 10;
        }
        if (isAutoFocusBoss)
        {
            mFont.tahoma_7.drawString(g, "Auto focus boss: on", 25, num, 0);
            num += 10;
        }

        mFont.tahoma_7.drawString(g, $"X:{Char.myCharz().cx}- Y:{Char.myCharz().cy}", 25, num, 0);
       

    }

 
    public bool KeyCode(int keyCode)
    {
        switch (keyCode)
        {
            case AutoKey.A:
                {
                    isAutoAttack = !isAutoAttack;
                    GameScr.info1.addInfo("Tự đánh\n" + (isAutoAttack ? "[ON]" : "[OFF]"), 0);
                    break;
                }
            case AutoKey.B:
                {
                    for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                    {
                        Item item = Char.myCharz().arrItemBag[i];
                        if (item != null && (item.template.id == 455 || item.template.id == 921))
                        {
                            Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                            break;
                        }
                    }
                    break;
                }
            case AutoKey.C:
                {
                    for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                    {
                        Item item = Char.myCharz().arrItemBag[i];
                        if (item != null && (item.template.id == 194 || item.template.id == 193))
                        {
                            Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                            break;
                        }
                    }
                    break;
                }
            case AutoKey.I:
                {
                    Service.gI().getMapOffline();
                    break;
                }
            case AutoKey.G:
                {
                    if (Char.myCharz().charFocus == null)
                    {
                        GameScr.info1.addInfo("Bạn chưa chọn mục tiêu!", 0);
                    }
                    else
                    {
                        Service.gI().giaodich(0, Char.myCharz().charFocus.charID, -1, -1);
                        GameScr.info1.addInfo("Đã gửi lời mời giao dịch đến " + Char.myCharz().charFocus.subcName(), 0);
                    }
                    break;
                }
            case AutoKey.M:
                {
                    GameCanvas.panel.isShowZone = true;
                    Service.gI().openUIZone();
                    GameCanvas.panel.setTypeZone();
                    GameCanvas.panel.show();
                    break;
                }
            case AutoKey.N:
                {
                    isAutoPick = !isAutoPick;
                    GameScr.info1.addInfo("Auto Nhặt\n" + (isAutoPick ? "[ON]" : "[OFF]"), 0);
                    break;
                }
            case AutoKey.T:
                {
                    isCanAutoPlay = !isCanAutoPlay;
                    if (isCanAutoPlay)
                    {
                        GameScr.isAutoPlay = true;
                    }
                    GameScr.info1.addInfo("Tàn sát\n" + (isCanAutoPlay ? "[ON]" : "[OFF]"), 0);
                    break;
                }
            case AutoKey.S:
                {
                    Char.myCharz().mobFocus = null;
                    Char.myCharz().itemFocus = null;
                    Char.myCharz().npcFocus = null;
                    for (int l = 0; l < GameScr.vCharInMap.size(); l++)
                    {
                        Char @char = (Char)GameScr.vCharInMap.elementAt(l);
                        if (!@char.cName.Equals("") && CharIsBoss(@char) && (Char.myCharz().charFocus == null || (Char.myCharz().charFocus != null && Char.myCharz().charFocus.cName != @char.cName)))
                        {
                            Char.myCharz().charFocus = @char;
                            break;
                        }
                    }
                    break;
                }
        }
        return false;
    }

    public bool OnChatFromMe(string text)
    {
        if (text.Equals("") || text.Contains(" "))
        {
            return false;
        }
        text = text.ToLower();
        if (text.StartsWith("k"))
        {
            try
            {
                Service.gI().requestChangeZone(int.Parse(text.Substring(1)), -1);
            }
            catch
            {
            }
            return true;
        }
        if (text.StartsWith("cheat"))
        {
            try
            {
                Time.timeScale = float.Parse(text.Substring(5));
                GameScr.info1.addInfo("Cheat: " + Time.timeScale, 0);
            }
            catch
            {
            }
            return true;
        }
        if (text.StartsWith("ndc_"))
        {
            try
            {
                contentChat = text.Substring(4);
                GameScr.info1.addInfo("Đã thay đổi nội dung auto chat", 0);
            }
            catch
            {
            }
            return true;
        }
        if (text.Equals("atc"))
        {
            isAutoChat = !isAutoChat;
            GameScr.info1.addInfo("Auto Chat\n" + (isAutoChat ? "[ON]" : "[OFF]"), 0);
            return true;
        }
        return false;
    }

    public bool IsMapNRD()
    {
        if (TileMap.mapID >= 85 && TileMap.mapID <= 91)
        {
            return true;
        }
        return false;
    }

    private void AutoFocusBoss()
    {
        if (Char.myCharz().charFocus != null && CharIsBoss(Char.myCharz().charFocus))
        {
            return;
        }
        for (int l = 0; l < GameScr.vCharInMap.size(); l++)
        {
            Char @char = (Char)GameScr.vCharInMap.elementAt(l);
            if (!@char.cName.Equals("") && CharIsBoss(@char))
            {
                Char.myCharz().charFocus = @char;
                Char.myCharz().mobFocus = null;
                Char.myCharz().itemFocus = null;
                Char.myCharz().npcFocus = null;
                break;
            }
        }
    }

    private void AutoPlay()
    {
        isAutoAttack = true;
        if ((!isCanAutoPlay && !GameScr.canAutoPlay) || Char.myCharz().statusMe == 14 || Char.myCharz().statusMe == 5)
        {
            return;
        }
        if (Char.myCharz().mobFocus == null || (Char.myCharz().mobFocus != null && Char.myCharz().mobFocus.isMobMe))
        {
            if (!GameScr.canAutoPlay && isAutoPick)
            {
                SearchItem();
                if (Char.myCharz().itemFocus != null)
                {
                    PickItem();
                    SearchItem();
                }
            }
            else
            {
                Char.myCharz().itemFocus = null;
            }
            if (Char.myCharz().itemFocus == null)
            {
                Mob mob = FindMob(0);
                if (mob == null)
                {
                    mob = FindMob(1);

                    Char.myCharz().cx = mob.xFirst;
                    Char.myCharz().cy = mob.yFirst;
                    Service.gI().charMove();

                }
                else
                {
                    Char.myCharz().mobFocus = mob;
                    Char.myCharz().cx = mob.x;
                    Char.myCharz().cy = mob.y;
                    Service.gI().charMove();

                }
            }
        }
        else if (Char.myCharz().mobFocus.hp <= 0 || Char.myCharz().mobFocus.status == 1 || Char.myCharz().mobFocus.status == 0 || !IsCanAttackMob(Char.myCharz().mobFocus))
        {
            Char.myCharz().mobFocus = null;
        }
        if (Char.myCharz().mobFocus == null || (Char.myCharz().skillInfoPaint() != null && Char.myCharz().indexSkill < Char.myCharz().skillInfoPaint().Length && Char.myCharz().dart != null && Char.myCharz().arr != null))
        {
            return;
        }
        if (Char.myCharz().mobFocus != null && GameScr.canAutoPlay && (Math.Abs(Char.myCharz().mobFocus.x - Char.myCharz().cx) > 100 || Math.Abs(Char.myCharz().mobFocus.y - Char.myCharz().cy) > 100) && mSystem.currentTimeMillis() - lastTimeMoveAutoPlay > 100)
        {
            lastTimeMoveAutoPlay = mSystem.currentTimeMillis();
            Char.myCharz().cx = Char.myCharz().mobFocus.x;
            Char.myCharz().cy = Char.myCharz().mobFocus.y;
            Service.gI().charMove();
        }
        Skill skill = null;
        for (int i = 0; i < GameScr.keySkill.Length; i++)
        {
            if (GameScr.keySkill[i] == null || GameScr.keySkill[i].paintCanNotUseSkill || GameScr.keySkill[i].template.id == 10 || GameScr.keySkill[i].template.id == 11 || GameScr.keySkill[i].template.id == 14 || GameScr.keySkill[i].template.id == 23 || GameScr.keySkill[i].template.id == 7 || GameScr.keySkill[i].template.id == 3 || GameScr.keySkill[i].template.id == 1 || GameScr.keySkill[i].template.id == 5 || GameScr.keySkill[i].template.id == 20 || GameScr.keySkill[i].template.id == 9 || GameScr.keySkill[i].template.id == 22 || GameScr.keySkill[i].template.id == 18 || (Char.myCharz().cgender == 1 && (Char.myCharz().cgender != 1 || (Char.myCharz().getSkill(Char.myCharz().nClass.skillTemplates[5]) != null && (Char.myCharz().getSkill(Char.myCharz().nClass.skillTemplates[5]) == null || GameScr.keySkill[i].template.id == 2)))) || Char.myCharz().skillInfoPaint() != null)
            {
                continue;
            }
            long num = ((GameScr.keySkill[i].template.manaUseType == 2) ? 1 : ((GameScr.keySkill[i].template.manaUseType == 1) ? (GameScr.keySkill[i].manaUse * Char.myCharz().cMPFull / 100) : GameScr.keySkill[i].manaUse));
            if (Char.myCharz().cMP >= num)
            {
                if (skill == null)
                {
                    skill = GameScr.keySkill[i];
                }
                else if (skill.coolDown < GameScr.keySkill[i].coolDown)
                {
                    skill = GameScr.keySkill[i];
                }
            }
        }
        if (skill != null)
        {
            GameScr.gI().doSelectSkill(skill, isShortcut: true);
            GameScr.gI().doDoubleClickToObj(Char.myCharz().mobFocus);
        }
    }

    private Mob FindMob(int type)
    {
        if (type == 1)
        {
            long num = mSystem.currentTimeMillis();
            Mob result = null;
            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                long cTimeDie = mob.lastTimeDie;
                if (!mob.isMobMe && cTimeDie < num)
                {
                    result = mob;
                    num = cTimeDie;
                }
            }
            return result;
        }
        Mob result2 = null;
        int num2 = 9999;
        for (int j = 0; j < GameScr.vMob.size(); j++)
        {
            Mob mob2 = (Mob)GameScr.vMob.elementAt(j);
            if (mob2.status != 0 && mob2.status != 1 && mob2.hp > 0 && !mob2.isMobMe && IsCanAttackMob(mob2))
            {
                int num3 = Math.Abs(Char.myCharz().cx - mob2.x);
                if (num2 > num3)
                {
                    result2 = mob2;
                    num2 = num3;
                }
            }
        }
        return result2;
    }

    private bool IsCanAttackMob(Mob mob)
    {
        if (!GameScr.canAutoPlay && mob.checkIsBoss())
        {
            return false;
        }
        return true;
    }

    private int GetYSd(int xSd)
    {
        int num = 50;
        int num2 = 0;
        while (num2 < 30)
        {
            num2++;
            num += 24;
            if (TileMap.tileTypeAt(xSd, num, 2))
            {
                if (num % 24 != 0)
                {
                    num -= num % 24;
                }
                break;
            }
        }
        return num;
    }

    public void SearchItem()
    {
        if (Char.myCharz().itemFocus != null)
        {
            return;
        }
        for (int i = 0; i < GameScr.vItemMap.size(); i++)
        {
            ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
            int num = Math.Abs(Char.myCharz().cx - itemMap.x);
            int num2 = Math.Abs(Char.myCharz().cy - itemMap.y);
            if (num <= 50 && num2 <= 50 && itemMap.template.id != 673)
            {
                Char.myCharz().itemFocus = itemMap;
                break;
            }
        }
    }

    public void PickItem()
    {
        if (mSystem.currentTimeMillis() - lastTimePick < 550 || Char.myCharz().itemFocus == null)
        {
            return;
        }
        if (Char.myCharz().cx < Char.myCharz().itemFocus.x)
        {
            Char.myCharz().cdir = 1;
        }
        else
        {
            Char.myCharz().cdir = -1;
        }
        int num = Math.Abs(Char.myCharz().cx - Char.myCharz().itemFocus.x);
        int num2 = Math.Abs(Char.myCharz().cy - Char.myCharz().itemFocus.y);
        if (num <= 40 && num2 < 40)
        {
            GameCanvas.clearKeyHold();
            GameCanvas.clearKeyPressed();
            if (Char.myCharz().itemFocus.template.id != 673)
            {
                Service.gI().pickItem(Char.myCharz().itemFocus.itemMapID);
                lastTimePick = mSystem.currentTimeMillis();
            }
        }
        else
        {
            Char.myCharz().currentMovePoint = new MovePoint(Char.myCharz().itemFocus.x, Char.myCharz().itemFocus.y);
            Char.myCharz().endMovePointCommand = new Command(null, null, 8002, null);
            GameCanvas.clearKeyHold();
            GameCanvas.clearKeyPressed();
        }
    }

    private bool CharIsBoss(Char @char)
    {
        if (@char.cName != null && @char.cName != "" && !@char.isPet && !@char.isMiniPet && char.IsUpper(char.Parse(@char.cName.Substring(0, 1))) && @char.cName != "Trọng tài" && !@char.cName.StartsWith("#"))
        {
            return !@char.cName.StartsWith("$");
        }
        return false;
    }

    private void AutoAttack()
    {
        if (Char.myCharz().meDead || Char.myCharz().cHP <= 0 || Char.myCharz().statusMe == 14 || Char.myCharz().statusMe == 5 || Char.myCharz().myskill.template.type == 3 || Char.myCharz().myskill.template.id == 10 || Char.myCharz().myskill.template.id == 11 || Char.myCharz().myskill.paintCanNotUseSkill)
        {
            return;
        }
        int indexSkill = GetIndexSkill();
        if (mSystem.currentTimeMillis() - lastTimeAutoAttack[indexSkill] > GetCoolDown(Char.myCharz().myskill, indexSkill))
        {
            if (GameScr.gI().isMeCanAttackMob(Char.myCharz().mobFocus))
            {
                int distance = TileMap.pxw;
                if (!GameScr.canAutoPlay)
                {
                    distance = (int)((double)Char.myCharz().myskill.dx * 1.7);
                }
                if (Math.Abs(Char.myCharz().mobFocus.xFirst - Char.myCharz().cx) <= distance)
                {
                    Char.myCharz().myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
                    GetAttackMob();
                    lastTimeAutoAttack[indexSkill] = mSystem.currentTimeMillis();
                }
            }
            else if (Char.myCharz().charFocus != null && IsCanAttackCharFocus(Char.myCharz().charFocus) && (double)Math.Abs(Char.myCharz().charFocus.cx - Char.myCharz().cx) < (double)Char.myCharz().myskill.dx * 1.7)
            {
                Char.myCharz().myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
                GetAttackChar();
                lastTimeAutoAttack[indexSkill] = mSystem.currentTimeMillis();
            }
        }
    }

    private bool IsCanAttackCharFocus(Char @char)
    {
        if (TileMap.mapID == 113)
        {
            if (@char != null && Char.myCharz().myskill != null)
            {
                if (@char.cTypePk != 5)
                {
                    return @char.cTypePk == 3;
                }
                return true;
            }
            return false;
        }
        if (@char != null && Char.myCharz().myskill != null)
        {
            if (@char.statusMe == 14 || @char.statusMe == 5 || Char.myCharz().myskill.template.type == 2 || ((Char.myCharz().cFlag != 8 || @char.cFlag == 0) && (Char.myCharz().cFlag == 0 || @char.cFlag != 8) && (Char.myCharz().cFlag == @char.cFlag || Char.myCharz().cFlag == 0 || @char.cFlag == 0) && (@char.cTypePk != 3 || Char.myCharz().cTypePk != 3) && Char.myCharz().cTypePk != 5 && @char.cTypePk != 5 && (Char.myCharz().cTypePk != 1 || @char.cTypePk != 1) && (Char.myCharz().cTypePk != 4 || @char.cTypePk != 4)))
            {
                if (Char.myCharz().myskill.template.type == 2)
                {
                    return @char.cTypePk != 5;
                }
                return false;
            }
            return true;
        }
        return false;
    }

    private void GetAttackChar()
    {
        MyVector myVector = new MyVector();
        myVector.addElement(Char.myCharz().charFocus);
        Service.gI().sendPlayerAttack(new MyVector(), myVector, 2);
    }

    private void GetAttackMob()
    {
        MyVector myVector = new MyVector();
        myVector.addElement(Char.myCharz().mobFocus);
        Service.gI().sendPlayerAttack(myVector, new MyVector(), 1);
    }

    private long GetCoolDown(Skill skill, int index)
    {
        if (skill.template.id == 20 || skill.template.id == 22 || skill.template.id == 7 || skill.template.id == 18 || skill.template.id == 23)
        {
            return (long)skill.coolDown + 500L;
        }
        long num = (long)((double)skill.coolDown * 1.2);
        if (num < 415)
        {
            return 415L;
        }
        return num;
    }

    private int GetIndexSkill()
    {
        for (int i = 0; i < GameScr.keySkill.Length; i++)
        {
            if (GameScr.keySkill[i] == Char.myCharz().myskill)
            {
                return i;
            }
        }
        return 0;
    }
}