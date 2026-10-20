using System;

public class RankSystem
{
    public const long SM_MAX = 120010000000L;
    
    public static Image[] rankIcons = new Image[9];
    
    public static string[] rankNames = {
        "Đồng", "Sắt", "Bạc", "Vàng", "Bạch Kim",
        "Lục Bảo", "Kim Cương", "Cao Thủ", "Đại Cao Thủ"
    };
    
    public static long[] rankPowers = {
        0L,
        17999999999L,
        24999999999L,
        39999999999L,
        50010000000L,
        60010000000L,
        80010000000L,
        100010000000L,
        120010000000L
    };
    
    public static int[] rankMaxHP = { 220000, 240000, 280000, 300000, 350000, 400000, 450000, 475000, 500000 };
    public static int[] rankMaxMP = { 220000, 240000, 280000, 300000, 350000, 400000, 450000, 475000, 500000 };
    public static int[] rankMaxDamage = { 11000, 12000, 16000, 18000, 20000, 22000, 24000, 25000, 26000 };
    public static int[] rankMaxDefense = { 550, 600, 800, 1000, 1200, 1400, 1600, 1600, 2000 };
    public static int[] rankMaxCritical = { 5, 6, 8, 9, 10, 10, 10, 10, 15 };
    
    public static string[] rankIconFiles = {
        "dong.png", "sat.png", "bac.png", "vang.png", "bach-kim.png",
        "luc-bao.png", "kim-cuong.png", "cao-thu.png", "dai-cao-thu.png"
    };
    
    private static bool isLoaded = false;
    
    public static void LoadRankIcons()
    {
        if (isLoaded) return;
        for (int i = 0; i < rankIconFiles.Length; i++)
        {
            try { rankIcons[i] = GameCanvas.loadImage("/mainImage/" + rankIconFiles[i]); }
            catch { if (i > 0 && rankIcons[0] != null) rankIcons[i] = rankIcons[0]; }
        }
        isLoaded = true;
    }
    
    public static int GetMyRankIndex()
    {
        if (Char.myCharz() == null) return 0;
        long tn = Char.myCharz().cTiemNang;
        for (int i = rankPowers.Length - 1; i >= 0; i--)
        {
            if (tn >= rankPowers[i]) return i;
        }
        return 0;
    }
    
    public static string GetMyRankName() => rankNames[GetMyRankIndex()];
    
    public static Image GetMyRankIcon()
    {
        LoadRankIcons();
        int i = GetMyRankIndex();
        return rankIcons[i];
    }
    
    public static string GetNextRankName()
    {
        int i = GetMyRankIndex();
        return i >= 8 ? "MAX" : rankNames[i + 1];
    }
    
    public static long GetPowerForNextRank()
    {
        if (Char.myCharz() == null) return 0;
        int i = GetMyRankIndex();
        if (i >= 8) return 0;
        return rankPowers[i + 1] - Char.myCharz().cTiemNang;
    }
    
    public static int GetTierProgress()
    {
        if (Char.myCharz() == null) return 0;
        int i = GetMyRankIndex();
        long tn = Char.myCharz().cTiemNang;
        long low = rankPowers[i];
        long high = (i >= 8) ? SM_MAX : rankPowers[i + 1];
        if (high <= low) return 100;
        return (int)Math.Min(100, (tn - low) * 100 / (high - low));
    }
    
    public static int GetOverallProgress()
    {
        if (Char.myCharz() == null) return 0;
        return (int)(Char.myCharz().cTiemNang * 100 / SM_MAX);
    }
    
    public static bool IsMaxRank() => GetMyRankIndex() >= 8;
    
    public static int GetMaxHP() => rankMaxHP[GetMyRankIndex()];
    public static int GetMaxMP() => rankMaxMP[GetMyRankIndex()];
    public static int GetMaxDamage() => rankMaxDamage[GetMyRankIndex()];
    public static int GetMaxDefense() => rankMaxDefense[GetMyRankIndex()];
    public static int GetMaxCritical() => rankMaxCritical[GetMyRankIndex()];
}
