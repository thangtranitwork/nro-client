using System;

public class ChatVip
{
    public string nameBoss;

    public string mapName;

    public int mapId;

    public DateTime appearTime;

    public ChatVip()
    {
    }

    public ChatVip(string text)
    {
        text = text.Replace("BOSS ", "").Replace(" vừa xuất hiện tại ", "|").Replace(" appear at ", "|");
        string[] array = text.Split('|');
        nameBoss = array[0].Trim();
        mapName = array[1].Trim();
        mapId = GetMapId(mapName);
        appearTime = DateTime.Now;
    }

    public int GetMapId(string mapName)
    {
        for (int i = 0; i < TileMap.mapNames.Length; i++)
        {
            if (TileMap.mapNames[i].Equals(mapName))
            {
                return i;
            }
        }
        return -1;
    }

    public void Paint(mGraphics g, int x, int y, int align)
    {
        TimeSpan timeSpan = DateTime.Now.Subtract(appearTime);
        int num = (int)timeSpan.TotalSeconds;
        mFont mFont2 = mFont.tahoma_7_yellow;
        if (TileMap.mapID == mapId)
        {
            mFont2 = mFont.tahoma_7_red;
            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                if (((Char)GameScr.vCharInMap.elementAt(i)).cName.Equals(nameBoss))
                {
                    mFont2 = mFont.tahoma_7b_red;
                    break;
                }
            }
        }
        mFont2.drawString(g, nameBoss + " - " + mapName + " - " + ((num < 60) ? (num + "s") : (timeSpan.Minutes + "ph")) + " trước", x, y, align);
    }
}