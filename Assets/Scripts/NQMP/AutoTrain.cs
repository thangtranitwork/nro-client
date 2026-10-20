using NQMP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NQMP.NQMPMain;

public class AutoTrain : IActionListener
{
    private static AutoTrain _Instance;
    public static bool isAutoTrain;
    private static bool isAvoidSuperMob;
    private static int minimumMPGoHome;
    private static string[] inputMPPercentGoHome;
    public static List<Mob> listMobIds = new List<Mob>();
    public static long lastTimeAddNewMob;
    private static long lastTimeTeleportToMob;
    public static AutoTrain getInstance()
    {
        if (AutoTrain._Instance == null)
        {
            AutoTrain._Instance = new AutoTrain();
        }
        return AutoTrain._Instance;
    }
    private static Mob GetNextMob(int type)
    {

        long num = mSystem.currentTimeMillis();
        Mob result = null;
        for (int i = 0; i < GameScr.vMob.size(); i++)
        {
            Mob mob2 = (Mob)GameScr.vMob.elementAt(i);
            if (mob2.status != 0 && mob2.status != 1 && mob2.hp > 0 && !mob2.isMobMe)
            {
                if (AutoTrain.listMobIds.Contains(mob2))
                {
                    return mob2;

                }
            }
        }
        return result;
    }
    private static bool isMeCanAttack(Mob mob)
    {
        return true;
    }
    public static void ResetFocus()
    {
        Char.myCharz().itemFocus = null;
        Char.myCharz().mobFocus = null;
        Char.myCharz().charFocus = null;
        Char.myCharz().npcFocus = null;
    }
    private static void DoIt()
    {
        try
        {
            if (!AutoTrain.isAutoTrain || global::Char.myCharz().statusMe == 14 || global::Char.myCharz().statusMe == 5)
            {
                return;
            }
            if (AutoTrain.listMobIds.Count == 0)
            {
                if (mSystem.currentTimeMillis() - AutoTrain.lastTimeAddNewMob > 5000L)
                {
                    AutoTrain.lastTimeAddNewMob = mSystem.currentTimeMillis();
                    GameScr.info1.addInfo("Danh Sách Tàn Sát Trống!", 0);
                }
                AutoTrain.isAutoTrain = false;
                return;
            }
            if (global::Char.myCharz().mobFocus == null ||  global::Char.myCharz().mobFocus.isMobMe)
            {
                Char.myCharz().mobFocus = AutoTrain.GetNextMob(0);
            }
            if (global::Char.myCharz().mobFocus == null || (global::Char.myCharz().skillInfoPaint() != null && global::Char.myCharz().indexSkill < global::Char.myCharz().skillInfoPaint().Length && global::Char.myCharz().dart != null && global::Char.myCharz().arr != null))
            {
                return;
            }

            if (Char.myCharz().mobFocus != null && mSystem.currentTimeMillis() - Char.myCharz().myskill.lastTimeUseThisSkill >= Char.myCharz().myskill.coolDown)
            {
                Mob mob2 = Char.myCharz().mobFocus;
                if (mob2.status != 0 && mob2.status != 1 && mob2.hp > 0 && !mob2.isMobMe)
                {
                    if (Math.Abs(mob2.x - Char.myCharz().cx) >= 50 || Math.Abs(mob2.y - Char.myCharz().cy) >= 50)
                    {
                        Char.myCharz().cx = mob2.x;
                        Char.myCharz().cy = mob2.y;
                        Service.gI().charMove();
                    }
                }
                else
                {
                    Char.myCharz().mobFocus = null;
                }
            }
            Skill skill = null;
            Skill[] skillcheck = Main.isPC ? GameScr.keySkill : GameScr.onScreenSkill;
            for (int m = 0; m < skillcheck.Length; m++)
            {
                Skill s = skillcheck[m];
                List<int> ints = new List<int>() { 6, 9, 10, 20, 22, 24, 19, 7, 11, 18, 26, 8, 14, 21, 23, 25 };
                if (s != null && !ints.Contains(s.template.id) && mSystem.currentTimeMillis() - s.lastTimeUseThisSkill >= s.coolDown + 50)
                {
                    long num2 = 0;
                    num2 = ((skillcheck[m].template.manaUseType == 2) ? 1 : ((skillcheck[m].template.manaUseType == 1) ? (skillcheck[m].manaUse * Char.myCharz().cMPFull / 100) : skillcheck[m].manaUse));
                    if (Char.myCharz().cMP >= num2)
                    {
                        skill = s;
                        break;
                    }
                }
            }
            if (skill != null)
            {
                if (skill != Char.myCharz().myskill)
                {
                    Service.gI().selectSkill(skill.template.id);
                    Char.myCharz().myskill = skill;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    public static void ShowMenu()
    {
        MyVector myVector = new MyVector();
        List<Mob> list = new List<Mob>();
        if (AutoTrain.isAutoTrain)
        {
            myVector.addElement(new Command("Tắt Auto Train", AutoTrain.getInstance(), 8, null));
        }
        for (int i = 0; i < GameScr.vMob.size(); i++)
        {
            Mob mob = (Mob)GameScr.vMob.elementAt(i);
            if (!mob.isMobMe)
            {
                bool flag = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (mob.templateId == list[j].templateId)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(mob);
                    myVector.addElement(new Command(string.Concat(new string[]
                    {
                    "Tàn Sát\n",
                    mob.getTemplate().name,
                    "\n[",
                    NinjaUtil.getMoneys((long)mob.maxHp),
                    "HP]"
                    }), AutoTrain.getInstance(), 1, mob.templateId));
                }
            }
        }
        myVector.addElement(new Command("Tàn Sát Tất Cả", AutoTrain.getInstance(), 2, null));
        myVector.addElement(new Command("Clear Danh Sách Train", AutoTrain.getInstance(), 3, null));
        if (global::Char.myCharz().mobFocus != null)
        {
            myVector.addElement(new Command(string.Concat(new string[]
            {
            "Thêm\n[",
            global::Char.myCharz().mobFocus.getTemplate().name,
            "]\n[",
            global::Char.myCharz().mobFocus.mobId.ToString(),
            "]"
            }), AutoTrain.getInstance(), 7, null));
        }
        GameCanvas.menu.startAt(myVector, 3);
    }
    private static void TurnOnAutoTrain()
    {
        if (AutoTrain.listMobIds.Count == 0)
        {
            GameScr.info1.addInfo("Danh Sách Tàn Sát Trống!", 0);
            AutoTrain.isAutoTrain = false;
            return;
        }
        AutoTrain.isAutoTrain = true;
        aaMod.isTanSat = true;
    }
    public void perform(int idAction, object p)
    {
        switch (idAction)
        {
            case 1:
                {
                    int num = (int)p;
                    AutoTrain.listMobIds.Clear();
                    for (int i = 0; i < GameScr.vMob.size(); i++)
                    {
                        Mob mob = (Mob)GameScr.vMob.elementAt(i);
                        if (!mob.isMobMe && mob.templateId == num)
                        {
                            AutoTrain.listMobIds.Add(mob);
                        }
                    }
                    AutoTrain.TurnOnAutoTrain();
                    return;
                }
            case 2:
                AutoTrain.listMobIds.Clear();
                for (int j = 0; j < GameScr.vMob.size(); j++)
                {
                    Mob mob2 = (Mob)GameScr.vMob.elementAt(j);
                    if (!mob2.isMobMe)
                    {
                        AutoTrain.listMobIds.Add(mob2);
                        Debug.LogError("addmob:" + mob2.getTemplate().name);
                    }
                }
                AutoTrain.TurnOnAutoTrain();
                return;
            case 3:
                AutoTrain.listMobIds.Clear();
                AutoTrain.isAutoTrain = false;
                GameScr.info1.addInfo("Đã Clear Danh Sách Train!", 0);
                return;
            case 8:
                AutoTrain.isAutoTrain = false;
                aaMod.isTanSat = false;
                global::Char.myCharz().mobFocus = null;
                GameScr.info1.addInfo("Đã Tắt Auto Train!", 0);
                return;
        }
    }

    public static void UseGrape()
    {
        for (int i = 0; i < global::Char.myCharz().arrItemBag.Length; i++)
        {
            Item item = global::Char.myCharz().arrItemBag[i];
            if (item != null && item.template.id == 212)
            {
                Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                return;
            }
        }
        for (int j = 0; j < global::Char.myCharz().arrItemBag.Length; j++)
        {
            Item item2 = global::Char.myCharz().arrItemBag[j];
            if (item2 != null && item2.template.id == 211)
            {
                Service.gI().useItem(0, 1, (sbyte)item2.indexUI, -1);
                return;
            }
        }
    }
    public static void Update()
    {
        if (AutoTrain.isAutoTrain && GameCanvas.gameTick % 5 == 0)
        {
            AutoTrain.DoIt();
        }
        if (global::Char.myCharz().cStamina <= 5 && GameCanvas.gameTick % 100 == 0)
        {
            AutoTrain.UseGrape();
        }
        if (global::Char.myCharz().meDead && GameCanvas.gameTick % 20 == 0)
        {
            Service.gI().wakeUpFromDead();
            Char.myCharz().liveFromDead();
        }

    }

}
