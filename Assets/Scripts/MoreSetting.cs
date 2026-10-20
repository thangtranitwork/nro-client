using Assets.Scripts.CustomGraphics;
using System;
using UnityEngine;

public class MoreSetting : mScreen
{
    Command btnExit = new Command();
    static MoreSetting instance;
    int x, y, w, h;

    // Các checkbox cho 8 setting từ hình ảnh
    internal CheckBox[] checkBoxes = new CheckBox[7];
    
    // Các biến bool để lưu trữ giá trị
    public static bool hideStrangers = false;           // Ẩn người lạ - Chỉ hiện thị người trong Bang hội
    public static bool hideAuraOthers = false;           // Ẩn hào quang - Không hiển thị hào quang của người chơi khác
    public static bool hideAuraAll = false;             // Ẩn hào quang - Ấn mọi hào quang kẻ cả bản thân
    public static bool soundEnabled = true;             // Âm thanh - Bật tắt âm thanh
    public static bool highFPS = false;                 // FPS Cao - Mượt hơn nhưng sẽ hao pin hon
    public static bool autoUpdateArea = false;          // Cập nhật khu vục - Tự động cập nhật người trong khu
    public static bool showDiscipleInfo = true;         // Thông tin đệ tử - In thông tin đệ tử ra màn hình
    public static bool autoCrystallize = false;         // Tự động Pha lê hóa - Auto pha lê tại Bà Hạt Mít

    public MoreSetting()
    {
       
        y = 15;
        w = 350; 
        h = 280;
        x = GameCanvas.w / 2 - w/2;
        btnExit.img = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
        btnExit.x = x + w - btnExit.img.getWidth() - 5;
        btnExit.y = y + btnExit.img.getHeight() / 2;

        // Khởi tạo các checkbox với vị trí và callback
        int startY = y + 50;
        int spacing = 28;
        
        checkBoxes[0] = new CheckBox(x + 15, startY, 20, 20, setHideStrangers, "Ẩn người lạ - Chỉ hiển thị người trong Bang hội");
        checkBoxes[1] = new CheckBox(x + 15, startY + spacing, 20, 20, setHideAuraOthers, "Ẩn hào quang - Không hiển thị hào quang của người chơi khác");
        checkBoxes[2] = new CheckBox(x + 15, startY + spacing * 2, 20, 20, setHideAuraAll, "Ẩn hào quang - Ấn mọi hào quang kẻ cả bản thân");
        checkBoxes[3] = new CheckBox(x + 15, startY + spacing * 3, 20, 20, setSoundEnabled, "Âm thanh - Bật tắt âm thanh");
        checkBoxes[4] = new CheckBox(x + 15, startY + spacing * 4, 20, 20, setHighFPS, "FPS Cao - Mượt hơn nhưng sẽ hao pin hon");
        checkBoxes[5] = new CheckBox(x + 15, startY + spacing * 5, 20, 20, setAutoUpdateArea, "Cập nhật khu vục - Tự động cập nhật người trong khu");
        checkBoxes[6] = new CheckBox(x + 15, startY + spacing * 6, 20, 20, setShowDiscipleInfo, "Thông tin đệ tử - In thông tin đệ tử ra màn hình");
//        checkBoxes[7] = new CheckBox(x + 15, startY + spacing * 7, 20, 20, setAutoCrystallize, "Tự động Pha lê hóa - Auto pha lê tại Bà Hạt Mít");

        // Load settings từ RMS
        loadSettings();
    }

    public static MoreSetting gI()
    {
        instance ??= new MoreSetting();
        return instance;
    }

    public override void switchToMe()
    {
        GameScr.isPaintOther = true;
        GameCanvas.panel.hideNow();
        base.switchToMe();
        loadSettings(); // Load lại settings khi mở
    }

    public override void paint(mGraphics g)
    {
        GameScr.gI().paint(g);
        
        // Vẽ shadow
        g.setColor(0x000000, 0.2f); 
        g.fillRect(x + 2, y + 4, w, h, 8);
        
        // Vẽ popup background
        PopUp.paintPopUp(g, x, y, w, h, 0, true);
        
        // Vẽ header
        g.setColor(0x8B4513, 0.95f); 
        g.fillRect(x + 8, y + 8, w - 16, 30, 6);
        mFont.tahoma_7b_white.drawString(g, "Cài đặt nâng cao", x + w / 2, y + 20, mFont.CENTER);
        
        // Vẽ background cho danh sách settings
        g.setColor(0xF5DEB3, 0.9f); 
        g.fillRect(x + 8, y + 45, w - 16, h - 55, 6);
        
        // Vẽ các checkbox
        foreach (CheckBox checkBox in checkBoxes)
        {
            checkBox?.paint(g);
        }
        
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
        
        foreach (CheckBox checkBox in checkBoxes)
        {
            checkBox?.updateKey();
        }
    }

    public override void update()
    {
        GameScr.gI().update();
    }
    public static bool checkPaint(Char @char)
    {
        if (@char == null)
        {
            return false;
        }
        if (!hideStrangers)
        {
            return true;
        }
        if (@char.cTypePk == 5)
        {
            return true;
        }
        if (@char.cTypePk == 0)
        {
            if (Char.myCharz().clan != null && @char.clanID != Char.myCharz().clan.ID)
            {
                return false;
            }
        }
        return true;
    }
    // Các callback methods cho checkbox
    public void setHideStrangers()
    {
        hideStrangers = checkBoxes[0].isCheck;
        saveSettings();
    }

    public void setHideAuraOthers()
    {
        hideAuraOthers = checkBoxes[1].isCheck;
        saveSettings();
    }

    public void setHideAuraAll()
    {
        hideAuraAll = checkBoxes[2].isCheck;
        saveSettings();
    }

    public void setSoundEnabled()
    {
        soundEnabled = checkBoxes[3].isCheck;
        saveSettings();
    }

    public void setHighFPS()
    {
        highFPS = checkBoxes[4].isCheck;
        saveSettings();
        if (highFPS)
        {
            Application.targetFrameRate = 150;
        }
        else
        {
            Application.targetFrameRate = 90;
        }
    }

    public void setAutoUpdateArea()
    {
        autoUpdateArea = checkBoxes[5].isCheck;
        saveSettings();
    }

    public void setShowDiscipleInfo()
    {
        showDiscipleInfo = checkBoxes[6].isCheck;
        saveSettings();
    }

    public void setAutoCrystallize()
    {
        autoCrystallize = checkBoxes[7].isCheck;
        saveSettings();
    }

    // Lưu settings vào RMS
    public void saveSettings()
    {
        try
        {
            // Tạo byte array chứa tất cả settings
            sbyte[] data = new sbyte[7];
            data[0] = (sbyte)(hideStrangers ? 1 : 0);
            data[1] = (sbyte)(hideAuraOthers ? 1 : 0);
            data[2] = (sbyte)(hideAuraAll ? 1 : 0);
            data[3] = (sbyte)(soundEnabled ? 1 : 0);
            data[4] = (sbyte)(highFPS ? 1 : 0);
            data[5] = (sbyte)(autoUpdateArea ? 1 : 0);
            data[6] = (sbyte)(showDiscipleInfo ? 1 : 0);
          //  data[7] = (sbyte)(autoCrystallize ? 1 : 0);

            // Gửi lên server để lưu
            Rms.saveRMS("more_settings", data);
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi khi lưu settings: " + ex.Message);
        }
    }

    // Load settings từ RMS
    public void loadSettings()
    {
        try
        {
            // Yêu cầu load settings từ server
            updateSettingsFromRMS(Rms.loadRMS("more_settings"));
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi khi load settings: " + ex.Message);
        }
    }

    // Method để cập nhật UI khi nhận được data từ server
    public void updateSettingsFromRMS(sbyte[] data)
    {
        if (data != null && data.Length >= 7)
        {
            hideStrangers = data[0] == 1;
            hideAuraOthers = data[1] == 1;
            hideAuraAll = data[2] == 1;
            soundEnabled = data[3] == 1;
            highFPS = data[4] == 1;
            autoUpdateArea = data[5] == 1;
            showDiscipleInfo = data[6] == 1;
         //   autoCrystallize = data[7] == 1;

            // Cập nhật UI
            if (checkBoxes[0] != null) checkBoxes[0].setCheck(hideStrangers);
            if (checkBoxes[1] != null) checkBoxes[1].setCheck(hideAuraOthers);
            if (checkBoxes[2] != null) checkBoxes[2].setCheck(hideAuraAll);
            if (checkBoxes[3] != null) checkBoxes[3].setCheck(soundEnabled);
            if (checkBoxes[4] != null) checkBoxes[4].setCheck(highFPS);
            if (checkBoxes[5] != null) checkBoxes[5].setCheck(autoUpdateArea);
            if (checkBoxes[6] != null) checkBoxes[6].setCheck(showDiscipleInfo);
            //if (checkBoxes[7] != null) checkBoxes[7].setCheck(autoCrystallize);

            if (highFPS)
            {
                Application.targetFrameRate = 150;
            }
            else
            {
                Application.targetFrameRate = 90;
            }
        }
    }
}