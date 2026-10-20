using System;

public class SlotMachineScreen : mScreen
{
    static SlotMachineScreen instance;
    Command btnExit = new Command();
    Command btnSpin = new Command();
    Command btnTurbo = new Command(); // Lightning bolt turbo button
    bool isTurboMode = false;
    
    int x, y, w, h;
    int cellSize = 36;
    int gridRows = 4;
    int gridCols = 5;
    int gridX, gridY;
    
    // Slot machine state
    bool isSpinning = false;
    int spinFrame = 0;
    int spinDuration = 60; // frames
    int[] spinOffsets = new int[5]; // offset for each reel
    int[] reelStopFrame = new int[5]; // when each reel stops
    
    // Grid symbols (item IDs)
    int[,] grid = new int[4, 5];
    int[,] displayGrid = new int[6, 5]; // extended for animation
    
    // Symbol pool with weights
    int[] symbolPool = { 6467, 9424, 14, 15, 16, 17, 18, 19, 20, 9642 };
    
    // Win state
    // Win state
    bool hasWin = false;
    int totalWin = 0;
    int winWays = 0;
    
    int freeSpinCount = 0;
    int newFreeSpins = 0;
    
    int autoSpinCount = 0;
    int[] autoSpinOptions = { 10, 20, 30, 50, 999 };
    
    public class WinPath
    {
        public int symbol;
        public System.Collections.Generic.List<int> rows = new System.Collections.Generic.List<int>();
        public System.Collections.Generic.List<int> cols = new System.Collections.Generic.List<int>();
    }
    
    System.Collections.Generic.List<WinPath> winPaths = new System.Collections.Generic.List<WinPath>();

    public SlotMachineScreen()
    {
        RefreshLayout();
        btnExit.img = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
        InitializeGrid();
    }
    


    public void ReceiveSpinResult(int[,] resultGrid, int reward, System.Collections.Generic.List<WinPath> wins, int freeSpin, int newFree)
    {
        targetGrid = resultGrid;
        targetReward = reward;
        winPaths = wins;
        freeSpinCount = freeSpin;
        newFreeSpins = newFree;
        resultReceived = true;
        
        // Finalize stop times if they haven't been set or to ensure they stop soon
        // Logic in UpdateSpin handles the stop based on resultReceived
    }

    public void ReceiveSpinResult(int[,] resultGrid, int reward) // Legacy overload if needed
    {
        ReceiveSpinResult(resultGrid, reward, new System.Collections.Generic.List<WinPath>(), 0, 0);
    }
    

    
    private void DrawWinLines(mGraphics g)
    {
        if (winPaths == null || winPaths.Count == 0) return;
        
        // Clip to grid area to prevent lines jumping out
        g.setClip(gridX, gridY, gridCols * cellSize, gridRows * cellSize);
        
        for (int i = 0; i < winPaths.Count; i++)
        {
            WinPath path = winPaths[i];
            
            // 1. Organize points by column
            System.Collections.Generic.List<int>[] colsRows = new System.Collections.Generic.List<int>[gridCols];
            for (int c = 0; c < gridCols; c++) colsRows[c] = new System.Collections.Generic.List<int>();
            
            for (int k = 0; k < path.cols.Count; k++)
            {
                int c = path.cols[k];
                int r = path.rows[k];
                // Safety check for bounds
                if (c >= 0 && c < gridCols && r >= 0 && r < gridRows) 
                {
                    colsRows[c].Add(r);
                }
            }
            
            // 2. Draw lines
            g.setColor(GetSymbolColor(path.symbol));
            
            for (int c = 0; c < gridCols - 1; c++)
            {
                if (colsRows[c].Count > 0 && colsRows[c+1].Count > 0)
                {
                    // Connect all in C to all in C+1
                    foreach (int r1 in colsRows[c])
                    {
                        foreach (int r2 in colsRows[c+1])
                        {
                            int x1 = gridX + c * cellSize + cellSize / 2;
                            int y1 = gridY + r1 * cellSize + cellSize / 2;
                            int x2 = gridX + (c + 1) * cellSize + cellSize / 2;
                            int y2 = gridY + r2 * cellSize + cellSize / 2;
                            
                            g.drawLine(x1, y1, x2, y2);
                            // Thicken line? maybe draw offset
                            g.drawLine(x1, y1 + 1, x2, y2 + 1);
                        }
                    }
                }
            }
        }
        
        // Reset clip to full screen (or previous state)
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private int GetSymbolColor(int symbolId) {
        // Return distinct colors for clarity
        switch(symbolId % 7) {
            case 0: return 0xFF0000; // Red
            case 1: return 0x00FF00; // Green
            case 2: return 0x0000FF; // Blue
            case 3: return 0xFFFF00; // Yellow
            case 4: return 0xFF00FF; // Magenta
            case 5: return 0x00FFFF; // Cyan
            default: return 0xFFFFFF; // White
        }
    }
    
    private void RefreshLayout()
    {
        w = Math.Min(280, GameCanvas.w - 20);
        h = Math.Min(340, GameCanvas.h - 20);
        x = (GameCanvas.w - w) / 2;
        y = (GameCanvas.h - h) / 2;
        
        gridX = x + (w - cellSize * gridCols) / 2;
        gridY = y + 40;
    }
    
    private void InitializeGrid()
    {
        Random rand = new Random();
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                displayGrid[row, col] = symbolPool[rand.Next(symbolPool.Length)];
            }
        }
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                grid[row, col] = displayGrid[row + 1, col];
            }
        }
    }
    
    public static SlotMachineScreen gI()
    {
        instance ??= new SlotMachineScreen();
        return instance;
    }
    
    public override void switchToMe()
    {
        RefreshLayout();
        btnExit.x = x + w - btnExit.img.getWidth() - 3;
        btnExit.y = y + 3;
        
        btnSpin.x = x + (w - 70) / 2;
        btnSpin.y = y + h - 45;
        btnSpin.w = 70;
        btnSpin.h = 25;
        
        btnTurbo.x = btnSpin.x + btnSpin.w + 5;
        btnTurbo.y = btnSpin.y;
        btnTurbo.w = 25;
        btnTurbo.h = 25;
        
        GameScr.isPaintOther = true;
        GameCanvas.panel.hideNow();
        base.switchToMe();
    }
    
    public override void paint(mGraphics g)
    {
        GameScr.gI().paint(g);
        
        // Background
        g.setColor(0x000000, 0.6f);
        g.fillRect(x + 3, y + 4, w, h, 8);
        PopUp.paintPopUp(g, x, y, w, h, 0, true);
        
        g.setColor(0x8B0000, 0.95f);
        g.fillRect(x + 4, y + 4, w - 8, 28, 5);
        
        string title = "QUAY HU NGOC RONG";
        if (freeSpinCount > 0)
        {
            title = "FREE SPIN: " + freeSpinCount;
        }
        mFont.tahoma_7b_yellow.drawString(g, title, x + w / 2, y + 14, mFont.CENTER, mFont.tahoma_7b_dark);
        
        g.setColor(0x1a1a2e, 0.95f);
        g.fillRect(gridX - 5, gridY - 5, cellSize * gridCols + 10, cellSize * gridRows + 10, 8);
        
        DrawGrid(g);
        
        Char c = Char.myCharz();
        if (c != null)
        {
            int infoY = gridY + cellSize * gridRows + 15;
            g.setColor(0x2d2d44, 0.9f);
            g.fillRect(x + 10, infoY, w - 20, 25, 5);
            mFont.tahoma_7_yellow.drawString(g, "Thoi vang: " + GetGoldBarCount(), x + 20, infoY + 7, 0);
        }
        
        if (hasWin && !isSpinning)
        {
            int popupW = 150;
            int popupH = 60;
            int popupX = x + (w - popupW) / 2;
            int popupY = gridY + (cellSize * gridRows - popupH) / 2;
            
            g.setColor(0x006400, 0.95f);
            g.fillRect(popupX - 3, popupY - 3, popupW + 6, popupH + 6, 12);
            g.setColor(0x00FF00, 0.8f);
            g.fillRect(popupX, popupY, popupW, popupH, 10);
            
            mFont.tahoma_7b_yellow.drawString(g, "THANG!", popupX + popupW / 2, popupY + 12, mFont.CENTER);
            mFont.tahoma_7b_yellow.drawString(g, "+" + totalWin + " TV", popupX + popupW / 2, popupY + 28, mFont.CENTER);
            mFont.tahoma_7_yellow.drawString(g, winWays + " ways", popupX + popupW / 2, popupY + 44, mFont.CENTER);
            
            DrawWinLines(g);
        }
        
        if (newFreeSpins > 0 && !isSpinning)
        {
            int notifyY = y + 35;
            g.setColor(0xFFD700, 0.95f);
            g.fillRect(x + 10, notifyY, w - 20, 22, 5);
            mFont.tahoma_7b_dark.drawString(g, "+" + newFreeSpins + " VONG QUAY MIEN PHI!", x + w / 2, notifyY + 6, mFont.CENTER);
        }
        
        DrawSpinButton(g);
        DrawTurboButton(g);
        DrawAutoSpinButtons(g);
        
        if (autoSpinCount > 0)
        {
            int autoY = btnSpin.y - 18;
            g.setColor(0xFF6600, 0.9f);
            g.fillRect(btnSpin.x, autoY, btnSpin.w, 15, 4);
            mFont.tahoma_7_yellow.drawString(g, "AUTO: " + autoSpinCount, btnSpin.x + btnSpin.w / 2, autoY + 3, mFont.CENTER);
        }
        
        btnExit.paint(g);
    }
    
    private void DrawGrid(mGraphics g)
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                int cellX = gridX + col * cellSize;
                int cellY = gridY + row * cellSize;
                
                // Cell background
                g.setColor(0x16213e, 0.95f);
                g.fillRect(cellX + 1, cellY + 1, cellSize - 2, cellSize - 2, 4);
                
                int iconId = -1;
                int itemId = grid[row, col];
                
                // Try to get icon from ItemTemplate first (Legacy/Placeholder support)
                try {
                    ItemTemplate template = ItemTemplates.get((short)itemId);
                    if (template != null) {
                        iconId = template.iconID;
                    }
                } catch { }

                // If not found or specific range, treat as direct Image ID
                if (iconId == -1 || itemId > 10000) { 
                    iconId = itemId; 
                }

                if (iconId != -1)
                {
                    int iconX = cellX + cellSize / 2;
                    int iconY = cellY + cellSize / 2;
                    
                    if (isSpinning && spinFrame < reelStopFrame[col])
                    {
                        int offset = spinOffsets[col] % cellSize;
                        SmallImage.drawSmallImage(g, iconId, iconX, iconY + offset, 0, mGraphics.VCENTER | mGraphics.HCENTER);
                    }
                    else
                    {
                        SmallImage.drawSmallImage(g, iconId, iconX, iconY, 0, mGraphics.VCENTER | mGraphics.HCENTER);
                    }
                }
            }
        }
    }
    
    private void DrawSpinButton(mGraphics g)
    {
        int btnColor = isSpinning ? 0x555555 : 0xB22222;
        g.setColor(btnColor, 0.95f);
        g.fillRect(btnSpin.x, btnSpin.y, btnSpin.w, btnSpin.h, 8);
        
        g.setColor(0xFFD700);
        g.drawRect(btnSpin.x, btnSpin.y, btnSpin.w, btnSpin.h);
        
        string btnText = isSpinning ? "ĐANG QUAY..." : "QUAY (1 TV)";
        mFont.tahoma_7b_yellow.drawString(g, btnText, btnSpin.x + btnSpin.w / 2, btnSpin.y + 12, mFont.CENTER, mFont.tahoma_7b_dark);
    }
    
    private void DrawTurboButton(mGraphics g)
    {
        // Button background
        int bgColor = isTurboMode ? 0xFFD700 : 0x333333; // Yellow when active, dark when off
        g.setColor(bgColor, 0.95f);
        g.fillRect(btnTurbo.x, btnTurbo.y, btnTurbo.w, btnTurbo.h, 8);
        
        // Border
        g.setColor(isTurboMode ? 0xFFFFFF : 0x888888);
        g.drawRect(btnTurbo.x, btnTurbo.y, btnTurbo.w, btnTurbo.h);
        
        // Draw lightning bolt icon (programmatically)
        DrawLightningBolt(g, btnTurbo.x + btnTurbo.w / 2, btnTurbo.y + btnTurbo.h / 2, isTurboMode);
    }
    
    private void DrawLightningBolt(mGraphics g, int cx, int cy, bool isActive)
    {
        int color = isActive ? 0xFFFFFF : 0x666666;
        g.setColor(color);
        
        int x1 = cx + 1, y1 = cy - 8;
        int x2 = cx - 3, y2 = cy - 1;
        int x3 = cx + 1, y3 = cy - 1;
        int x4 = cx - 1, y4 = cy + 8;
        int x5 = cx + 3, y5 = cy + 1;
        int x6 = cx - 1, y6 = cy + 1;
        
        g.drawLine(x1, y1, x2, y2);
        g.drawLine(x2, y2, x3, y3);
        g.drawLine(x3, y3, x4, y4);
        g.drawLine(x4, y4, x5, y5);
        g.drawLine(x5, y5, x6, y6);
        g.drawLine(x6, y6, x1, y1);
    }
    
    int autoSpinBtnY = 0;
    int autoSpinBtnStartX = 0;
    int autoSpinBtnW = 28;
    int autoSpinBtnH = 20;
    
    private void DrawAutoSpinButtons(mGraphics g)
    {
        autoSpinBtnY = btnSpin.y + btnSpin.h + 5;
        autoSpinBtnStartX = x + 10;
        
        for (int i = 0; i < autoSpinOptions.Length; i++)
        {
            int bx = autoSpinBtnStartX + i * (autoSpinBtnW + 3);
            int opt = autoSpinOptions[i];
            
            g.setColor(autoSpinCount == opt ? 0xFF6600 : 0x444444, 0.9f);
            g.fillRect(bx, autoSpinBtnY, autoSpinBtnW, autoSpinBtnH, 4);
            
            g.setColor(0xFFD700);
            g.drawRect(bx, autoSpinBtnY, autoSpinBtnW, autoSpinBtnH);
            
            string label = opt == 999 ? "INF" : opt.ToString();
            mFont.tahoma_7_yellow.drawString(g, label, bx + autoSpinBtnW / 2, autoSpinBtnY + 5, mFont.CENTER);
        }
        
        int stopX = autoSpinBtnStartX + autoSpinOptions.Length * (autoSpinBtnW + 3);
        g.setColor(0xAA0000, 0.9f);
        g.fillRect(stopX, autoSpinBtnY, 35, autoSpinBtnH, 4);
        g.setColor(0xFFD700);
        g.drawRect(stopX, autoSpinBtnY, 35, autoSpinBtnH);
        mFont.tahoma_7_yellow.drawString(g, "STOP", stopX + 17, autoSpinBtnY + 5, mFont.CENTER);
    }
    
    public override void update()
    {
        GameScr.gI().update();
        
        if (isSpinning)
        {
            UpdateSpin();
        }
    }
    
    private int GetGoldBarCount()
    {
        Char c = Char.myCharz();
        if (c == null) return 0;
        int count = 0;
        if (c.arrItemBag != null)
        {
            for (int i = 0; i < c.arrItemBag.Length; i++)
            {
                Item item = c.arrItemBag[i];
                if (item != null && item.template != null && item.template.id == 457) // 457 is Thoi Vang
                {
                    count += item.quantity;
                }
            }
        }
        return count;
    }

    private void StartSpin()
    {
        if (GetGoldBarCount() < 1)
        {
            GameCanvas.startOKDlg("Không đủ thỏi vàng!");
            return;
        }
        
        // Send spin request to server
        Service.gI().sendSlotMachineSpin();
        
        isSpinning = true;
        hasWin = false;
        totalWin = 0;
        winWays = 0;
        spinFrame = 0;
        resultReceived = false; 
        
        // Set staggered stop times for each reel
        // Turbo mode: instant stop (no animation)
        int baseDuration = isTurboMode ? 5 : spinDuration;
        int staggerDelay = isTurboMode ? 0 : 10;
        
        for (int col = 0; col < gridCols; col++)
        {
            reelStopFrame[col] = baseDuration + col * staggerDelay;
            spinOffsets[col] = 0;
        }
    }

    // Server result
    int[,] targetGrid = new int[4, 5];
    int targetReward = 0;
    bool resultReceived = false;
 
    private void UpdateSpin()
    {
        spinFrame++;
        Random rand = new Random();
        
        // If result not received, extend spin duration
        if (!resultReceived && spinFrame > spinDuration) 
        {
            spinFrame = spinDuration; // Hold frame count
        }
        
        for (int col = 0; col < gridCols; col++)
        {
            if (spinFrame < reelStopFrame[col])
            {
                spinOffsets[col] += 36; // Fast spin
                
                // Shift symbols when offset exceeds cell size
                if (spinOffsets[col] >= cellSize)
                {
                    spinOffsets[col] = 0;
                    // Shift grid down for this column
                    for (int row = gridRows - 1; row > 0; row--)
                    {
                        grid[row, col] = grid[row - 1, col];
                    }
                    grid[0, col] = symbolPool[rand.Next(symbolPool.Length)];
                }
            }
            else if (resultReceived)
            {
                 // Snap to target if stopped
                 for (int row = 0; row < gridRows; row++)
                 {
                     grid[row, col] = targetGrid[row, col];
                 }
                 spinOffsets[col] = 0;
            }
        }
        
        // Check if all reels stopped
        bool allStopped = true;
        for (int col = 0; col < gridCols; col++)
        {
            if (spinFrame < reelStopFrame[col]) allStopped = false;
        }
        
        if (allStopped && resultReceived)
        {
            isSpinning = false;
            totalWin = targetReward;
            hasWin = targetReward > 0;
            CalculateDisplayWays();
            
            if (autoSpinCount > 0)
            {
                autoSpinCount--;
                StartSpin();
            }
        }
    }
    
    private void CalculateDisplayWays()
    {
         // Recalculate ways for UI verification/display
         winWays = 0;
         foreach (int symbol in symbolPool)
         {
             int ways = CountWaysForSymbol(symbol);
             if (ways > 0) winWays += ways;
         }
    }
    
    private int CountWaysForSymbol(int symbol)
    {
        // Count symbols in each column
        int[] colCounts = new int[gridCols];
        for (int col = 0; col < gridCols; col++)
        {
            for (int row = 0; row < gridRows; row++)
            {
                if (grid[row, col] == symbol)
                    colCounts[col]++;
            }
        }
        
        // Need at least 3 consecutive columns from left with this symbol
        int consecutiveCols = 0;
        for (int col = 0; col < gridCols; col++)
        {
            if (colCounts[col] > 0)
                consecutiveCols++;
            else
                break;
        }
        
        if (consecutiveCols < 3) return 0;
        
        // Calculate ways: multiply counts of consecutive columns
        int ways = 1;
        for (int col = 0; col < consecutiveCols; col++)
        {
            ways *= colCounts[col];
        }
        
        return ways;
    }


    
    public override void updateKey()
    {
        base.updateKey();
        
        if (btnExit.isCheckClick())
        {
            autoSpinCount = 0;
            GameScr.gI().switchToMe();
            GameCanvas.clearAllPointerEvent();
            return;
        }
        
        if (btnTurbo.isCheckClick())
        {
            isTurboMode = !isTurboMode;
            GameCanvas.clearAllPointerEvent();
            return;
        }
        
        if (GameCanvas.isPointerDown)
        {
            int tx = GameCanvas.px;
            int ty = GameCanvas.py;
            
            for (int i = 0; i < autoSpinOptions.Length; i++)
            {
                int bx = autoSpinBtnStartX + i * (autoSpinBtnW + 3);
                if (tx >= bx && tx <= bx + autoSpinBtnW && ty >= autoSpinBtnY && ty <= autoSpinBtnY + autoSpinBtnH)
                {
                    autoSpinCount = autoSpinOptions[i];
                    if (!isSpinning) StartSpin();
                    GameCanvas.clearAllPointerEvent();
                    return;
                }
            }
            
            int stopX = autoSpinBtnStartX + autoSpinOptions.Length * (autoSpinBtnW + 3);
            if (tx >= stopX && tx <= stopX + 35 && ty >= autoSpinBtnY && ty <= autoSpinBtnY + autoSpinBtnH)
            {
                autoSpinCount = 0;
                GameCanvas.clearAllPointerEvent();
                return;
            }
        }
        
        if (!isSpinning && btnSpin.isCheckClick())
        {
            StartSpin();
            GameCanvas.clearAllPointerEvent();
        }
    }
}
