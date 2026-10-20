using System;
using System.Collections.Generic;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class Boss
    {
        private List<Boss> bosses = new List<Boss>();
        public DateTime AppearTime;
        private static Boss instance;
        public bool isShow;
        private string name;
        private string map;
        private int mapID;
        public Boss()
        {
        }
        public static Boss gI()
        {
            if (instance == null)
            {
                instance = new Boss();
            }
            return instance;
        }
        private int getMapID(string mapName)
        {
            for (int i = 0; i < TileMap.mapNames.Length; i++)
            {
                if (TileMap.mapNames[i] != null && TileMap.mapNames[i] == mapName)
                {
                    return i;
                }
            }
            return -1;
        }
        private void getBOSSInfo(string string_0)
        {
            string[] array = string_0.Replace(string_0.StartsWith("BOSS") ? "BOSS " : "Boss ", "").Replace(" vừa xuất hiện tại ", "|").Replace(" appear at ", "|")
                .Split(new char[] { '|' });
            Boss bossInfo = new Boss
            {
                name = array[0].Trim(),
                map = array[1].Trim(),
                mapID = this.getMapID(array[0]),
                AppearTime = DateTime.Now
            };

            bosses.Add(bossInfo);
            if (bosses.Count > 5)
            {
                bosses.RemoveAt(0);
            }
        }
        public void paintBossesScreen(mGraphics g)
        {
            if (!isShow) return;
            for (int i = 0; i < bosses.Count; i++)
            {
                var bossInfos = bosses[i];
                string name = bossInfos.name;
                string map = bossInfos.map; 
                TimeSpan timeSpan = DateTime.Now.Subtract(AppearTime);
                int num = (int)timeSpan.TotalSeconds;
                string bBoss = string.Concat(new object[] {
                name + " - " + map + " - " + ((num < 60) ? (num + "s") : (timeSpan.Minutes + "ph")) + " trước"
                });
                g.setColor(2721889, 0.5f);
                g.fillRect(GameCanvas.w - 23, 39 + 12 * i, 21, 9);
                mFont.tahoma_7b_yellow.drawString(g, bBoss, GameCanvas.w, 37 + 12 * i, mFont.RIGHT, mFont.tahoma_7b_dark);
            }
        }
        public void chatVip(string info)
        {
            if (info.StartsWith("BOSS") || info.StartsWith("Boss"))
            {
                getBOSSInfo(info);
            }

        }
    }
}
