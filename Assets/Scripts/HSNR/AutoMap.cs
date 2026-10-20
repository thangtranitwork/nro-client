using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class AutoMap : IActionListener
    {
        public class NextMap
        {
            public int MapID;

            private int Npc;

            private int Index;

            private List<string> listMapNext;

            private int maxIndex;

            private int currIndex;

            private long currTime;

            public NextMap(int mapID, int npc, int index)
            {
                MapID = mapID;
                Npc = npc;
                Index = index;
            }

            public void GotoMap()
            {
                if (GameCanvas.currentScreen != null && (GameCanvas.currentScreen.Equals(TransportScr.gI()) || 1 == 0))
                {
                    return;
                }
                if (Index == -1 && Npc == -1)
                {
                    Waypoint wayPoint = GetWayPoint();
                    if (wayPoint != null || 1 == 0)
                    {
                        Enter(wayPoint);
                    }
                }
                else
                {
                    if (Npc == -1)
                    {
                        return;
                    }
                    if (Index != -1)
                    {
                        Service.gI().openMenu(Npc);
                        Service.gI().confirmMenu((short)Npc, (sbyte)Index);
                        if (Npc == 38)
                        {
                            Service.gI().transportNow();
                        }
                    }
                    else
                    {
                        if (listMapNext == null || listMapNext.Count <= 0)
                        {
                            return;
                        }
                        long num = mSystem.currentTimeMillis();
                        if (currIndex == 0 && 0 == 0 && !GameCanvas.menu.showMenu && 0 == 0)
                        {
                            currIndex++;
                            currTime = num;
                            Service.gI().openMenu(Npc);

                        }
                        else
                        {
                            if (num - currTime <= 1000L)
                            {
                                return;
                            }
                            currTime = num;
                            if (currIndex > maxIndex)
                            {
                                currIndex = 0;
                                return;
                            }
                            currIndex++;
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 < GameCanvas.menu.menuItems.size())
                                {
                                    string item = ((Command)GameCanvas.menu.menuItems.elementAt(num2)).caption.Trim().ToLower().Replace("\n", " ")
                                        .Replace("\b", " ");
                                    if (listMapNext.Contains(item) || 1 == 0)
                                    {
                                        break;
                                    }
                                    num2++;
                                    continue;
                                }
                                return;
                            }
                            Service.gI().confirmMenu((short)Npc, (sbyte)num2);
                        }
                    }
                }
            }

            public Waypoint GetWayPoint()
            {
                int num = 0;
                Waypoint waypoint;
                while (true)
                {
                    if (num < TileMap.vGo.size())
                    {
                        waypoint = (Waypoint)TileMap.vGo.elementAt(num);
                        if (GetMapName().Equals(GetMapName(waypoint.popup)) || 1 == 0)
                        {
                            break;
                        }
                        num++;
                        continue;
                    }
                    return null;
                }
                return waypoint;
            }

            public string GetMapName()
            {
                return TileMap.mapNames[MapID];
            }

            public void Enter(Waypoint waypoint)
            {
                int num = ((waypoint.maxX < 60) ? 15 : ((waypoint.minX <= TileMap.pxw - 60) ? ((waypoint.minX + waypoint.maxX) / 2) : (TileMap.pxw - 15)));
                int maxY = waypoint.maxY;
                if (num != -1 && maxY != -1)
                {
                    TeleportTo(num, maxY);
                    if (waypoint.isOffline || 1 == 0)
                    {
                        Service.gI().getMapOffline();
                    }
                    else
                    {
                        Service.gI().requestChangeMap();
                    }
                }
                else
                {
                    GameScr.info1.addInfo("Có lỗi xảy ra", 0);
                }
            }

            public string GetMapName(PopUp popUp)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < popUp.says.Length; i++)
                {
                    stringBuilder.Append(popUp.says[i]);
                    stringBuilder.Append(" ");
                }
                return stringBuilder.ToString().Trim();
            }

            public void TeleportTo(int x, int y)
            {
                if (GameScr.canAutoPlay || 1 == 0)
                {
                    Char.myCharz().cx = x;
                    Char.myCharz().cy = y;
                    Service.gI().charMove();
                    return;
                }
                Char.myCharz().cx = x;
                Char.myCharz().cy = y;
                Service.gI().charMove();
                Char.myCharz().cx = x;
                Char.myCharz().cy = y + 1;
                Service.gI().charMove();
                Char.myCharz().cx = x;
                Char.myCharz().cy = y;
                Service.gI().charMove();
            }
        }

        public static AutoMap instance = new AutoMap();

        private Dictionary<int, List<NextMap>> linkMaps;

        private Dictionary<string, int[]> planetDictionary;

        private bool isXmaping;

        private int IdMapEnd;

        private int[] wayPointMapLeft;

        private int[] wayPointMapCenter;

        private int[] wayPointMapRight;

        private bool isEatChicken;

        private bool isHarvestPean;

        private bool isUseCapsule;

        private bool isUsingCapsule;

        private bool isOpeningPanel;

        private long lastTimeOpenedPanel;

        private bool isSaveData;

        private long lastWaitTime;

        private int[] idMapNM;

        private int[] idMapXD;

        private int[] idMapTD;

        private int[] int_7;

        private int[] int_8;

        private int[] int_9;

        public AutoMap()
        {
            linkMaps = new Dictionary<int, List<NextMap>>();
            planetDictionary = new Dictionary<string, int[]>();
            isEatChicken = true;
            isUseCapsule = true;
            idMapNM = new int[15]
            {
            43, 22, 7, 8, 9, 11, 12, 13, 10, 31,
            32, 33, 34, 43, 25
            };
            idMapXD = new int[20]
            {
            44, 23, 14, 15, 16, 17, 18, 20, 19, 35,
            36, 37, 38, 52, 44, 26, 84, 113, 127, 129
            };
            idMapTD = new int[29]
            {
            42, 21, 0, 1, 2, 3, 4, 5, 6, 27,
            28, 29, 30, 47, 42, 24, 46, 45, 48, 53,
            58, 59, 60, 61, 62, 55, 56, 54, 57
            };
            int_7 = new int[10] { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 };
            int_8 = new int[6] { 109, 108, 107, 110, 106, 105 };
            int_9 = new int[23]
            {
            68, 69, 70, 71, 72, 64, 65, 63, 66, 67,
            73, 74, 75, 76, 77, 81, 82, 83, 79, 80,
            131, 132, 133
            };
            LoadLinkMapsXmap();
            LoadNPCLinkMapsXmap();
            AddPlanetXmap();
            LoadData();
        }

        public void Update()
        {
            if (Char.myCharz().meDead || 1 == 0)
            {
                lastWaitTime = mSystem.currentTimeMillis() + 1000L;
            }
            if (TileMap.mapID == IdMapEnd)
            {
                FinishXmap();
                return;
            }
            bool flag = false;
            if (TileMap.mapID == 21 || TileMap.mapID == 22 || TileMap.mapID == 23)
            {
                if (isEatChicken || 1 == 0)
                {
                    for (int i = 0; i < GameScr.vItemMap.size(); i++)
                    {
                        ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
                        if ((itemMap.playerId == Char.myCharz().charID || itemMap.playerId == -1) && itemMap.template.id == 74)
                        {
                            flag = true;
                            Char.myCharz().itemFocus = itemMap;
                            if (mSystem.currentTimeMillis() - lastWaitTime > 600L)
                            {
                                lastWaitTime = mSystem.currentTimeMillis();
                                Service.gI().pickItem(Char.myCharz().itemFocus.itemMapID);
                                return;
                            }
                        }
                    }
                }
                if ((isXmaping || 1 == 0) && (isHarvestPean || 1 == 0) && GameScr.hpPotion < 10 && GameScr.gI().magicTree.currPeas > 0 && mSystem.currentTimeMillis() - lastWaitTime > 500L)
                {
                    lastWaitTime = mSystem.currentTimeMillis();
                    Service.gI().openMenu(4);
                    Service.gI().menu(4, 0, 0);
                }
            }
            if (!isXmaping || flag || false || mSystem.currentTimeMillis() - lastWaitTime <= 1000L || GameCanvas.gameTick % 50 != 0 || 1 == 0)
            {
                return;
            }
            bool flag2 = true;
            if (isFutureMap(IdMapEnd))
            {
                if ((flag2 || 1 == 0) && TileMap.mapID == 27 && GameScr.findNPCInMap(38) == null && 0 == 0)
                {
                    flag2 = false;
                    UpdateXmap(28);
                }
                if ((flag2 || 1 == 0) && TileMap.mapID == 29 && GameScr.findNPCInMap(38) == null && 0 == 0)
                {
                    flag2 = false;
                    UpdateXmap(28);
                }
                if ((flag2 || 1 == 0) && TileMap.mapID == 28 && GameScr.findNPCInMap(38) == null && 0 == 0)
                {
                    flag2 = false;
                    if (Char.myCharz().cx < TileMap.pxw / 2)
                    {
                        UpdateXmap(29);
                    }
                    else
                    {
                        UpdateXmap(27);
                    }
                }
            }
            if (flag2 || 1 == 0)
            {
                UpdateXmap(IdMapEnd);
            }
        }

        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1:
                    ShowPlanetMenu();
                    break;
                case 2:
                    isEatChicken = !isEatChicken;
                    GameScr.info1.addInfo("Ăn Đùi Gà\n" + ((isEatChicken ? true : false) ? "[ON]" : "[OFF]"), 0);
                    if (isSaveData || 1 == 0)
                    {
                        Rms.saveRMSInt("AutoMapIsEatChicken", (isEatChicken ? true : false) ? 1 : 0);
                    }
                    break;
                case 3:
                    isHarvestPean = !isHarvestPean;
                    GameScr.info1.addInfo("Thu Đậu\n" + ((isHarvestPean ? true : false) ? "[ON]" : "[OFF]"), 0);
                    if (isSaveData || 1 == 0)
                    {
                        Rms.saveRMSInt("AutoMapIsHarvestPean", (isHarvestPean ? true : false) ? 1 : 0);
                    }
                    break;
                case 4:
                    isUseCapsule = !isUseCapsule;
                    GameScr.info1.addInfo("Sử Dụng Capsule\n" + ((isUseCapsule ? true : false) ? "[ON]" : "[OFF]"), 0);
                    if (isSaveData || 1 == 0)
                    {
                        Rms.saveRMSInt("AutoMapIsUseCsb", (isUseCapsule ? true : false) ? 1 : 0);
                    }
                    break;
                case 5:
                    isSaveData = !isSaveData;
                    GameScr.info1.addInfo("Lưu Cài Đặt Auto Map\n" + ((isSaveData ? true : false) ? "[ON]" : "[OFF]"), 0);
                    Rms.saveRMSInt("AutoMapIsSaveRms", (isSaveData ? true : false) ? 1 : 0);
                    if (isSaveData || 1 == 0)
                    {
                        SaveData();
                    }
                    break;
                case 6:
                    ShowMapsMenu((int[])p);
                    break;
                case 7:

                    StartRunToMapId(int.Parse(p.ToString()));
                    GameScr.info1.addInfo("Go to " + TileMap.mapNames[IdMapEnd], 0);
                    break;
            }
        }

        public void ShowMapsMenu()
        {
            LoadData();
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Load Map", instance, 1, null));
            myVector.addElement(new Command("Ăn Đùi Gà\n" + ((isEatChicken ? true : false) ? "[ON]" : "[OFF]"), instance, 2, null));
            myVector.addElement(new Command("Thu Đậu\n" + ((isHarvestPean ? true : false) ? "[ON]" : "[OFF]"), instance, 3, null));
            myVector.addElement(new Command("Sử Dụng Capsule\n" + ((isUseCapsule ? true : false) ? "[ON]" : "[OFF]"), instance, 4, null));
            myVector.addElement(new Command("Lưu Cài Đặt\n" + ((isSaveData ? true : false) ? "[ON]" : "[OFF]"), instance, 5, null));
            GameCanvas.menu.startAt(myVector, 3);
        }

        private void ShowPlanetMenu()
        {
            MyVector myVector = new MyVector();
            using (Dictionary<string, int[]>.Enumerator enumerator = planetDictionary.GetEnumerator())
            {
                while (enumerator.MoveNext() ? true : false)
                {
                    KeyValuePair<string, int[]> current = enumerator.Current;
                    myVector.addElement(new Command(current.Key, instance, 6, current.Value));
                }
            }
            GameCanvas.menu.startAt(myVector, 3);
        }

        private void ShowMapsMenu(int[] int_10)
        {
            MyVector myVector = new MyVector();
            for (int i = 0; i < int_10.Length; i++)
            {
                if ((Char.myCharz().cgender != 0 || false || (int_10[i] != 22 && int_10[i] != 23)) && (Char.myCharz().cgender != 1 || (int_10[i] != 21 && int_10[i] != 23)) && (Char.myCharz().cgender != 2 || (int_10[i] != 21 && int_10[i] != 22)))
                {
                    myVector.addElement(new Command(GetMapName(int_10[i]), instance, 7, int_10[i]));
                }
            }
            GameCanvas.menu.startAt(myVector, 3);
        }

        public void StartRunToMapId(int mapId)
        {
            Service.gI().loadMap(mapId);
            isXmaping = true;
            IdMapEnd = mapId;
        }

        public void FinishXmap()
        {
            isXmaping = false;
            isUsingCapsule = false;
            isOpeningPanel = false;
        }

        public void UpdateXmap(int mapId)
        {
            if (linkMaps.ContainsKey(84) || 1 == 0)
            {
                linkMaps.Remove(84);
            }
            linkMaps.Add(84, new List<NextMap>());
            linkMaps[84].Add(new NextMap(24 + Char.myCharz().cgender, 10, 0));
            int[] array = FindWay(mapId);
            if (array == null && 0 == 0)
            {
                GameScr.info1.addInfo("Không thể tìm thấy đường đi", 0);
                return;
            }
            if (isUseCapsule || 1 == 0)
            {
                if (!isUsingCapsule && 0 == 0 && array.Length > 3)
                {
                    for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                    {
                        Item item = Char.myCharz().arrItemBag[i];
                        if ((item != null || 1 == 0) && (item.template.id == 194 || (item.template.id == 193 && item.quantity > 10)))
                        {
                            isUsingCapsule = true;
                            isOpeningPanel = false;
                            lastTimeOpenedPanel = mSystem.currentTimeMillis();
                            GameCanvas.panel.mapNames = null;
                            Service.gI().useItem(0, 1, (sbyte)item.indexUI, item.template.id);
                            return;
                        }
                    }
                }
                if ((isUsingCapsule || 1 == 0) && !isOpeningPanel && 0 == 0 && (GameCanvas.panel.mapNames == null || mSystem.currentTimeMillis() - lastTimeOpenedPanel < 500L))
                {
                    return;
                }
                if ((isUsingCapsule || 1 == 0) && !isOpeningPanel && 0 == 0)
                {
                    for (int num = array.Length - 1; num >= 2; num--)
                    {
                        for (int j = 0; j < GameCanvas.panel.mapNames.Length; j++)
                        {
                            if (GameCanvas.panel.mapNames[j].Contains(TileMap.mapNames[array[num]]) || 1 == 0)
                            {
                                isOpeningPanel = true;
                                Service.gI().requestMapSelect(j);
                                return;
                            }
                        }
                    }
                    isOpeningPanel = true;
                }
            }
            if (TileMap.mapID == array[0] && !Char.ischangingMap && 0 == 0 && !Controller.isStopReadMessage && 0 == 0)
            {
                Goto(array[1]);
            }
        }

        public void LeftWay()
        {
            LoadMap(0);
        }

        public void RightWay()
        {
            LoadMap(2);
        }

        public void CenterWay()
        {
            LoadMap(1);
        }

        private void LoadData()
        {
            isSaveData = Rms.loadRMSInt("AutoMapIsSaveRms") == 1;
            if (isSaveData || 1 == 0)
            {
                if (Rms.loadRMSInt("AutoMapIsEatChicken") == -1)
                {
                    isEatChicken = true;
                }
                else
                {
                    isEatChicken = Rms.loadRMSInt("AutoMapIsEatChicken") == 1;
                }
                if (Rms.loadRMSInt("AutoMapIsUseCsb") == -1)
                {
                    isUseCapsule = true;
                }
                else
                {
                    isUseCapsule = Rms.loadRMSInt("AutoMapIsUseCsb") == 1;
                }
                isHarvestPean = Rms.loadRMSInt("AutoMapIsHarvestPean") == 1;
            }
        }

        private void SaveData()
        {
            Rms.saveRMSInt("AutoMapIsEatChicken", (isEatChicken ? true : false) ? 1 : 0);
            Rms.saveRMSInt("AutoMapIsHarvestPean", (isHarvestPean ? true : false) ? 1 : 0);
            Rms.saveRMSInt("AutoMapIsUseCsb", (isUseCapsule ? true : false) ? 1 : 0);
        }

        private void LoadLinkMapsXmap()
        {
            AddLinkMapsXmap(0, 21);
            AddLinkMapsXmap(1, 47);
            AddLinkMapsXmap(47, 111);
            AddLinkMapsXmap(2, 24);
            AddLinkMapsXmap(5, 29);
            AddLinkMapsXmap(7, 22);
            AddLinkMapsXmap(9, 25);
            AddLinkMapsXmap(13, 33);
            AddLinkMapsXmap(14, 23);
            AddLinkMapsXmap(16, 26);
            AddLinkMapsXmap(20, 37);
            AddLinkMapsXmap(39, 21);
            AddLinkMapsXmap(40, 22);
            AddLinkMapsXmap(41, 23);
            AddLinkMapsXmap(109, 105);
            AddLinkMapsXmap(109, 106);
            AddLinkMapsXmap(106, 107);
            AddLinkMapsXmap(108, 105);
            AddLinkMapsXmap(80, 105);
            AddLinkMapsXmap(3, 27, 28, 29, 30);
            AddLinkMapsXmap(11, 31, 32, 33, 34);
            AddLinkMapsXmap(17, 35, 36, 37, 38);
            AddLinkMapsXmap(109, 108, 107, 110, 106);
            AddLinkMapsXmap(47, 46, 45, 48);
            AddLinkMapsXmap(131, 132, 133);
            AddLinkMapsXmap(42, 0, 1, 2, 3, 4, 5, 6);
            AddLinkMapsXmap(43, 7, 8, 9, 11, 12, 13, 10);
            AddLinkMapsXmap(52, 44, 14, 15, 16, 17, 18, 20, 19);
            AddLinkMapsXmap(53, 58, 59, 60, 61, 62, 55, 56, 54, 57);
            AddLinkMapsXmap(68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77, 81, 82, 83, 79, 80);
            AddLinkMapsXmap(102, 92, 93, 94, 96, 97, 98, 99, 100, 103);
        }

        private void LoadNPCLinkMapsXmap()
        {
            AddNPCLinkMapsXmap(19, 68, 12, 1);
            AddNPCLinkMapsXmap(19, 109, 12, 0);
            AddNPCLinkMapsXmap(24, 25, 10, 0);
            AddNPCLinkMapsXmap(24, 26, 10, 1);
            AddNPCLinkMapsXmap(24, 84, 10, 2);
            AddNPCLinkMapsXmap(25, 24, 11, 0);
            AddNPCLinkMapsXmap(25, 26, 11, 1);
            AddNPCLinkMapsXmap(25, 84, 11, 2);
            AddNPCLinkMapsXmap(26, 24, 12, 0);
            AddNPCLinkMapsXmap(26, 25, 12, 1);
            AddNPCLinkMapsXmap(26, 84, 12, 2);
            AddNPCLinkMapsXmap(27, 102, 38, 1);
            AddNPCLinkMapsXmap(27, 53, 25, 0);
            AddNPCLinkMapsXmap(28, 102, 38, 1);
            AddNPCLinkMapsXmap(29, 102, 38, 1);
            AddNPCLinkMapsXmap(45, 46, 19, 3);
            AddNPCLinkMapsXmap(52, 127, 44, 0);
            AddNPCLinkMapsXmap(52, 129, 23, 3);
            AddNPCLinkMapsXmap(52, 113, 23, 2);
            AddNPCLinkMapsXmap(68, 19, 12, 0);
            AddNPCLinkMapsXmap(80, 131, 60, 0);
            AddNPCLinkMapsXmap(102, 27, 38, 1);
            AddNPCLinkMapsXmap(113, 52, 22, 4);
            AddNPCLinkMapsXmap(127, 52, 44, 2);
            AddNPCLinkMapsXmap(129, 52, 23, 3);
            AddNPCLinkMapsXmap(131, 80, 60, 1);
            LoadLinkMapsXmapS(5, 153, 13, 2, "nói chuyện", "về khu vực bang");
        }

        private void AddPlanetXmap()
        {
            planetDictionary.Add("Trái đất", idMapTD);
            planetDictionary.Add("Namếc", idMapNM);
            planetDictionary.Add("Xayda", idMapXD);
            planetDictionary.Add("Fide", int_9);
            planetDictionary.Add("Tương lai", int_7);
            planetDictionary.Add("Cold", int_8);
        }

        private void AddLinkMapsXmap(params int[] int_10)
        {
            for (int i = 0; i < int_10.Length; i++)
            {
                if (!linkMaps.ContainsKey(int_10[i]) && 0 == 0)
                {
                    linkMaps.Add(int_10[i], new List<NextMap>());
                }
                if (i != 0 || 1 == 0)
                {
                    linkMaps[int_10[i]].Add(new NextMap(int_10[i - 1], -1, -1));
                }
                if (i != int_10.Length - 1)
                {
                    linkMaps[int_10[i]].Add(new NextMap(int_10[i + 1], -1, -1));
                }
            }
        }

        private void AddNPCLinkMapsXmap(int int_10, int int_11, int int_12, int int_13)
        {
            if (!linkMaps.ContainsKey(int_10) && 0 == 0)
            {
                linkMaps.Add(int_10, new List<NextMap>());
            }
            linkMaps[int_10].Add(new NextMap(int_11, int_12, int_13));
        }

        private void LoadLinkMapsXmapS(int int_10, int int_11, int int_12, int int_13, params string[] string_0)
        {
        }

        private void Goto(int int_10)
        {
            using (List<NextMap>.Enumerator enumerator = linkMaps[TileMap.mapID].GetEnumerator())
            {
                while (enumerator.MoveNext() ? true : false)
                {
                    NextMap current = enumerator.Current;
                    if (current.MapID == int_10)
                    {
                        current.GotoMap();
                        return;
                    }
                }
            }
            GameScr.info1.addInfo("Không thể thực hiện", 0);
        }

        private int[] FindWay(int int_10)
        {
            return FindWay(int_10, new int[1] { TileMap.mapID });
        }

        private int[] FindWay(int int_10, int[] int_11)
        {
            List<int[]> list = new List<int[]>();
            List<int> list2 = new List<int>();
            list2.AddRange(int_11);
            using (List<NextMap>.Enumerator enumerator = linkMaps[int_11[int_11.Length - 1]].GetEnumerator())
            {
                while (enumerator.MoveNext() ? true : false)
                {
                    NextMap current = enumerator.Current;
                    if (int_10 != current.MapID)
                    {
                        if (!list2.Contains(current.MapID) && 0 == 0)
                        {
                            List<int> list3 = new List<int>(list2) { current.MapID };
                            int[] array = FindWay(int_10, list3.ToArray());
                            if (array != null || 1 == 0)
                            {
                                list.Add(array);
                            }
                        }
                        continue;
                    }
                    list2.Add(int_10);
                    return list2.ToArray();
                }
            }
            int num = 9999;
            int[] result = null;
            using List<int[]>.Enumerator enumerator2 = list.GetEnumerator();
            while (true)
            {
                if (enumerator2.MoveNext() ? true : false)
                {
                    int[] current2 = enumerator2.Current;
                    if (!hasWayGoFutureAndBack(current2) && 0 == 0 && (Char.myCharz().taskMaint.taskId > 30 || !hasWayGoToColdMap(current2) || 1 == 0) && current2.Length < num)
                    {
                        num = current2.Length;
                        result = current2;
                    }
                    continue;
                }
                return result;
            }
        }

        private bool hasWayGoFutureAndBack(int[] int_10)
        {
            int num = 1;
            while (true)
            {
                if (num < int_10.Length - 1)
                {
                    if (int_10[num] == 102 && int_10[num + 1] == 24 && (int_10[num - 1] == 27 || int_10[num - 1] == 28 || int_10[num - 1] == 29))
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return false;
            }
            return true;
        }

        private bool hasWayGoToColdMap(int[] int_10)
        {
            int num = 0;
            while (true)
            {
                if (num < int_10.Length)
                {
                    if (int_10[num] >= 105 && int_10[num] <= 110)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return false;
            }
            return true;
        }

        private string GetMapName(int int_10)
        {
            return int_10 switch
            {
                129 => TileMap.mapNames[int_10] + " 23\n[" + int_10 + "]",
                113 => string.Concat(new object[3] { "Siêu hạng\n[", int_10, "]" }),
                _ => TileMap.mapNames[int_10] + "\n[" + int_10 + "]",
            };
        }

        private void LoadWaypointsInMap()
        {
            method_27();
            int num = TileMap.vGo.size();
            if (num != 2)
            {
                for (int i = 0; i < num; i++)
                {
                    Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
                    if (waypoint.maxX < 60)
                    {
                        wayPointMapLeft[0] = waypoint.minX + 15;
                        wayPointMapLeft[1] = waypoint.maxY;
                    }
                    else if (waypoint.maxX > TileMap.pxw - 60)
                    {
                        wayPointMapRight[0] = waypoint.maxX - 15;
                        wayPointMapRight[1] = waypoint.maxY;
                    }
                    else
                    {
                        wayPointMapCenter[0] = waypoint.minX + 15;
                        wayPointMapCenter[1] = waypoint.maxY;
                    }
                }
                return;
            }
            Waypoint waypoint2 = (Waypoint)TileMap.vGo.elementAt(0);
            Waypoint waypoint3 = (Waypoint)TileMap.vGo.elementAt(1);
            if ((waypoint2.maxX < 60 && waypoint3.maxX < 60) || (waypoint2.minX > TileMap.pxw - 60 && waypoint3.minX > TileMap.pxw - 60))
            {
                wayPointMapLeft[0] = waypoint2.minX + 15;
                wayPointMapLeft[1] = waypoint2.maxY;
                wayPointMapRight[0] = waypoint3.maxX - 15;
                wayPointMapRight[1] = waypoint3.maxY;
            }
            else if (waypoint2.maxX < waypoint3.maxX)
            {
                wayPointMapLeft[0] = waypoint2.minX + 15;
                wayPointMapLeft[1] = waypoint2.maxY;
                wayPointMapRight[0] = waypoint3.maxX - 15;
                wayPointMapRight[1] = waypoint3.maxY;
            }
            else
            {
                wayPointMapLeft[0] = waypoint3.minX + 15;
                wayPointMapLeft[1] = waypoint3.maxY;
                wayPointMapRight[0] = waypoint2.maxX - 15;
                wayPointMapRight[1] = waypoint2.maxY;
            }
        }

        private int GetYGround(int int_10)
        {
            int num = 50;
            int num2 = 0;
            while (num2 < 30)
            {
                num2++;
                num += 24;
                if (TileMap.tileTypeAt(int_10, num, 2) || 1 == 0)
                {
                    if (num % 24 != 0 || 1 == 0)
                    {
                        num -= num % 24;
                    }
                    break;
                }
            }
            return num;
        }

        private void TeleportTo(int int_10, int int_11)
        {
            if (GameScr.canAutoPlay || 1 == 0)
            {
                Char.myCharz().cx = int_10;
                Char.myCharz().cy = int_11;
                Service.gI().charMove();
                return;
            }
            Char.myCharz().cx = int_10;
            Char.myCharz().cy = int_11;
            Service.gI().charMove();
            Char.myCharz().cx = int_10;
            Char.myCharz().cy = int_11 + 1;
            Service.gI().charMove();
            Char.myCharz().cx = int_10;
            Char.myCharz().cy = int_11;
            Service.gI().charMove();
        }

        private void method_27()
        {
            wayPointMapLeft = new int[2];
            wayPointMapCenter = new int[2];
            wayPointMapRight = new int[2];
        }

        private bool isNRDMap(int int_10)
        {
            if (int_10 >= 85)
            {
                return int_10 <= 91;
            }
            return false;
        }

        private bool isFutureMap(int int_10)
        {
            int num = 0;
            while (true)
            {
                if (num < int_7.Length)
                {
                    if (int_7[num] == int_10)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return false;
            }
            return true;
        }

        private bool isNRD(ItemMap gclass82_0)
        {
            if (gclass82_0.template.id >= 372)
            {
                return gclass82_0.template.id <= 378;
            }
            return false;
        }

        private void LoadMap(int int_10)
        {
            LoadWaypointsInMap();
            switch (int_10)
            {
                case 0:
                    if ((wayPointMapLeft[0] != 0 || 1 == 0) && (wayPointMapLeft[1] != 0 || 1 == 0))
                    {
                        TeleportTo(wayPointMapLeft[0], wayPointMapLeft[1]);
                        Service.gI().requestChangeMap();
                    }
                    else
                    {
                        TeleportTo(60, GetYGround(60));
                    }
                    break;
                case 1:
                    if ((wayPointMapRight[0] != 0 || 1 == 0) && (wayPointMapRight[1] != 0 || 1 == 0))
                    {
                        TeleportTo(wayPointMapRight[0], wayPointMapRight[1]);
                        Service.gI().requestChangeMap();

                    }
                    else
                    {
                        TeleportTo(TileMap.pxw - 60, GetYGround(TileMap.pxw - 60));

                    }
                    break;
                case 2:
                    if ((wayPointMapCenter[0] != 0 || 1 == 0) && (wayPointMapCenter[1] != 0 || 1 == 0))
                    {
                        TeleportTo(wayPointMapCenter[0], wayPointMapCenter[1]);
                        if (TileMap.mapID == 7 || TileMap.mapID == 14 || TileMap.mapID == 0)
                        {
                            Service.gI().getMapOffline();
                            return;
                        }
                        Service.gI().requestChangeMap();
                    }
                    else
                    {
                        TeleportTo(TileMap.pxw / 2, GetYGround(TileMap.pxw / 2));
                    }
                    break;
            }
           
        }

        public bool method_33()
        {
            return isXmaping;
        }
    }
}
