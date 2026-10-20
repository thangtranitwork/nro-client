using Assets.Scripts.CustomGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.CustomGraphics;

namespace Assets.Scripts.VoiceChat
{
    class VoiceChatSettingScreen : mScreen
    {
        Command btnExit = new Command();
        static VoiceChatSettingScreen instance;
        int x, y, w, h;

        public CheckBox[] checkBoxes = new CheckBox[4];
        public NumbericSlider volumeSlider;
        public int volumeValue = 100; // Biến lưu giá trị âm lượng
        private string micStatus = "";
        private string speakerStatus = "";
        private float remoteLevel = 0f;
        private ComboBox micCombo;
        private ComboBox speakerCombo;

        private void RefreshLayout()
        {
            int pad = 10;
            int row = y + 45;
            // Combos
            micCombo.x = x + pad; micCombo.y = row; micCombo.w = w - pad * 2; micCombo.h = 24;
            row += 30;
            speakerCombo.x = x + pad; speakerCombo.y = row; speakerCombo.w = w - pad * 2; speakerCombo.h = 24;
            row += 36;
            // Checkboxes in 2 columns
            int colW = (w - pad * 2) / 2;
            checkBoxes[0].x = x + pad;             checkBoxes[0].y = 12+row;           // Loa Map
            checkBoxes[1].x = x + pad;             checkBoxes[1].y = 12 + row + 26;      // MIC Map
            checkBoxes[2].x = x + pad + colW;      checkBoxes[2].y = 12 + row;           // Loa Bang
            checkBoxes[3].x = x + pad + colW;      checkBoxes[3].y = 12 + row + 26;      // MIC Bang
            row += 70;
            // Volume
            volumeSlider.x = x + pad; volumeSlider.y = row; volumeSlider.w = w - pad * 2;
        }

        public VoiceChatSettingScreen()
        {
            x = GameCanvas.panel.xScroll + GameCanvas.panel.wScroll;
            y = 15;
            w = 220; h = 270;
            btnExit.img = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
            btnExit.x = x + w - btnExit.img.getWidth() - 5;
            btnExit.y = y + btnExit.img.getHeight() / 2;

            checkBoxes[0] = new CheckBox(x + 5, y + 60, 20, 20, setLoaMap, "Loa Map");
            checkBoxes[1] = new CheckBox(x + 5, y + 85, 20, 20, setMICMap, "MIC Map");
            checkBoxes[2] = new CheckBox(x + 5, y + 100, 20, 20, setLoaBang, "Loa Bang");
            checkBoxes[3] = new CheckBox(x + 5, y + 135, 20, 20, setMICBang, "MIC Bang");

            // Tạo slider điều chỉnh âm lượng
            volumeSlider = new NumbericSlider(x + 5, y + 158, w - 10, 25, volumeValue, 0, 200, onVolumeChanged, "Âm lượng:");

            // Combo chọn mic và loa
            var micList = new System.Collections.Generic.List<string>();
            try { foreach (var d in Microphone.devices) micList.Add(d); } catch { }
            if (micList.Count == 0) micList.Add("Không có mic");
            micCombo = new ComboBox(x + 5, y + 195, w - 10, 22, micList, OnMicSelected, "");

            var spkList = new System.Collections.Generic.List<string>();
            // Unity không có danh sách thiết bị output tiêu chuẩn; hiển thị chế độ loa
            spkList.Add("Loa mặc định");
            speakerCombo = new ComboBox(x + 5, y + 245, w - 10, 22, spkList, OnSpeakerSelected, "");
            speakerCombo.maxVisibleItems = 3;

            RefreshLayout();
        }

        public static VoiceChatSettingScreen gI()
        {
            instance ??= new VoiceChatSettingScreen();
            return instance;
        }
        public override void switchToMe()
        {
            GameScr.isPaintOther = true;
            GameCanvas.panel.hideNow();
            base.switchToMe();
            // Hook volume callback
            VoiceChatManager.OnRemoteVolumeChanged = (lvl) =>
            {
                SetRemoteLevel(lvl);
            };
        }
        public override void paint(mGraphics g)
        {
            GameScr.gI().paint(g);
            // Card background with soft shadow
            g.setColor(0x000000, 0.2f); g.fillRect(x + 2, y + 4, w, h, 8);
            PopUp.paintPopUp(g, x, y, w, h, 0, true);
            // Header bar
            g.setColor(0x1b2a41, 0.95f); g.fillRect(x + 8, y + 8, w - 16, 24, 6);
            mFont.tahoma_7b_white.drawString(g, "Cấu hình VoiceChat", x + w / 2, y + 15, mFont.CENTER);
            foreach (CheckBox checkBox in checkBoxes)
            {
                checkBox?.paint(g);
            }

            // Section: Thiết bị
            g.setColor(0x1b2a41, 0.18f); g.fillRect(x + 8, y + 35, w - 16, 70, 6);
            mFont.tahoma_7b_white.drawString(g, "Thiết bị", x + 33, y + 35, mFont.LEFT, mFont.tahoma_7b_dark);
            

            // Section: Tùy chọn
            g.setColor(0x1b2a41, 0.18f); g.fillRect(x + 8, y + 110, w - 16, 63, 6);
            mFont.tahoma_7b_white.drawString(g, "Tùy chọn", x + 33, y + 111, mFont.LEFT, mFont.tahoma_7b_dark);
            foreach (CheckBox checkBox in checkBoxes) checkBox?.paint(g);

            // Vẽ volume slider
            volumeSlider?.paint(g);

            // Device status and simple VU meter
            string status = $"Mic: {micStatus}";
            mFont.tahoma_7b_dark.drawString(g, status, x + w/2, y + h - 53, mFont.CENTER,mFont.tahoma_7b_white);
            status = $"Loa: {speakerStatus}";
            mFont.tahoma_7b_dark.drawString(g, status, x + w / 2, y + h - 40, mFont.CENTER, mFont.tahoma_7b_white);

            // Draw simple remote level bar
            int barW = w - 20; int barH = 8; int barX = x + 10; int barY = y + h - 22;
            g.setColor(0x333333); g.fillRect(barX, barY, barW, barH,4);
            int fill = (int)(barW * Mathf.Clamp01(remoteLevel));
            if (fill > 0) { g.setColor(0x00FF66); g.fillRect(barX, barY, fill, barH,4); }
            speakerCombo?.paint(g);
            micCombo?.paint(g);
           
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

            // Update volume slider
            volumeSlider?.updateKey();

            // Update combos
            micCombo?.updateKey();
            speakerCombo?.updateKey();
        }
        public override void update()
        {
            GameScr.gI().update();
            UpdateDeviceStatus();
        }

        private void UpdateDeviceStatus()
        {
            try
            {
                var mics = Microphone.devices;
                micStatus = (mics != null && mics.Length > 0) ? mics[0] : "Không có";
            }
            catch { micStatus = "Lỗi mic"; }

            try
            {
                // Unity không expose danh sách thiết bị loa, hiển thị trạng thái AudioListener
                speakerStatus = AudioListener.pause ? "Tạm dừng" : "Đang hoạt động";
            }
            catch { speakerStatus = "Lỗi loa"; }
        }
        public void syncBool()
        {
            VoiceEnable.enableLoaBang = checkBoxes[2].isCheck;
            VoiceEnable.enableMICBang = checkBoxes[3].isCheck;
            VoiceEnable.enableLoaMap = checkBoxes[0].isCheck;
            VoiceEnable.enableMICMap = checkBoxes[1].isCheck;
        }

        public void setLoaBang()
        {
            Debug.LogError("123");
            bool flag = checkBoxes[2].isCheck;
            VoiceEnable.enableLoaBang = flag;
            if (flag)
            {

            }
            else
            {
                checkBoxes[3].setCheck(false);
                VoiceEnable.enableMICBang = false;
                if (!VoiceEnable.enableMICMap)
                {
                    VoiceChatRecorder.gI().EnableVoiceChat = false;
                    VoiceEnable.enableMICBang = false;
                }
            }
            syncBool();
            Service.gI().sendSettingVoiceChat();
        }
        public void setMICBang()
        {
            Debug.LogError("234");
            bool flag = checkBoxes[3].isCheck;
            VoiceEnable.enableMICBang = flag;
            if (flag)
            {
                checkBoxes[2].setCheck(true);
                VoiceChatRecorder.gI().EnableVoiceChat = true;
            }
            else
            {
                if (!VoiceEnable.enableMICMap)
                {
                    VoiceChatRecorder.gI().EnableVoiceChat = false;
                }
            }
            syncBool();
            Service.gI().sendSettingVoiceChat();
        }

        public void setLoaMap()
        {
            Debug.LogError("345");
            bool flag = checkBoxes[0].isCheck;
            VoiceEnable.enableLoaMap = flag;
            if (flag)
            {

            }
            else
            {
                checkBoxes[1].setCheck(false);
                VoiceEnable.enableMICMap = false;
                if (!VoiceEnable.enableMICBang)
                {
                    VoiceChatRecorder.gI().EnableVoiceChat = false;
                }
            }
            syncBool();
            Service.gI().sendSettingVoiceChat();
        }
        public void setMICMap()
        {
            Debug.LogError("456");
            bool flag = checkBoxes[1].isCheck;
            VoiceEnable.enableMICMap = flag;
            if (flag)
            {
                VoiceChatRecorder.gI().EnableVoiceChat = true;
                checkBoxes[0].setCheck(true);
            }
            else
            {
                if (!VoiceEnable.enableMICBang)
                {
                    VoiceChatRecorder.gI().EnableVoiceChat = false;
                }
            }
            syncBool();
            Service.gI().sendSettingVoiceChat();
        }

        // Callback khi âm lượng thay đổi
        public void onVolumeChanged(int newVolume)
        {
            volumeValue = newVolume;
            Debug.Log($"Âm lượng đã thay đổi: {volumeValue}");
            // Bạn có thể thêm logic xử lý âm lượng ở đây
        }

        // Nhận remote volume rms để vẽ VU meter
        public void SetRemoteLevel(float level)
        {
            remoteLevel = Mathf.Clamp01(level);
        }

        private void OnMicSelected(int index)
        {
            try
            {
                var devices = Microphone.devices;
                if (devices != null && devices.Length > 0 && index >= 0 && index < devices.Length)
                {
                    // Áp dụng ngay cho VoiceChatRecorder nếu đang bật
                    // Lưu ý: Microphone API của Unity không cho set default trực tiếp; bạn cần stop/start với device name
                    if (VoiceChatRecorder.instance != null && VoiceChatRecorder.instance.IsVoiceChatActive)
                    {
                        VoiceChatRecorder.instance.StopVoiceChat();
                        // Tạm thời VoiceChatRecorder sử dụng device đầu tiên; có thể mở rộng API để chọn device cụ thể
                        VoiceChatRecorder.instance.StartVoiceChat();
                    }
                    micStatus = devices[index];
                }
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }

        private void OnSpeakerSelected(int index)
        {
            // Unity không hỗ trợ chọn thiết bị output runtime trên mọi nền tảng.
            // Ở đây chỉ hiển thị thông tin và có thể toggle AudioListener.pause như mô phỏng.
            if (index == 1) AudioListener.pause = true; else AudioListener.pause = false;
            speakerStatus = AudioListener.pause ? "Tạm dừng" : "Đang hoạt động";
        }

    }
}
