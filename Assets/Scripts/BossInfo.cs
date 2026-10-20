using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInfo
{
    public short head;
    public string name;
    public string description;
    public BossInfo(short head,string name,string des)
    {
        this.head = head;
        this.name = name;
        this.description = des;
    }

    public static void request(Message msg)
    {
        int lenght = msg.reader().readInt();
        bossInfos.Clear();
        for (int i = 0; i < lenght; i++)
        {
            short head = msg.reader().readShort();
            string name = msg.reader().readUTF();
            string description = msg.reader().readUTF();
            BossInfo boss = new BossInfo(head, name, description);
            bossInfos.Add(boss);           
        }
        GameCanvas.panel.setTypeBossInfo();
        GameCanvas.panel.show();

    }

    public static List<BossInfo> bossInfos = new List<BossInfo>();
}
