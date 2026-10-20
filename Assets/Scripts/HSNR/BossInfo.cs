using System;
using System.Collections.Generic;
using System.Linq;

public class ServerBOSS
{
    private DateTime? startTime, endTime;
    private string playerName, bossName; 
    private string mapName;
    public const string HAS_NOT_INFO = "Chưa có thông tin";

    public static Dictionary<string, List<ServerBOSS>> BossDictionary = new Dictionary<string, List<ServerBOSS>>();
    private List<ServerBOSS> Bosses = new List<ServerBOSS>();
    public ServerBOSS()
    {
        startTime = endTime = null;
    }
    public static void addBoss(string chatVip)
    {
        if (chatVip.Contains("tiêu diệt"))
            getPlayerInfo(chatVip);
        else
            getBOSSInfo(chatVip);
    }
    private static void getPlayerInfo(string chatVip)
    {
        string[] arr = chatVip
            .Replace(": Đã tiêu diệt được ", "|")
            .Replace(" mọi người đều ngưỡng mộ.", string.Empty)
            .Split('|');
        if (!BossDictionary.ContainsKey(arr[1].Trim()))
            BossDictionary.Add(arr[1].Trim(), new List<ServerBOSS>());
        var list = BossDictionary[arr[1].Trim()];
        if(list.Count == 0)
        {
            list.Add(new ServerBOSS
            {
                playerName = arr[0].Trim(),
                bossName = arr[1].Trim(),
                endTime = new DateTime?(DateTime.Now)
            });
            return;
        }
        list[0].playerName = arr[0].Trim();
        list[0].bossName = arr[1].Trim();
        list[0].endTime = new DateTime?(DateTime.Now); 
    }
    private static void getBOSSInfo(string chatVip)
    {
        string[] arr = chatVip.
            Replace("BOSS", string.Empty)
           .Replace(" vừa xuất hiện tại ", "|")
           .Split('|');
        ServerBOSS b = new ServerBOSS
        {
            bossName = arr[0].Trim(),
            mapName = arr[1].Trim(),
            playerName = string.Empty,
            startTime = new DateTime?(DateTime.Now)
        };
        if (!BossDictionary.ContainsKey(b.bossName))
        { BossDictionary.Add(b.bossName, new List<ServerBOSS>());
            SortBossDictionary();
        }
        BossDictionary[b.bossName].Insert(0, b);  
    }
    private string getTimeInfo(DateTime? dateTime)
    {
        if (dateTime == null)
            return HAS_NOT_INFO;
        TimeSpan tsp = DateTime.Now.Subtract(dateTime.Value);
        int seconds = (int)tsp.TotalSeconds;
        return $"{dateTime.Value.ToString("HH")}h:" +
            $"{dateTime.Value.ToString("mm")}({(seconds < 60 ? (seconds.ToString() + "s") : (tsp.Minutes.ToString() + "ph"))}) trước";
    }
    public string getStartTimeInfo() => getTimeInfo(startTime);
    public string getEndTimeInfo() => getTimeInfo(endTime);

    public string getMapName() => mapName ?? HAS_NOT_INFO;
    public string getPlayerName() => playerName ?? HAS_NOT_INFO;

    public static void SortBossDictionary()
    {
        var sortedDict = BossDictionary
            .OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        BossDictionary.Clear();
        foreach (var kvp in sortedDict)
        {
            BossDictionary.Add(kvp.Key, kvp.Value);
        }
    }
}