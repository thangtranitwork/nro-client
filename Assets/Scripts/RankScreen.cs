using System;

public class RankScreen : mScreen
{
    static RankScreen instance;
    Command btnExit = new Command();
    int x, y, w, h;
    
    public RankScreen()
    {
        RefreshLayout();
        btnExit.img = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
    }
    
    private void RefreshLayout()
    {
        w = Math.Min(220, GameCanvas.w - 20);
        h = Math.Min(280, GameCanvas.h - 20);
        x = (GameCanvas.w - w) / 2;
        y = (GameCanvas.h - h) / 2;
    }
    
    public static RankScreen gI()
    {
        instance ??= new RankScreen();
        return instance;
    }
    
    public override void switchToMe()
    {
        RefreshLayout();
        btnExit.x = x + w - btnExit.img.getWidth() - 3;
        btnExit.y = y + 3;
        GameScr.isPaintOther = true;
        GameCanvas.panel.hideNow();
        base.switchToMe();
    }
    
    public override void paint(mGraphics g)
    {
        GameScr.gI().paint(g);
        
        g.setColor(0x000000, 0.35f);
        g.fillRect(x + 3, y + 4, w, h, 8);
        PopUp.paintPopUp(g, x, y, w, h, 0, true);
        
        g.setColor(0x1a237e, 0.95f);
        g.fillRect(x + 4, y + 4, w - 8, 24, 5);
        mFont.tahoma_7b_yellow.drawString(g, "HỆ THỐNG RANK", x + w / 2, y + 12, mFont.CENTER, mFont.tahoma_7b_dark);
        
        try
        {
            Char c = Char.myCharz();
            if (c == null) return;
            
            int row = y + 32;
            int pad = 8;
            
            Image rankIcon = RankSystem.GetMyRankIcon();
            if (rankIcon != null)
            {
                int iconX = x + w / 2 - mGraphics.getImageWidth(rankIcon) / 2;
                g.drawImage(rankIcon, iconX, row, 0);
                row += mGraphics.getImageHeight(rankIcon) + 3;
            }
            
            mFont.tahoma_7b_yellow.drawString(g, RankSystem.GetMyRankName(), x + w / 2, row, mFont.CENTER, mFont.tahoma_7b_dark);
            row += 18;
            
            g.setColor(0x283593, 0.4f);
            g.fillRect(x + pad, row, w - pad * 2, 35, 5);
            
            int tierProgress = RankSystem.GetTierProgress();
            mFont.tahoma_7_green2.drawString(g, "Tiến độ: " + tierProgress + "%", x + pad + 5, row + 3, 0);
            
            int barX = x + pad + 5, barY = row + 16, barW = w - pad * 2 - 10, barH = 10;
            g.setColor(0x222222);
            g.fillRect(barX, barY, barW, barH, 4);
            int fillW = barW * tierProgress / 100;
            if (fillW > 0) { g.setColor(0x4CAF50); g.fillRect(barX, barY, fillW, barH, 4); }
            
            row += 40;
            
            g.setColor(0x283593, 0.4f);
            g.fillRect(x + pad, row, w - pad * 2, 55, 5);
            
            mFont.tahoma_7b_white.drawString(g, "Chỉ số max:", x + pad + 5, row + 3, 0);
            row += 14;
            mFont.tahoma_7_green2.drawString(g, "HP: " + NinjaUtil.getMoneys(RankSystem.GetMaxHP()), x + pad + 5, row, 0);
            mFont.tahoma_7_green2.drawString(g, "MP: " + NinjaUtil.getMoneys(RankSystem.GetMaxMP()), x + w / 2, row, 0);
            row += 11;
            mFont.tahoma_7_green2.drawString(g, "Đánh: " + NinjaUtil.getMoneys(RankSystem.GetMaxDamage()), x + pad + 5, row, 0);
            mFont.tahoma_7_green2.drawString(g, "Thủ: " + RankSystem.GetMaxDefense(), x + w / 2, row, 0);
            row += 11;
            mFont.tahoma_7_green2.drawString(g, "Chí mạng: " + RankSystem.GetMaxCritical() + "%", x + pad + 5, row, 0);
            
            row += 18;
            
            g.setColor(0x283593, 0.4f);
            g.fillRect(x + pad, row, w - pad * 2, 40, 5);
            
            if (!RankSystem.IsMaxRank())
            {
                // Ẩn thông tin Tiếp rank và TN cần
                // mFont.tahoma_7b_green2.drawString(g, "Tiếp: " + RankSystem.GetNextRankName(), x + pad + 5, row + 5, 0);
                // mFont.tahoma_7_yellow.drawString(g, "Cần: " + NinjaUtil.getMoneys(RankSystem.GetPowerForNextRank()) + " TN", x + pad + 5, row + 18, 0);
            }
            else
            {
                mFont.tahoma_7b_yellow.drawString(g, "RANK CAO NHẤT!", x + w / 2, row + 15, mFont.CENTER, mFont.tahoma_7b_dark);
            }
        }
        catch { }
        
        btnExit.paint(g);
    }
    
    public override void updateKey()
    {
        base.updateKey();
        if (btnExit.isCheckClick())
        {
            GameScr.gI().switchToMe();
            GameCanvas.clearAllPointerEvent();
        }
    }
    
    public override void update() => GameScr.gI().update();
}
