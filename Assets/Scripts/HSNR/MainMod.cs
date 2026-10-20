using Assets.Scripts.HSNR;
using Assets.Scripts.VoiceChat;
using System;
using UnityEngine;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class MainMod : IActionListener
    {
        private static MainMod instance;
        public static bool isAutoRevive;
        private Button[] button = new Button[13];
        private Image settingMod = GameCanvas.loadImage("/mainImage/setting.png");


        public static readonly string[] ARR_MOD_NAMES = new string[]
        {
            "Tàn Sát (Auto Train)",
            "Tự Đánh",
            "Tự Đánh VIP",
            "Auto Hồi Sinh",
            "Auto Nhặt",
            "Auto Nhặt NRSD",
            "Auto Buff Đậu",
            "Thông báo BOSS",
            "Nhân vật trong Map",
            "Auto Focus vào Boss",
            "Auto Chat Map",
            "Auto Chat Thế Giới"

        };
        public static readonly string[] ARR_TITLE_NAMES = new string[]
        {
            "tàn sát (Auto train)",
            "tự động đánh",
            "tự động đánh",
            "tự động hồi sinh",
            "tự động nhặt",
            "tự động nhặt NRSD",
            "Auto Buff Đậu",
            "xem thông báo Boss",
            "xem nhân vật trong Map",
            "auto focus vào Boss",
            string.Empty,
            string.Empty
        };
       public static bool isPickNRSD;
        public static bool[] getStatusModFields() => new bool[]
        {
            AutoTrainVIP.isAutoTrain,
            AutoSkill.isAutoSendAttack == 1,
            AutoSkill.isAutoSendAttack == 2,
            isAutoRevive,
            AutoPick.isAutoPick,
            isPickNRSD,
            AutoPean.isAutoBuffPean,
            Boss.gI().isShow,
            ListChars.gI().isShow,
            autoFocus,
            AutoChat.enableChatMap,
             AutoChat.enableChatGlobal,
        };


        public static void changeStatus(ref bool input, string status)
        {
            input = !input;
            GameScr.info1.addInfo((input ? "Đã bật " : "Đã tắt ") + status, 0);
        }

        public static MainMod gI()
        {
            if (instance == null)
            {
                instance = new MainMod();
            }
            return instance;

        }
        public MainMod()
        {
            button = new Button[13]
               {
                    new Button("J", this, 12),
                    new Button("K", this, 13),
                    new Button("L", this, 14),
                    new Button("A", this, 15),
                    new Button("F", this, 16),
                    new Button("E", this, 17),
                    new Button("G", this, 18),
                    new Button("N", this, 19),
                    new Button("X", this, 20),
                    new Button("T", this, 23),
                    new Button("S", this, 24),
                    new Button("C", this, 21),
                    new Button("M", this, 22),
               };
        }
        public void Paint(mGraphics g)
        {
            try
            {
                string mapInfo = string.Concat(new object[]
         {
                $"Map: {TileMap.mapNames[TileMap.mapID]} [{TileMap.zoneID}]\n",
                $"Time: {DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")}"
         });
                mFont.tahoma_7b_white.drawString(g, mapInfo, 20, GameCanvas.hh / 2 + 10, mFont.LEFT, mFont.tahoma_7b_dark);
            }
            catch
            {

            }
            string strHP = NinjaUtil.getMoneys(Char.myCharz().cHP);
            string strMP = NinjaUtil.getMoneys(Char.myCharz().cMP);
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.normal.textColor = Color.yellow;
            mFont.tahoma_7b_red.drawString(g, $"FPS: {((byte)(Main.main.max)).ToString()}", 85, 30, 0, mFont.tahoma_7b_dark);
            long ping = Service.gI().delayPing;
            mFont font = ping < 50 ? mFont.tahoma_7b_green : ping < 100 ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_red;
            font.drawString(g, $"{ping.ToString()} ms", 85, 42, 0, mFont.tahoma_7b_dark);
            guiStyle.fontSize = 8 * mGraphics.zoomLevel;

            g.drawString(strHP, 85, 4, guiStyle);
            guiStyle.fontSize = 7 * mGraphics.zoomLevel;
            g.drawString(strMP, 85, 19, guiStyle);
            paintCharInfo(g, Char.myCharz());
            ListChars.gI().paintListCharsInMap(g);
            //Boss.gI().paintBossesScreen(g);
            paintModStatus(g);
            paintButton(g);
            if (Boss.gI().isShow)
            {
                NQMP.NQMPMain.aaMod.paintBoss(g);
            }
            if (VoiceEnable.enableLoaBang || VoiceEnable.enableLoaMap)
            {
                if (VoiceChatManager.gI().namePlayer != "")
                {
                    mFont.tahoma_7b_yellow.drawString(g, $"[{VoiceChatManager.gI().namePlayer}] đang nói", 5, GameCanvas.h - 10, mFont.LEFT, mFont.tahoma_7b_dark);
                }
            }
            if (Char.myCharz().havePet && MoreSetting.showDiscipleInfo)
            {
                int num = 25;
                if (GameCanvas.gameTick % 30 == 0)
                {
                    Service.gI().petInfo();
                }
                mFont.tahoma_7b_white.drawString(g, "Đệ Tử :", num, GameCanvas.h - 170, mFont.LEFT,mFont.tahoma_7b_dark);
                mFont.tahoma_7b_white.drawString(g, "Sức Mạnh : " + NinjaUtil.getMoneys(Char.myPetz().cPower), num, GameCanvas.h - 160, mFont.LEFT, mFont.tahoma_7b_dark);
                mFont.tahoma_7b_white.drawString(g, "Tiềm Năng : " + NinjaUtil.getMoneys(Char.myPetz().cTiemNang), num, GameCanvas.h - 150, mFont.LEFT, mFont.tahoma_7b_dark);
                mFont.tahoma_7b_white.drawString(g, string.Concat(new object[]
                {
                "Sức Đánh : ",
                NinjaUtil.getMoneys(Char.myPetz().cDamFull),
                "  Giáp : ",
                Char.myPetz().cDefull
                }), num, GameCanvas.h - 140, mFont.LEFT, mFont.tahoma_7b_dark);
                mFont.tahoma_7b_white.drawString(g, "HP : " + NinjaUtil.getMoneys(global::Char.myPetz().cHP), num, GameCanvas.h - 130, mFont.LEFT, mFont.tahoma_7b_dark);
                mFont.tahoma_7b_white.drawString(g, "MP : " + NinjaUtil.getMoneys(global::Char.myPetz().cMP), num, GameCanvas.h - 120, mFont.LEFT, mFont.tahoma_7b_dark);
            }
        }
        private void paintButton(mGraphics g)
        {
            if (settingMod != null)
            {
                if (GameCanvas.panel != null && !GameCanvas.panel.isShow)
                {
                    g.drawImage(settingMod, 150, 0);              
                    if (GameCanvas.isPointerHoldIn(150, 0, settingMod.getWidth(), settingMod.getHeight()) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        GameCanvas.panel.setTypeMod(false);
                        GameCanvas.panel.show();
                        GameCanvas.clearAllPointerEvent();
                    }
                }
            }
            if (!Panel.isShowPhimTat)
            {
                return;
            }
            if (GameScr.isAnalog == 1 && GameCanvas.isTouch && !ChatTextField.gI().isShow && !GameCanvas.menu.showMenu && !GameCanvas.panel.isShow && !Char.isLoadingMap && !Char.myCharz().meDead)
            {
                if (button != null)
                {

                    for (int i = 0; i < button.Length; i++)
                    {
                        if (i <= 2)
                        {
                            if (button[i] != null)
                            {
                                button[i].x = 20 + 23 * i;
                                button[i].y = GameCanvas.h - 105;
                                button[i].Paint(g);
                            }
                        }
                        else if (i >= 3 && i <= 10)
                        {
                            if (button[i] != null)
                            {

                                button[i].x = ((GameCanvas.w / 2) - (30 * 6)) + 26 * i;
                                button[i].y = GameScr.ySkill - GameScr.imgSkill.getHeight() / 2 - 40;
                                button[i].Paint(g);
                            }
                        }
                        else
                        {
                            if (button[i] != null)
                            {
                                button[i].x = GameScr.xTG + 25 * (i - 13);
                                button[i].y = GameScr.yTG - 30;
                                button[i].Paint(g);
                            }
                        }
                    }
                }
            }
        }
        static Char holdNRSD()
        {
            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                if (c != null && !c.isPet && !c.isMiniPet)
                {
                    if (c.bag >= 61 && c.bag <= 66)
                    {
                        return c;
                    }
                }
            }
            return null;
        }
        private void paintCharInfo(mGraphics g, Char c)
        {
            string strInfo = string.Empty;
            if (c.mobFocus != null)
            {
                strInfo = $"{c.mobFocus.getTemplate().name}\n[{NinjaUtil.getMoneys(c.mobFocus.hp)}/{NinjaUtil.getMoneys(c.mobFocus.getTemplate().hp)}]";
            }
            else if (holdNRSD() != null)
            {
                c = holdNRSD();
                strInfo = $"{c.cName}\n{NinjaUtil.getMoneys(c.cHP)}/{NinjaUtil.getMoneys(c.cHPFull)}";
                if (c.clanID != -1)
                {
                    strInfo += $"\nClanID: {c.clanID}";
                }
            }
            else if (c.charFocus != null && !c.charFocus.isPet && !c.charFocus.isMiniPet)
            {
                strInfo = $"{c.charFocus.cName}\n{NinjaUtil.getMoneys(c.charFocus.cHP)}/{NinjaUtil.getMoneys(c.charFocus.cHPFull)}";
                if (c.charFocus.clanID != -1)
                {
                    strInfo += $"\nClanID: {c.charFocus.clanID}";
                }
            }
            if (!string.IsNullOrEmpty(strInfo))
                mFont.tahoma_7b_red.drawString(g, strInfo, GameCanvas.w / 2, 60, mGraphics.VCENTER | mGraphics.HCENTER, mFont.tahoma_7b_dark);
        }
        private void paintModStatus(mGraphics g)
        {
            int num = GameCanvas.hh / 2 + 30;
            if (AutoSkill.isAutoSendAttack == 1)
            {
                mFont.tahoma_7b_white.drawString(g, "Tự đánh: on", 20, num, 0, mFont.tahoma_7b_dark);
                num += 10;
            }
            if (AutoSkill.isAutoSendAttack == 2)
            {
                mFont.tahoma_7b_white.drawString(g, "Tự đánh VIP: on", 20, num, 0, mFont.tahoma_7b_dark);
                num += 10;
            }
            if (isAutoRevive)
            {
                mFont.tahoma_7b_white.drawString(g, "Hồi sinh: on", 20, num, 0, mFont.tahoma_7b_dark);
                num += 10;
            }
            if (autoFocus)
            {
                mFont.tahoma_7b_white.drawString(g, "Auto FocusBoss: on", 20, num, 0, mFont.tahoma_7b_dark);
                num += 10;
            }
            if (AutoPick.isAutoPick)
            {
                mFont.tahoma_7b_white.drawString(g, "Auto nhặt: on", 20, num, 0, mFont.tahoma_7b_dark);
                num += 10;
            }
        }
        public void Update()
        {
            try
            {
                AutoChat.update();
                ListChars.gI().Update();
                AutoItem.gI().Update();
                AutoMap.instance.Update();
                AutoSkill.Update();
                AutoPean.Update();
                AutoPick.Update();
                AutoTrainVIP.Update();
                focusBoss();
                if (isAutoRevive)
                {
                    if (Char.myCharz().meDead && GameCanvas.gameTick % 20 == 0)
                    {
                        if (Char.myCharz().luong > 0)
                        {
                            Service.gI().wakeUpFromDead();
                            Char.myCharz().liveFromDead();
                        }
                    }
                }
                if (mSystem.currentTimeMillis() - Service.gI().lastRequestPing >= 1000L)
                {
                    Service.gI().pingToServer();
                }
                if (GameCanvas.gameTick % 15 == 0 && MoreSetting.autoUpdateArea)
                {
                    Service.gI().openUIZone();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        public void UpdateKey()
        {

            if (Panel.isShowPhimTat)
            {
                if (GameScr.isAnalog == 1 && GameCanvas.isTouch && !ChatTextField.gI().isShow && !GameCanvas.menu.showMenu && !GameCanvas.panel.isShow && !Char.isLoadingMap && !Char.myCharz().meDead)
                {
                    if (button != null)
                    {
                        for (int i = 0; i < button.Length; i++)
                        {
                            if (button[i] != null && button[i].isPointerPressInside())
                            {
                                button[i].performAction();
                            }
                        }
                    }
                }
            }
            ListChars.gI().UpdateKey();
        }
        public void HotKey()
        {
            switch (GameCanvas.keyAsciiPress)
            {
                case 'q':
                    GameCanvas.panel.setTypeMod(false);
                    GameCanvas.panel.show();
                    break;
                case 'x':
                    showMenu();
                    break;
                case 'j':
                    AutoMap.instance.LeftWay();
                    break;
                case 'l':
                    AutoMap.instance.CenterWay();
                    break;
                case 'k':
                    AutoMap.instance.RightWay();
                    break;
                case 'a':
                    AutoSkill.getInstance().perform(1, null);
                    break;
                case 'b':
                    Service.gI().friend(0, -1);
                    break;
                case 'c':
                    for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                    {
                        Item item = Char.myCharz().arrItemBag[i];
                        if (item != null && (item.template.id == 194 || item.template.id == 193 || item.template.id == 2309))
                        {
                            Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                            break;
                        }
                    }
                    break;
                case 'd':
                    AutoSkill.FreezeSelectedSkill();
                    break;
                case 'e':
                    isAutoRevive = !isAutoRevive;
                    GameScr.info1.addInfo("Auto Hồi Sinh\n" + (isAutoRevive ? "[ON]" : "[OFF]"), 0);
                    break;
                case 'f':
                    bool isNhapThe = Char.myCharz().isNhapThe;
                    if (GameUtils.gI().IsExistItem(454))
                    {
                        GameUtils.gI().UseItem(454);
                    }
                    if (GameUtils.gI().IsExistItem(921))
                    {
                        GameUtils.gI().UseItem(921);
                    }
                    if (GameUtils.gI().IsExistItem(2379))
                    {
                        GameUtils.gI().UseItem(2379);
                    }
                    //if (isNhapThe)
                    //    Service.gI().petStatus(3);
                    break;
                case 'g':
                    if (Char.myCharz().charFocus == null)
                        GameScr.info1.addInfo("Vui Lòng Chọn Mục Tiêu!", 0);
                    else
                    {
                        Service.gI().giaodich(0, Char.myCharz().charFocus.charID, -1, -1);
                        GameScr.info1.addInfo("Đã Gửi Lời Mời Giao Dịch Đến: " + Char.myCharz().charFocus.subcName(), 0);
                    }
                    break;
                case 'm':
                    Service.gI().openUIZone();
                    GameCanvas.panel.setTypeZone();
                    GameCanvas.panel.show();
                    break;
                case 'n':
                    AutoPick.getInstance().perform(1, null);
                    break;
                case 't':
                    AutoTrainVIP.ShowMenu();
                    break;
                case 's':
                    autoFocus = !autoFocus;
                    GameScr.info1.addInfo("Auto Focus Boss\n" + (autoFocus ? "[ON]" : "[OFF]"), 0);
                    break;
            }
        }
        public static bool autoFocus;
        public static void focusBoss()
        {
            if (autoFocus)
            {
                if (Char.myCharz().charFocus != null && isBoss(Char.myCharz().charFocus))
                {
                    if (Char.myCharz().charFocus.isDie || Char.myCharz().charFocus.cHP <= 0)
                    {

                    }
                    else
                    {
                        return;
                    }
                }
                for (int j = 0; j < GameScr.vCharInMap.size(); j++)
                {
                    Char @char = (Char)GameScr.vCharInMap.elementAt(j);
                    if (!@char.cName.Equals("") && isBoss(@char) && (Char.myCharz().charFocus == null || (Char.myCharz().charFocus != null && Char.myCharz().charFocus.cName != @char.cName)))
                    {
                        if (@char.isDie || @char.cHP <= 0)
                        {
                            continue;
                        }
                        resetFocus();
                        Char.myCharz().charFocus = @char;
                        break;
                    }
                }
            }
        }
        public static void resetFocus()
        {
            Char.myCharz().mobFocus = null;
            Char.myCharz().npcFocus = null;
            Char.myCharz().itemFocus = null;
            Char.myCharz().charFocus = null;
        }
        public static bool isBoss(Char ch)
        {
            if (ch.cName != null && ch.cName != "" && !ch.isPet && !ch.isMiniPet && char.IsUpper(char.Parse(ch.cName.Substring(0, 1))) && ch.cName != "Trọng tài" && !ch.cName.StartsWith("#"))
                return !ch.cName.StartsWith("$");
            return false;
        }
        public void setPosBoolean()
        {
            Rms.saveRMSInt("sanboss", 1);
            Rms.saveRMSInt("showchar", 1);
        }
        public void loadData()
        {
            Boss.gI().isShow = Rms.loadRMSInt("sanboss") == 1;
            ListChars.gI().isShow = Rms.loadRMSInt("showchar") == 1;
            //HackInjectionDetector.init();
        }
        private void showMenu()
        {
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Auto Map", this, 1, null));
            myVector.addElement(new Command("Auto Skill", this, 2, null));
            myVector.addElement(new Command("Auto Pean", this, 3, null));
            myVector.addElement(new Command("Auto Pick", this, 4, null));
            myVector.addElement(new Command("Auto Train", this, 5, null));
            myVector.addElement(new Command("More", this, 7, null));
            GameCanvas.menu.startAt(myVector, 0);
        }
        private void showMoreMenu()
        {
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Đồ Họa", this, 8, null));
            GameCanvas.menu.startAt(myVector, 0);
        }
        private void showGraphicsMenu()
        {
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Thông Báo\nBoss\n" + (Boss.gI().isShow ? "[ON]" : "[OFF]"), this, 9, null));
            myVector.addElement(new Command("Danh Sách\nNhân Vật\nTrong Khu\n" + (ListChars.gI().isShow ? "[ON]" : "[OFF]"), this, 10, null));

            GameCanvas.menu.startAt(myVector, 0);
        }
        public bool Chat(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            text = text.Trim(); // Remove whitespace

            if (text.StartsWith("k"))
            {
                string zoneStr = text.Substring(1);
                if (int.TryParse(zoneStr, out int zone))
                {
                    Service.gI().requestChangeZone(zone, -1);
                    return true;
                }
                return false;
            }
            else if (text.StartsWith("cheat"))
            {
                string cheatStr = text.Substring(5);
                if (int.TryParse(cheatStr, out int cheat))
                {
                    if (cheat >= 0 && cheat <= 10)
                    {
                        Time.timeScale = cheat;
                        GameScr.info1.addInfo($"Cheat: {cheat}", 0);
                    }
                    else
                    {
                        GameScr.info1.addInfo("Cheat phải từ 0-10", 0);
                    }
                    return true;
                }
                return false;
            }
            else if (text.StartsWith("cheatf"))
            {
                string cheatStr = text.Substring(6);
                if (int.TryParse(cheatStr, out int cheatValue))
                {
                    float cheat = cheatValue / 10.0f;
                    if (cheat >= 0 && cheat <= 10)
                    {
                        Time.timeScale = cheat;
                        GameScr.info1.addInfo($"Cheat: {cheat:F1}", 0);
                    }
                    else
                    {
                        GameScr.info1.addInfo("Cheat phải từ 0-100 (0.0-10.0)", 0);
                    }
                    return true;
                }
                return false;
            }
            else if (text.StartsWith("s"))
            {
                string speedStr = text.Substring(1);
                if (int.TryParse(speedStr, out int speed))
                {
                    if (speed >= 0 && speed <= 20) // Add reasonable limits
                    {
                        Char.myCharz().cspeed = speed;
                        GameScr.info1.addInfo($"Speed set to {speed}", 0);
                    }
                    else
                    {
                        GameScr.info1.addInfo("Speed phải từ 0-20", 0);
                    }
                    return true;
                }
                return false;
            }

            return false;
        }
        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1:
                    AutoMap.instance.ShowMapsMenu();
                    break;
                case 2:
                    AutoSkill.ShowMenu();
                    break;
                case 3:
                    AutoPean.ShowMenu();
                    break;
                case 4:
                    AutoPick.getInstance().perform(1, null);
                    break;
                case 5:
                    AutoTrainVIP.ShowMenu();
                    break;
                case 7:
                    showMoreMenu();
                    break;
                case 8:
                    showGraphicsMenu();
                    break;
                case 9:
                    Boss.gI().isShow = !Boss.gI().isShow;
                    Rms.saveRMSInt("sanboss", Boss.gI().isShow ? 1 : 0);
                    break;
                case 10:
                    ListChars.gI().isShow = !ListChars.gI().isShow;
                    Rms.saveRMSInt("showchar", ListChars.gI().isShow ? 1 : 0);
                    break;



            }
            if (idAction >= 12 && idAction <= 24)
            {
                char[] nChar = new char[] { 'j', 'k', 'l', 'a', 'f', 'e', 'g', 'n', 'x', 'c', 'm', 't', 's' };
                GameCanvas.keyAsciiPress = nChar[idAction - 12];
            }
        }
    }
}
