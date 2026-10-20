using System;
using System.Collections.Generic;
using System.Text;

namespace NQMP
{
    public class LoadMap : IActionListener
    {
        public static LoadMap getInstance()
        {
            if (NQMP.LoadMap._Instance == null)
            {
                NQMP.LoadMap._Instance = new LoadMap();
            }
            return NQMP.LoadMap._Instance;
        }

        // Token: 0x06000A19 RID: 2585 RVA: 0x0008AD88 File Offset: 0x00088F88
        public static void update()
        {
            if (global::Char.myCharz().meDead)
            {
                NQMP.LoadMap.lastWaitTime = mSystem.currentTimeMillis() + 1000L;
            }
            if (TileMap.mapID == NQMP.LoadMap.idMapGoTo)
            {
                NQMP.LoadMap.ResetStatus();
                return;
            }
            bool flag = false;
            if (TileMap.mapID == 21 || TileMap.mapID == 22 || TileMap.mapID == 23)
            {
                if (NQMP.LoadMap.isEatChicken)
                {
                    for (int i = 0; i < GameScr.vItemMap.size(); i++)
                    {
                        ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
                        if ((itemMap.playerId == global::Char.myCharz().charID || itemMap.playerId == -1) && itemMap.template.id == 74)
                        {
                            flag = true;
                            global::Char.myCharz().itemFocus = itemMap;
                            if (mSystem.currentTimeMillis() - NQMP.LoadMap.lastWaitTime > 600L)
                            {
                                NQMP.LoadMap.lastWaitTime = mSystem.currentTimeMillis();
                                Service.gI().pickItem(global::Char.myCharz().itemFocus.itemMapID);
                                return;
                            }
                        }
                    }
                }
                if (NQMP.LoadMap.isXmaping && NQMP.LoadMap.isHarvestPean && GameScr.hpPotion < 10 && GameScr.gI().magicTree.currPeas > 0 && mSystem.currentTimeMillis() - NQMP.LoadMap.lastWaitTime > 500L)
                {
                    NQMP.LoadMap.lastWaitTime = mSystem.currentTimeMillis();
                    Service.gI().openMenu(4);
                    Service.gI().menu(4, 0, 0);
                }
            }
            if (NQMP.LoadMap.isXmaping && !flag && mSystem.currentTimeMillis() - NQMP.LoadMap.lastWaitTime > 1000L && GameCanvas.gameTick % 50 == 0)
            {
                bool flag2 = true;
                if (NQMP.LoadMap.isFutureMap(NQMP.LoadMap.idMapGoTo))
                {
                    if (flag2 && TileMap.mapID == 27 && GameScr.findNPCInMap(38) == null)
                    {
                        flag2 = false;
                        NQMP.LoadMap.StartRunToMapId(28);
                    }
                    if (flag2 && TileMap.mapID == 29 && GameScr.findNPCInMap(38) == null)
                    {
                        flag2 = false;
                        NQMP.LoadMap.StartRunToMapId(28);
                    }
                    if (flag2 && TileMap.mapID == 28 && GameScr.findNPCInMap(38) == null)
                    {
                        flag2 = false;
                        if (global::Char.myCharz().cx < TileMap.pxw / 2)
                        {
                            NQMP.LoadMap.StartRunToMapId(29);
                        }
                        else
                        {
                            NQMP.LoadMap.StartRunToMapId(27);
                        }
                    }
                }
                if (flag2)
                {
                    NQMP.LoadMap.StartRunToMapId(NQMP.LoadMap.idMapGoTo);
                }
            }
        }

        // Token: 0x06000A1A RID: 2586 RVA: 0x0008AFB4 File Offset: 0x000891B4
        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1:
                    NQMP.LoadMap.ShowPlanetMenu();
                    return;
                case 2:
                    NQMP.LoadMap.isEatChicken = !NQMP.LoadMap.isEatChicken;
                    GameScr.info1.addInfo("Ăn Đùi Gà\n" + (NQMP.LoadMap.isEatChicken ? "[STATUS: ON]" : "[STATUS: OFF]"), 0);
                    if (NQMP.LoadMap.isSaveData)
                    {
                        Rms.saveRMSInt("AutoMapIsEatChicken", NQMP.LoadMap.isEatChicken ? 1 : 0);
                    }
                    return;
                case 3:
                    NQMP.LoadMap.isHarvestPean = !NQMP.LoadMap.isHarvestPean;
                    GameScr.info1.addInfo("Thu Đậu\n" + (NQMP.LoadMap.isHarvestPean ? "[STATUS: ON]" : "[STATUS: OFF]"), 0);
                    if (NQMP.LoadMap.isSaveData)
                    {
                        Rms.saveRMSInt("AutoMapIsHarvestPean", NQMP.LoadMap.isHarvestPean ? 1 : 0);
                    }
                    return;
                case 4:
                    NQMP.LoadMap.isUseCapsule = !NQMP.LoadMap.isUseCapsule;
                    GameScr.info1.addInfo("Sử Dụng Capsule\n" + (NQMP.LoadMap.isUseCapsule ? "[STATUS: ON]" : "[STATUS: OFF]"), 0);
                    if (NQMP.LoadMap.isSaveData)
                    {
                        Rms.saveRMSInt("AutoMapIsUseCsb", NQMP.LoadMap.isUseCapsule ? 1 : 0);
                    }
                    return;
                case 5:
                    NQMP.LoadMap.isSaveData = !NQMP.LoadMap.isSaveData;
                    GameScr.info1.addInfo("Lưu Cài Đặt Auto Map\n" + (NQMP.LoadMap.isSaveData ? "[STATUS: ON]" : "[STATUS: OFF]"), 0);
                    Rms.saveRMSInt("AutoMapIsSaveRms", NQMP.LoadMap.isSaveData ? 1 : 0);
                    if (NQMP.LoadMap.isSaveData)
                    {
                        NQMP.LoadMap.SaveData();
                    }
                    return;
                case 6:
                    NQMP.LoadMap.ShowMapsMenu((int[])p);
                    return;
                case 7:
                    NQMP.LoadMap.isXmaping = true;
                    NQMP.LoadMap.idMapGoTo = (int)p;
                    GameScr.info1.addInfo("Go to " + TileMap.mapNames[NQMP.LoadMap.idMapGoTo], 0);
                    return;
                default:
                    return;
            }
        }

        // Token: 0x06000A1B RID: 2587 RVA: 0x0008B178 File Offset: 0x00089378
        public static void ShowMenu()
        {
            NQMP.LoadMap.LoadData();
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Load Map", NQMP.LoadMap.getInstance(), 1, null));
            myVector.addElement(new Command("Ăn Đùi Gà\n" + (NQMP.LoadMap.isEatChicken ? "[STATUS: ON]" : "[STATUS: OFF]"), NQMP.LoadMap.getInstance(), 2, null));
            myVector.addElement(new Command("Thu Đậu\n" + (NQMP.LoadMap.isHarvestPean ? "[STATUS: ON]" : "[STATUS: OFF]"), NQMP.LoadMap.getInstance(), 3, null));
            myVector.addElement(new Command("Sử Dụng Capsule\n" + (NQMP.LoadMap.isUseCapsule ? "[STATUS: ON]" : "[STATUS: OFF]"), NQMP.LoadMap.getInstance(), 4, null));
            myVector.addElement(new Command("Lưu Cài Đặt\n" + (NQMP.LoadMap.isSaveData ? "[STATUS: ON]" : "[STATUS: OFF]"), NQMP.LoadMap.getInstance(), 5, null));
            GameCanvas.menu.startAt(myVector, 3);
        }

        // Token: 0x06000A1C RID: 2588 RVA: 0x0008B270 File Offset: 0x00089470
        private static void ShowPlanetMenu()
        {
            MyVector myVector = new MyVector();
            foreach (KeyValuePair<string, int[]> keyValuePair in NQMP.LoadMap.PlanetDictionary)
            {
                myVector.addElement(new Command(keyValuePair.Key, NQMP.LoadMap.getInstance(), 6, keyValuePair.Value));
            }
            GameCanvas.menu.startAt(myVector, 3);
        }

        // Token: 0x06000A1D RID: 2589 RVA: 0x0008B2F0 File Offset: 0x000894F0
        private static void ShowMapsMenu(int[] a)
        {
            MyVector myVector = new MyVector();
            for (int i = 0; i < a.Length; i++)
            {
                if ((global::Char.myCharz().cgender != 0 || (a[i] != 22 && a[i] != 23)) && (global::Char.myCharz().cgender != 1 || (a[i] != 21 && a[i] != 23)) && (global::Char.myCharz().cgender != 2 || (a[i] != 21 && a[i] != 22)))
                {
                    myVector.addElement(new Command(NQMP.LoadMap.GetMapName(a[i]), NQMP.LoadMap.getInstance(), 7, a[i]));
                }
            }
            GameCanvas.menu.startAt(myVector, 3);
        }

        // Token: 0x06000A1E RID: 2590 RVA: 0x00008618 File Offset: 0x00006818
        public static void Xmap(int a)
        {
            NQMP.LoadMap.isXmaping = true;
            NQMP.LoadMap.idMapGoTo = a;
        }

        // Token: 0x06000A1F RID: 2591 RVA: 0x00008626 File Offset: 0x00006826
        public static void ResetStatus()
        {
            NQMP.LoadMap.isXmaping = false;
            NQMP.LoadMap.isUsingCapsule = false;
            NQMP.LoadMap.isOpeningPanel = false;
        }

        // Token: 0x06000A20 RID: 2592 RVA: 0x0008B390 File Offset: 0x00089590
        public static void StartRunToMapId(int a)
        {
            if (NQMP.LoadMap.LinkMaps.ContainsKey(84))
            {
                NQMP.LoadMap.LinkMaps.Remove(84);
            }
            NQMP.LoadMap.LinkMaps.Add(84, new List<NextMap>());
            NQMP.LoadMap.LinkMaps[84].Add(new LoadMap.NextMap(24 + global::Char.myCharz().cgender, 10, 0));
            int[] array = NQMP.LoadMap.FindWay(a);
            if (array == null)
            {
                GameScr.info1.addInfo("Không thể tìm thấy đường đi", 0);
                return;
            }
            if (NQMP.LoadMap.isUseCapsule)
            {
                if (!NQMP.LoadMap.isUsingCapsule && array.Length > 3)
                {
                    for (int i = 0; i < global::Char.myCharz().arrItemBag.Length; i++)
                    {
                        Item item = global::Char.myCharz().arrItemBag[i];
                        if (item != null && (item.template.id == 194 || (item.template.id == 193 && item.quantity > 10)))
                        {
                            NQMP.LoadMap.isUsingCapsule = true;
                            NQMP.LoadMap.isOpeningPanel = false;
                            NQMP.LoadMap.lastTimeOpenedPanel = mSystem.currentTimeMillis();
                            GameCanvas.panel.mapNames = null;
                            Service.gI().useItem(0, 1, -1, item.template.id);
                            return;
                        }
                    }
                }
                if (NQMP.LoadMap.isUsingCapsule && !NQMP.LoadMap.isOpeningPanel && (GameCanvas.panel.mapNames == null || mSystem.currentTimeMillis() - NQMP.LoadMap.lastTimeOpenedPanel < 500L))
                {
                    return;
                }
                if (NQMP.LoadMap.isUsingCapsule && !NQMP.LoadMap.isOpeningPanel)
                {
                    for (int j = array.Length - 1; j >= 2; j--)
                    {
                        for (int k = 0; k < GameCanvas.panel.mapNames.Length; k++)
                        {
                            if (GameCanvas.panel.mapNames[k].Contains(TileMap.mapNames[array[j]]))
                            {
                                NQMP.LoadMap.isOpeningPanel = true;
                                Service.gI().requestMapSelect(k);
                                return;
                            }
                        }
                    }
                    NQMP.LoadMap.isOpeningPanel = true;
                }
            }
            if (TileMap.mapID == array[0] && !global::Char.ischangingMap && !Controller.isStopReadMessage)
            {
                NQMP.LoadMap.Goto(array[1]);
            }
        }

        // Token: 0x06000A21 RID: 2593 RVA: 0x0000863A File Offset: 0x0000683A
        public static void LoadMapLeft()
        {
            NQMP.LoadMap.LoadMap2(0);
        }

        // Token: 0x06000A22 RID: 2594 RVA: 0x00008642 File Offset: 0x00006842
        public static void LoadMapCenter()
        {
            NQMP.LoadMap.LoadMap2(2);
        }

        // Token: 0x06000A23 RID: 2595 RVA: 0x0000864A File Offset: 0x0000684A
        public static void LoadMapRight()
        {
            NQMP.LoadMap.LoadMap2(1);
        }

        // Token: 0x06000A24 RID: 2596 RVA: 0x0008B57C File Offset: 0x0008977C
        private static void LoadData()
        {
            NQMP.LoadMap.isSaveData = (Rms.loadRMSInt("AutoMapIsSaveRms") == 1);
            if (NQMP.LoadMap.isSaveData)
            {
                if (Rms.loadRMSInt("AutoMapIsEatChicken") == -1)
                {
                    NQMP.LoadMap.isEatChicken = true;
                }
                else
                {
                    NQMP.LoadMap.isEatChicken = (Rms.loadRMSInt("AutoMapIsEatChicken") == 1);
                }
                if (Rms.loadRMSInt("AutoMapIsUseCsb") == -1)
                {
                    NQMP.LoadMap.isUseCapsule = true;
                }
                else
                {
                    NQMP.LoadMap.isUseCapsule = (Rms.loadRMSInt("AutoMapIsUseCsb") == 1);
                }
                NQMP.LoadMap.isHarvestPean = (Rms.loadRMSInt("AutoMapIsHarvestPean") == 1);
            }
        }

        // Token: 0x06000A25 RID: 2597 RVA: 0x0008B604 File Offset: 0x00089804
        private static void SaveData()
        {
            Rms.saveRMSInt("AutoMapIsEatChicken", NQMP.LoadMap.isEatChicken ? 1 : 0);
            Rms.saveRMSInt("AutoMapIsHarvestPean", NQMP.LoadMap.isHarvestPean ? 1 : 0);
            Rms.saveRMSInt("AutoMapIsUseCsb", NQMP.LoadMap.isUseCapsule ? 1 : 0);
        }

        // Token: 0x06000A26 RID: 2598 RVA: 0x0008B650 File Offset: 0x00089850
        private static void LoadLinkMapsXmap()
        {
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            0,
            21
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            1,
            47
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            47,
            111
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            2,
            24
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            5,
            29
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            7,
            22
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            9,
            25
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            13,
            33
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            14,
            23
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            16,
            26
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            20,
            37
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            39,
            21
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            40,
            22
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            41,
            23
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            109,
            105
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            109,
            106
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            106,
            107
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            108,
            105
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            80,
            105
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            3,
            27,
            28,
            29,
            30
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            11,
            31,
            32,
            33,
            34
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            17,
            35,
            36,
            37,
            38
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            109,
            108,
            107,
            110,
            106
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            47,
            46,
            45,
            48
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            131,
            132,
            133
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            42,
            0,
            1,
            2,
            3,
            4,
            5,
            6
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            43,
            7,
            8,
            9,
            11,
            12,
            13,
            10
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            52,
            44,
            14,
            15,
            16,
            17,
            18,
            20,
            19
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            53,
            58,
            59,
            60,
            61,
            62,
            55,
            56,
            54,
            57
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            68,
            69,
            70,
            71,
            72,
            64,
            65,
            63,
            66,
            67,
            73,
            74,
            75,
            76,
            77,
            81,
            82,
            83,
            79,
            80
            });
            NQMP.LoadMap.AddLinkMapsXmap(new int[]
            {
            102,
            92,
            93,
            94,
            96,
            97,
            98,
            99,
            100,
            103
            });
        }

        // Token: 0x06000A27 RID: 2599 RVA: 0x0008B8F0 File Offset: 0x00089AF0
        private static void LoadNPCLinkMapsXmap()
        {
            NQMP.LoadMap.AddNPCLinkMapsXmap(19, 68, 12, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(19, 109, 12, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(24, 25, 10, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(24, 26, 10, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(24, 84, 10, 2);
            NQMP.LoadMap.AddNPCLinkMapsXmap(25, 24, 11, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(25, 26, 11, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(25, 84, 11, 2);
            NQMP.LoadMap.AddNPCLinkMapsXmap(26, 24, 12, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(26, 25, 12, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(26, 84, 12, 2);
            NQMP.LoadMap.AddNPCLinkMapsXmap(27, 102, 38, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(27, 53, 25, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(28, 102, 38, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(29, 102, 38, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(45, 46, 19, 3);
            NQMP.LoadMap.AddNPCLinkMapsXmap(52, 127, 44, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(52, 129, 23, 3);
            NQMP.LoadMap.AddNPCLinkMapsXmap(52, 113, 23, 2);
            NQMP.LoadMap.AddNPCLinkMapsXmap(68, 19, 12, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(80, 131, 60, 0);
            NQMP.LoadMap.AddNPCLinkMapsXmap(102, 27, 38, 1);
            NQMP.LoadMap.AddNPCLinkMapsXmap(113, 52, 22, 4);
            NQMP.LoadMap.AddNPCLinkMapsXmap(127, 52, 44, 2);
            NQMP.LoadMap.AddNPCLinkMapsXmap(129, 52, 23, 3);
            NQMP.LoadMap.AddNPCLinkMapsXmap(131, 80, 60, 1);
        }

        // Token: 0x06000A28 RID: 2600 RVA: 0x0008BA44 File Offset: 0x00089C44
        private static void AddPlanetXmap()
        {
            NQMP.LoadMap.PlanetDictionary.Add("Trái đất", NQMP.LoadMap.idMapTraiDat);
            NQMP.LoadMap.PlanetDictionary.Add("Namếc", NQMP.LoadMap.idMapNamek);
            NQMP.LoadMap.PlanetDictionary.Add("Xayda", NQMP.LoadMap.idMapXayda);
            NQMP.LoadMap.PlanetDictionary.Add("Fide", NQMP.LoadMap.idMapNappa);
            NQMP.LoadMap.PlanetDictionary.Add("Tương lai", NQMP.LoadMap.idMapTuongLai);
            NQMP.LoadMap.PlanetDictionary.Add("Cold", NQMP.LoadMap.idMapCold);
        }

        // Token: 0x06000A29 RID: 2601 RVA: 0x0008BACC File Offset: 0x00089CCC
        private static void AddLinkMapsXmap(params int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (!NQMP.LoadMap.LinkMaps.ContainsKey(a[i]))
                {
                    NQMP.LoadMap.LinkMaps.Add(a[i], new List<NextMap>());
                }
                if (i != 0)
                {
                    NQMP.LoadMap.LinkMaps[a[i]].Add(new LoadMap.NextMap(a[i - 1], -1, -1));
                }
                if (i != a.Length - 1)
                {
                    NQMP.LoadMap.LinkMaps[a[i]].Add(new LoadMap.NextMap(a[i + 1], -1, -1));
                }
            }
        }

        // Token: 0x06000A2A RID: 2602 RVA: 0x00008652 File Offset: 0x00006852
        private static void AddNPCLinkMapsXmap(int a, int b, int c, int d)
        {
            if (!NQMP.LoadMap.LinkMaps.ContainsKey(a))
            {
                NQMP.LoadMap.LinkMaps.Add(a, new List<NextMap>());
            }
            NQMP.LoadMap.LinkMaps[a].Add(new LoadMap.NextMap(b, c, d));
        }

        // Token: 0x06000A2B RID: 2603 RVA: 0x0008BB50 File Offset: 0x00089D50
        private static void Goto(int a)
        {
            foreach (LoadMap.NextMap nextMap in NQMP.LoadMap.LinkMaps[TileMap.mapID])
            {
                if (nextMap.MapID == a)
                {
                    nextMap.GotoMap();
                    return;
                }
            }
            GameScr.info1.addInfo("Không thể thực hiện", 0);
        }

        // Token: 0x06000A2C RID: 2604 RVA: 0x00008689 File Offset: 0x00006889
        private static int[] FindWay(int a)
        {
            return NQMP.LoadMap.FindWay(a, new int[]
            {
            TileMap.mapID
            });
        }

        // Token: 0x06000A2D RID: 2605 RVA: 0x0008BBCC File Offset: 0x00089DCC
        private static int[] FindWay(int a, int[] b)
        {
            List<int[]> list = new List<int[]>();
            List<int> list2 = new List<int>();
            list2.AddRange(b);
            foreach (LoadMap.NextMap nextMap in NQMP.LoadMap.LinkMaps[b[b.Length - 1]])
            {
                if (a == nextMap.MapID)
                {
                    list2.Add(a);
                    return list2.ToArray();
                }
                if (!list2.Contains(nextMap.MapID))
                {
                    List<int> list3 = new List<int>(list2)
                {
                    nextMap.MapID
                };
                    int[] array = NQMP.LoadMap.FindWay(a, list3.ToArray());
                    if (array != null)
                    {
                        list.Add(array);
                    }
                }
            }
            int num = 9999;
            int[] result = null;
            foreach (int[] array2 in list)
            {
                if (!NQMP.LoadMap.hasWayGoFutureAndBack(array2) && (global::Char.myCharz().taskMaint.taskId > 30 || !NQMP.LoadMap.hasWayGoToColdMap(array2)) && array2.Length < num)
                {
                    num = array2.Length;
                    result = array2;
                }
            }
            return result;
        }

        // Token: 0x06000A2E RID: 2606 RVA: 0x0008BD10 File Offset: 0x00089F10
        private static bool hasWayGoFutureAndBack(int[] a)
        {
            for (int i = 1; i < a.Length - 1; i++)
            {
                if (a[i] == 102 && a[i + 1] == 24)
                {
                    if (a[i - 1] != 27 && a[i - 1] != 28)
                    {
                        if (a[i - 1] != 29)
                        {
                            goto IL_31;
                        }
                    }
                    return true;
                }
            IL_31:;
            }
            return false;
        }

        // Token: 0x06000A2F RID: 2607 RVA: 0x0008BD60 File Offset: 0x00089F60
        private static bool hasWayGoToColdMap(int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] >= 105 && a[i] <= 110)
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000A30 RID: 2608 RVA: 0x0008BD8C File Offset: 0x00089F8C
        private static string GetMapName(int a)
        {
            string result;
            if (a != 113)
            {
                if (a == 129)
                {
                    result = TileMap.mapNames[a] + " 23\n[" + a.ToString() + "]";
                }
                else
                {
                    result = TileMap.mapNames[a] + "\n[" + a.ToString() + "]";
                }
            }
            else
            {
                result = string.Concat(new object[]
                {
                "Siêu hạng\n[",
                a,
                "]"
                });
            }
            return result;
        }

        // Token: 0x06000A31 RID: 2609 RVA: 0x0008BE0C File Offset: 0x0008A00C
        private static void LoadWaypointsInMap()
        {
            NQMP.LoadMap.ResetSavedWaypoints();
            int num = TileMap.vGo.size();
            if (num != 2)
            {
                for (int i = 0; i < num; i++)
                {
                    Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
                    if (waypoint.maxX < 60)
                    {
                        NQMP.LoadMap.wayPointMapLeft[0] = (int)(waypoint.minX + 15);
                        NQMP.LoadMap.wayPointMapLeft[1] = (int)waypoint.maxY;
                    }
                    else if ((int)waypoint.maxX > TileMap.pxw - 60)
                    {
                        NQMP.LoadMap.wayPointMapRight[0] = (int)(waypoint.maxX - 15);
                        NQMP.LoadMap.wayPointMapRight[1] = (int)waypoint.maxY;
                    }
                    else
                    {
                        NQMP.LoadMap.wayPointMapCenter[0] = (int)(waypoint.minX + 15);
                        NQMP.LoadMap.wayPointMapCenter[1] = (int)waypoint.maxY;
                        if (waypoint.isEnter)
                        {
                            NQMP.LoadMap.wayPointMapCenter[2] = 1;
                        }
                        else
                        {
                            NQMP.LoadMap.wayPointMapCenter[2] = 0;
                        }
                    }

                }
                return;
            }
            Waypoint waypoint2 = (Waypoint)TileMap.vGo.elementAt(0);
            Waypoint waypoint3 = (Waypoint)TileMap.vGo.elementAt(1);
            if ((waypoint2.maxX < 60 && waypoint3.maxX < 60) || ((int)waypoint2.minX > TileMap.pxw - 60 && (int)waypoint3.minX > TileMap.pxw - 60))
            {
                NQMP.LoadMap.wayPointMapLeft[0] = (int)(waypoint2.minX + 15);
                NQMP.LoadMap.wayPointMapLeft[1] = (int)waypoint2.maxY;
                NQMP.LoadMap.wayPointMapRight[0] = (int)(waypoint3.maxX - 15);
                NQMP.LoadMap.wayPointMapRight[1] = (int)waypoint3.maxY;
                return;
            }
            if (waypoint2.maxX < waypoint3.maxX)
            {
                NQMP.LoadMap.wayPointMapLeft[0] = (int)(waypoint2.minX + 15);
                NQMP.LoadMap.wayPointMapLeft[1] = (int)waypoint2.maxY;
                NQMP.LoadMap.wayPointMapRight[0] = (int)(waypoint3.maxX - 15);
                NQMP.LoadMap.wayPointMapRight[1] = (int)waypoint3.maxY;
                return;
            }
            NQMP.LoadMap.wayPointMapLeft[0] = (int)(waypoint3.minX + 15);
            NQMP.LoadMap.wayPointMapLeft[1] = (int)waypoint3.maxY;
            NQMP.LoadMap.wayPointMapRight[0] = (int)(waypoint2.maxX - 15);
            NQMP.LoadMap.wayPointMapRight[1] = (int)waypoint2.maxY;
        }

        // Token: 0x06000A32 RID: 2610 RVA: 0x0008BFE8 File Offset: 0x0008A1E8
        public static int GetYGround(int a)
        {
            int num = 50;
            int i = 0;
            while (i < 30)
            {
                i++;
                num += 24;
                if (TileMap.tileTypeAt(a, num, 2))
                {
                    if (num % 24 != 0)
                    {
                        num -= num % 24;
                    }
                    return num;
                }
            }
            return num;
        }

        // Token: 0x06000A33 RID: 2611 RVA: 0x0008C024 File Offset: 0x0008A224
        private static void TeleportTo(int a, int b)
        {
            if (GameScr.canAutoPlay)
            {
                global::Char.myCharz().cx = a;
                global::Char.myCharz().cy = b;
                Service.gI().charMove();
                return;
            }
            global::Char.myCharz().cx = a;
            global::Char.myCharz().cy = b;
            Service.gI().charMove();
            global::Char.myCharz().cx = a;
            global::Char.myCharz().cy = b + 1;
            Service.gI().charMove();
            global::Char.myCharz().cx = a;
            global::Char.myCharz().cy = b;
            Service.gI().charMove();
        }

        // Token: 0x06000A34 RID: 2612 RVA: 0x0000869F File Offset: 0x0000689F
        private static void ResetSavedWaypoints()
        {
            NQMP.LoadMap.wayPointMapLeft = new int[2];
            NQMP.LoadMap.wayPointMapCenter = new int[3];
            NQMP.LoadMap.wayPointMapRight = new int[2];
        }

        // Token: 0x06000A35 RID: 2613 RVA: 0x000086C2 File Offset: 0x000068C2
        private static bool isNRDMap(int a)
        {
            return a >= 85 && a <= 91;
        }

        // Token: 0x06000A36 RID: 2614 RVA: 0x0008C0BC File Offset: 0x0008A2BC
        private static bool isFutureMap(int a)
        {
            for (int i = 0; i < NQMP.LoadMap.idMapTuongLai.Length; i++)
            {
                if (NQMP.LoadMap.idMapTuongLai[i] == a)
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000A37 RID: 2615 RVA: 0x000086D3 File Offset: 0x000068D3
        private static bool isNRD(ItemMap a)
        {
            return a.template.id >= 372 && a.template.id <= 378;
        }

        // Token: 0x06000A38 RID: 2616 RVA: 0x0008C0EC File Offset: 0x0008A2EC
        private static void LoadMap2(int a)
        {
            if (NQMP.LoadMap.isNRDMap(TileMap.mapID))
            {
                NQMP.LoadMap.isClanXmap(a);
                return;
            }
            NQMP.LoadMap.LoadWaypointsInMap();
            switch (a)
            {
                case 0:
                    if (NQMP.LoadMap.wayPointMapLeft[0] != 0 && NQMP.LoadMap.wayPointMapLeft[1] != 0)
                    {
                        NQMP.LoadMap.TeleportTo(NQMP.LoadMap.wayPointMapLeft[0], NQMP.LoadMap.wayPointMapLeft[1]);
                    }
                    else
                    {
                        NQMP.LoadMap.TeleportTo(60, NQMP.LoadMap.GetYGround(60));
                    }
                    break;
                case 1:
                    if (NQMP.LoadMap.wayPointMapRight[0] != 0 && NQMP.LoadMap.wayPointMapRight[1] != 0)
                    {
                        NQMP.LoadMap.TeleportTo(NQMP.LoadMap.wayPointMapRight[0], NQMP.LoadMap.wayPointMapRight[1]);
                    }
                    else
                    {
                        NQMP.LoadMap.TeleportTo(TileMap.pxw - 60, NQMP.LoadMap.GetYGround(TileMap.pxw - 60));
                    }

                    break;
                case 2:
                    if (NQMP.LoadMap.wayPointMapCenter[0] != 0 && NQMP.LoadMap.wayPointMapCenter[1] != 0)
                    {
                        NQMP.LoadMap.TeleportTo(NQMP.LoadMap.wayPointMapCenter[0], NQMP.LoadMap.wayPointMapCenter[1]);
                        if (wayPointMapCenter[2] == 1)
                        {
                            if (TileMap.mapID == 7 || TileMap.mapID == 14 || TileMap.mapID == 0)
                            {
                                Service.gI().getMapOffline();
                                return;
                            }

                        }
                    }
                    else
                    {
                        NQMP.LoadMap.TeleportTo(TileMap.pxw / 2, NQMP.LoadMap.GetYGround(TileMap.pxw / 2));
                    }

                    break;
            }
            Service.gI().requestChangeMap();
        }

        // Token: 0x06000A39 RID: 2617 RVA: 0x0008C214 File Offset: 0x0008A414
        private static void isClanXmap(int a)
        {
            if (a == 0)
            {
                NQMP.LoadMap.TeleportTo(60, NQMP.LoadMap.GetYGround(60));
                return;
            }
            if (a == 2)
            {
                if (global::Char.myCharz().bag >= 0 && ClanImage.idImages.containsKey(global::Char.myCharz().bag.ToString()))
                {
                    ClanImage clanImage = (ClanImage)ClanImage.idImages.get(global::Char.myCharz().bag.ToString());
                    if (clanImage.idImage != null)
                    {
                        for (int i = 0; i < clanImage.idImage.Length; i++)
                        {
                            if (clanImage.idImage[i] == 2322)
                            {
                                for (int j = 0; j < GameScr.vNpc.size(); j++)
                                {
                                    Npc npc = (Npc)GameScr.vNpc.elementAt(j);
                                    if (npc.template.npcTemplateId >= 30 && npc.template.npcTemplateId <= 36)
                                    {
                                        global::Char.myCharz().npcFocus = npc;
                                        NQMP.LoadMap.TeleportTo(npc.cx, npc.cy - 3);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                for (int k = 0; k < GameScr.vItemMap.size(); k++)
                {
                    ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(k);
                    if (itemMap != null && NQMP.LoadMap.isNRD(itemMap))
                    {
                        global::Char.myCharz().itemFocus = itemMap;
                        NQMP.LoadMap.TeleportTo(itemMap.x, itemMap.y);
                        return;
                    }
                }
                return;
            }
            NQMP.LoadMap.TeleportTo(TileMap.pxw - 60, NQMP.LoadMap.GetYGround(TileMap.pxw - 60));
        }

        // Token: 0x06000A3A RID: 2618 RVA: 0x0008C390 File Offset: 0x0008A590
        static LoadMap()
        {
            NQMP.LoadMap.LoadLinkMapsXmap();
            NQMP.LoadMap.LoadNPCLinkMapsXmap();
            NQMP.LoadMap.AddPlanetXmap();
            NQMP.LoadMap.LoadData();
        }

        // Token: 0x04001359 RID: 4953
        public static LoadMap _Instance;

        // Token: 0x0400135A RID: 4954
        private static Dictionary<int, List<LoadMap.NextMap>> LinkMaps = new Dictionary<int, List<LoadMap.NextMap>>();

        // Token: 0x0400135B RID: 4955
        private static Dictionary<string, int[]> PlanetDictionary = new Dictionary<string, int[]>();

        // Token: 0x0400135C RID: 4956
        public static bool isXmaping;

        // Token: 0x0400135D RID: 4957
        public static int idMapGoTo;

        // Token: 0x0400135E RID: 4958
        private static int[] wayPointMapLeft;

        // Token: 0x0400135F RID: 4959
        private static int[] wayPointMapCenter;

        // Token: 0x04001360 RID: 4960
        private static int[] wayPointMapRight;

        // Token: 0x04001361 RID: 4961
        private static bool isEatChicken = true;

        // Token: 0x04001362 RID: 4962
        private static bool isHarvestPean;

        // Token: 0x04001363 RID: 4963
        private static bool isUseCapsule = true;

        // Token: 0x04001364 RID: 4964
        private static bool isUsingCapsule;

        // Token: 0x04001365 RID: 4965
        private static bool isOpeningPanel;

        // Token: 0x04001366 RID: 4966
        private static long lastTimeOpenedPanel;

        // Token: 0x04001367 RID: 4967
        private static bool isSaveData;

        // Token: 0x04001368 RID: 4968
        private static long lastWaitTime;

        // Token: 0x04001369 RID: 4969
        private static int[] idMapNamek = new int[]
        {
        43,
        22,
        7,
        8,
        9,
        11,
        12,
        13,
        10,
        31,
        32,
        33,
        34,
        43,
        25
        };

        // Token: 0x0400136A RID: 4970
        private static int[] idMapXayda = new int[]
        {
        44,
        23,
        14,
        15,
        16,
        17,
        18,
        20,
        19,
        35,
        36,
        37,
        38,
        52,
        44,
        26,
        84,
        113,
        127,
        129
        };

        // Token: 0x0400136B RID: 4971
        private static int[] idMapTraiDat = new int[]
        {
        42,
        21,
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        27,
        28,
        29,
        30,
        47,
        42,
        24,
        46,
        45,
        48,
        53,
        58,
        59,
        60,
        61,
        62,
        55,
        56,
        54,
        57
        };

        // Token: 0x0400136C RID: 4972
        private static int[] idMapTuongLai = new int[]
        {
        102,
        92,
        93,
        94,
        96,
        97,
        98,
        99,
        100,
        103
        };

        // Token: 0x0400136D RID: 4973
        private static int[] idMapCold = new int[]
        {
        109,
        108,
        107,
        110,
        106,
        105
        };

        // Token: 0x0400136E RID: 4974
        private static int[] idMapNappa = new int[]
        {
        68,
        69,
        70,
        71,
        72,
        64,
        65,
        63,
        66,
        67,
        73,
        74,
        75,
        76,
        77,
        81,
        82,
        83,
        79,
        80,
        131,
        132,
        133
        };

        // Token: 0x020000ED RID: 237
        public class NextMap
        {
            // Token: 0x06000A3C RID: 2620 RVA: 0x000086FE File Offset: 0x000068FE
            public NextMap(int a, int b, int c)
            {
                this.MapID = a;
                this.Npc = b;
                this.Index = c;
            }

            // Token: 0x06000A3D RID: 2621 RVA: 0x0008C45C File Offset: 0x0008A65C
            public void GotoMap()
            {
                if (this.Index == -1 && this.Npc == -1)
                {
                    Waypoint wayPoint = this.GetWayPoint();
                    if (wayPoint != null)
                    {
                        this.Enter(wayPoint);
                        return;
                    }
                }
                else if (this.Npc != -1 && this.Index != -1)
                {
                    Service.gI().openMenu(this.Npc);
                    Service.gI().confirmMenu(0, (sbyte)this.Index);
                }
            }

            // Token: 0x06000A3E RID: 2622 RVA: 0x0008C4C4 File Offset: 0x0008A6C4
            public Waypoint GetWayPoint()
            {
                for (int i = 0; i < TileMap.vGo.size(); i++)
                {
                    Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
                    if (this.GetMapName().Equals(this.GetMapName(waypoint.popup)))
                    {
                        return waypoint;
                    }
                }
                return null;
            }

            // Token: 0x06000A3F RID: 2623 RVA: 0x0000871B File Offset: 0x0000691B
            public string GetMapName()
            {
                return TileMap.mapNames[this.MapID];
            }

            // Token: 0x06000A40 RID: 2624 RVA: 0x0008C514 File Offset: 0x0008A714
            public void Enter(Waypoint a)
            {
                int num = (a.maxX < 60) ? 15 : (((int)a.minX <= TileMap.pxw - 60) ? ((int)((a.minX + a.maxX) / 2)) : (TileMap.pxw - 15));
                int maxY = (int)a.maxY;
                if (num != -1)
                {
                    if (maxY != -1)
                    {
                        this.TeleportTo(num, maxY);
                        if (a.isOffline)
                        {
                            Service.gI().getMapOffline();
                            return;
                        }
                        Service.gI().requestChangeMap();
                        return;
                    }
                }
                GameScr.info1.addInfo("Có lỗi xảy ra", 0);
            }

            // Token: 0x06000A41 RID: 2625 RVA: 0x0008C5A0 File Offset: 0x0008A7A0
            public string GetMapName(PopUp a)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < a.says.Length; i++)
                {
                    stringBuilder.Append(a.says[i]);
                    stringBuilder.Append(" ");
                }
                return stringBuilder.ToString().Trim();
            }

            // Token: 0x06000A42 RID: 2626 RVA: 0x0008C5EC File Offset: 0x0008A7EC
            public void TeleportTo(int a, int b)
            {
                if (GameScr.canAutoPlay)
                {
                    global::Char.myCharz().cx = a;
                    global::Char.myCharz().cy = b;
                    Service.gI().charMove();
                    return;
                }
                global::Char.myCharz().cx = a;
                global::Char.myCharz().cy = b;
                Service.gI().charMove();
                global::Char.myCharz().cx = a;
                global::Char.myCharz().cy = b + 1;
                Service.gI().charMove();
                global::Char.myCharz().cx = a;
                global::Char.myCharz().cy = b;
                Service.gI().charMove();
            }

            // Token: 0x0400136F RID: 4975
            public int MapID;

            // Token: 0x04001370 RID: 4976
            public int Npc;

            // Token: 0x04001371 RID: 4977
            public int Index;
        }
    }
}