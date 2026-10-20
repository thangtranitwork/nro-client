using UnityEngine;
using Concentus.Structs;
using Concentus.Enums;
using System.Threading.Tasks;
using Assets.Scripts.VoiceChat.VoiceChatHandle;

namespace Assets.Scripts.VoiceChat
{
    class VoiceGlobalChatScreen : mScreen
    {
        static VoiceGlobalChatScreen instance;
        public static VoiceGlobalChatScreen gI() => instance ?? (instance = new VoiceGlobalChatScreen());

        private System.Collections.Generic.List<VoiceChatMessage> messages = new System.Collections.Generic.List<VoiceChatMessage>();

        int x, y, w, h;
        Command btnExit = new Command();
        Command btnMIC = new Command();

        bool focus;
        bool hasDown;
        bool isRecording;

        // Thu âm độc lập, tách khỏi VoiceChatRecorder realtime
        private string micDeviceName;
        private AudioClip localRecordingClip;
        private int localRecordedSamples;
        private int localSampleRate = 16000;
        private bool shouldResumeRealtimeAfter;

        int xM, yM, wM, hM;
        long timeRecorder;
        long totalTime;
        byte[] finalData;
        // Thêm biến cho hiệu ứng amply
        private float[] audioLevels = new float[20]; // Lưu lịch sử âm thanh
        private int currentLevelIndex = 0;
        private float maxAmplitude = 0f;
        private float smoothedAmplitude = 0f;

        // Màu sắc cho các thanh amply
        private int[] barColors = new int[]
        {
            0x00FF00, // Xanh lá - mức thấp
            0x40FF00,
            0x80FF00,
            0xBFFF00,
            0xFFFF00, // Vàng - mức trung bình
            0xFFBF00,
            0xFF8000,
            0xFF4000,
            0xFF2000,
            0xFF0000  // Đỏ - mức cao
        };

        // Noise Reduction Settings cho Global Chat
        private bool enableNoiseReduction = false;
        private bool enableNoiseGate = false;
        private bool enableHighPassFilter = false;
        private bool enableLowPassFilter = false;
        private float noiseGateThreshold = 0.007f;
        private float highPassCutoff = 60f;
        private float lowPassCutoff = 8000f;
        private float targetRmsForAGC = 0.2f;

        // Noise Reduction Filters
        private AdaptiveNoiseGate noiseGate;
        private HighPassFilter highPassFilter;
        private LowPassFilter lowPassFilter;

        // Async Opus encoding for Global to avoid blocking main thread
        private static OpusEncoder globalOpusEncoder;
        private const int GlobalOpusFrameMs = 20;
        private static int globalOpusFrameSize;
        private static System.Threading.Tasks.Task encodeTask;
        private static byte[] encodedGlobalData;
        public static bool isAutoPlay;
        // Utilities: chuyển stereo -> mono, resample
        private static float[] ToMono(float[] input, int channels)
        {
            if (channels <= 1) return input;
            int frames = input.Length / channels;
            float[] mono = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                float sum = 0f;
                for (int c = 0; c < channels; c++) sum += input[i * channels + c];
                mono[i] = sum / channels;
            }
            return mono;
        }

        private static float[] ResampleLinear(float[] input, int srcRate, int dstRate)
        {
            if (srcRate == dstRate || input == null || input.Length == 0) return input;
            double ratio = (double)dstRate / srcRate;
            int outLen = Mathf.Max(1, (int)System.Math.Ceiling(input.Length * ratio));
            float[] output = new float[outLen];
            for (int i = 0; i < outLen; i++)
            {
                double srcPos = i / ratio;
                int i0 = (int)System.Math.Floor(srcPos);
                int i1 = System.Math.Min(i0 + 1, input.Length - 1);
                double t = srcPos - i0;
                float s0 = input[i0];
                float s1 = input[i1];
                output[i] = (float)(s0 + (s1 - s0) * t);
            }
            return output;
        }

        private static void ApplyFadeOut(float[] samples, int sampleRate, float fadeMs = 5f)
        {
            if (samples == null || samples.Length == 0) return;
            int fadeSamples = Mathf.Clamp((int)(sampleRate * (fadeMs / 1000f)), 1, samples.Length);
            for (int i = 0; i < fadeSamples; i++)
            {
                int idx = samples.Length - 1 - i;
                if (idx < 0) break;
                float w = (float)i / fadeSamples;
                float gain = 1f - w;
                samples[idx] *= gain;
            }
        }

        public VoiceGlobalChatScreen()
        {
            w = GameCanvas.w;
            h = GameCanvas.h;
            x = 0;
            y = 0;

            btnExit.img = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
            btnExit.x = x + w - btnExit.img.getWidth() - 5;
            btnExit.y = y + btnExit.img.getHeight() / 2;

            btnMIC.img = GameCanvas.loadImage("/mainImage/iconmic.png");

            wM = hM = 60; // Tăng kích thước nút mic
            xM = x + (w >> 1) - (wM >> 1);
            yM = h - 120; // Điều chỉnh vị trí nút mic để tránh đè lên thanh amply cao hơn
            btnMIC.x = xM + (wM >> 1) - (btnMIC.img.getWidth() >> 1);
            btnMIC.y = yM + (hM >> 1) - (btnMIC.img.getHeight() >> 1);

            // Khởi tạo mảng âm thanh
            for (int i = 0; i < audioLevels.Length; i++)
            {
                audioLevels[i] = 0f;
            }

            // Init global Opus encoder once
            if (globalOpusEncoder == null)
            {
                int rate = 16000;
                globalOpusFrameSize = Mathf.RoundToInt(rate * (GlobalOpusFrameMs / 1000f));
                globalOpusEncoder = OpusEncoder.Create(rate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
                globalOpusEncoder.Bitrate = 24000; // hơi cao hơn để giảm méo ở câu dài
                globalOpusEncoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
            }
        }

        public override void switchToMe()
        {
            GameScr.isPaintOther = true;
            GameCanvas.panel.hideNow();
            base.switchToMe();
        }

        public void AddMessage(VoiceChatMessage msg)
        {
            if (messages.Count >= 20) messages.RemoveAt(0);
            messages.Add(msg);
        }

        private float GetVoiceAmplitude()
        {
            if (!isRecording || localRecordingClip == null)
                return 0f;

            int sampleCount = 1024;
            float[] samples = new float[sampleCount];
            if (string.IsNullOrEmpty(micDeviceName)) return 0f;
            int position = Microphone.GetPosition(micDeviceName);
            if (position <= 0 || position < sampleCount)
                return smoothedAmplitude * 0.95f;

            int start = position - sampleCount;
            if (start < 0) start = 0;
            localRecordingClip.GetData(samples, start);
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
            float rms = Mathf.Sqrt(sum / samples.Length);
            float amplitude = rms * 15f;

            // Cập nhật lịch sử âm thanh
            audioLevels[currentLevelIndex] = amplitude;
            currentLevelIndex = (currentLevelIndex + 1) % audioLevels.Length;

            // Tính amplitude trung bình để làm mượt
            float averageAmplitude = 0f;
            for (int i = 0; i < audioLevels.Length; i++)
            {
                averageAmplitude += audioLevels[i];
            }
            averageAmplitude /= audioLevels.Length;

            // Làm mượt amplitude
            smoothedAmplitude = Mathf.Lerp(smoothedAmplitude, averageAmplitude, Time.deltaTime * 8f);

            // Cập nhật amplitude tối đa
            if (amplitude > maxAmplitude)
                maxAmplitude = amplitude;
            else
                maxAmplitude = Mathf.Lerp(maxAmplitude, amplitude, Time.deltaTime * 2f);

            return smoothedAmplitude;
        }

        public override void paint(mGraphics g)
        {
            GameScr.gI().paint(g);
            g.setColor(0x000000, 0.7f);
            g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
            if (isRecording)
            {
                float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
                int pulseColor = (int)(pulse * 255);
                g.setColor((pulseColor << 16) | 0x4040, 0.9f);
            }
            else
            {
                g.setColor(focus ? 0xFFFFFF : 0xB4B4B4, 0.8f);
            }
            g.fillRect(xM, yM, wM, hM, 100);
            btnMIC.paint(g);
            if (isRecording)
            {
                float amplitude = GetVoiceAmplitude();
                DrawAmplitudeBars(g, amplitude);
                DrawWaveform(g);
                float time = (float)(totalTime / 1000f);
                string timeStr = time.ToString("0.0s");


                mFont.tahoma_7b_white.drawString(g, timeStr, xM + (wM >> 1) + 2, y + h - 40, mGraphics.VCENTER | mGraphics.HCENTER);
                string recordingText = "ĐANG THU ÂM";

                mFont.tahoma_7b_white.drawString(g, recordingText, w / 2, 50, mGraphics.VCENTER | mGraphics.HCENTER);
                mFont.tahoma_7b_white.drawString(g, "NHẤN RA NGOÀI MIC ĐỂ TẮT", w / 2, 10, mGraphics.VCENTER | mGraphics.HCENTER);
            }
            else
            {
                string instructionText = "Giữ để thu âm";
                g.setColor(0xCCCCCC);
                mFont.tahoma_7b_white.drawString(g, instructionText, xM + (wM >> 1) + 2, y + h - 40, mGraphics.VCENTER | mGraphics.HCENTER);
            }
        }

        /// <summary>
        /// Vẽ thanh âm lượng với gradient và hiệu ứng đẹp
        /// </summary>
        private void DrawAmplitudeBars(mGraphics g, float amplitude)
        {
            int barAreaX = x + w / 2 - 120;
            int barAreaY = y + h - 180;
            int barAreaWidth = 240;
            int barAreaHeight = 50;
            int barCount = 20;
            int barWidth = barAreaWidth / barCount;
            int spacing = 1;

            float normalizedAmplitude = Mathf.Clamp01(amplitude);

            for (int i = 0; i < barCount; i++)
            {
                float barThreshold = (float)(i + 1) / barCount;
                float barAmplitude = Mathf.Max(0f, (normalizedAmplitude * barCount - i) / 1.5f);
                barAmplitude = Mathf.Clamp01(barAmplitude);

                int barHeight = (int)(barAmplitude * barAreaHeight);
                if (barHeight < 3 && normalizedAmplitude > barThreshold * 0.5f)
                    barHeight = 3;

                if (barHeight > 0)
                {
                    int colorIndex = Mathf.Min(i / 2, barColors.Length - 1);
                    int baseColor = barColors[colorIndex];
                    float brightness = 0.6f + barAmplitude * 0.4f;
                    g.setColor(baseColor, brightness);

                    int barX = barAreaX + i * barWidth;
                    int barY = barAreaY + barAreaHeight - barHeight;
                    g.fillRect(barX + spacing / 2, barY, barWidth - spacing, barHeight, 2);
                    if (barHeight > 6)
                    {
                        g.setColor(0xFFFFFF, brightness * 0.6f);
                        g.fillRect(barX + spacing / 2, barY, barWidth - spacing, 3);
                    }
                }
            }


        }

        /// <summary>
        /// Vẽ sóng âm thanh phía trên
        /// </summary>
        private void DrawWaveform(mGraphics g)
        {
            int waveX = x + w / 2 - 100;
            int waveY = y + h - 220;
            int waveWidth = 200;
            int waveHeight = 30;
            g.setColor(0x00FFFF, 0.8f);
            int segmentWidth = waveWidth / audioLevels.Length;

            for (int i = 0; i < audioLevels.Length - 1; i++)
            {
                int index1 = (currentLevelIndex + i) % audioLevels.Length;
                int index2 = (currentLevelIndex + i + 1) % audioLevels.Length;

                float level1 = Mathf.Clamp01(audioLevels[index1]);
                float level2 = Mathf.Clamp01(audioLevels[index2]);

                int y1 = waveY + waveHeight / 2 - (int)(level1 * waveHeight / 2);
                int y2 = waveY + waveHeight / 2 - (int)(level2 * waveHeight / 2);

                int x1 = waveX + i * segmentWidth;
                int x2 = waveX + (i + 1) * segmentWidth;
                g.drawLine(x1, y1, x2, y2);
                if (level1 > 0.1f)
                {
                    g.fillRect(x1 - 1, y1 - 1, 2, 2, 100);
                }
            }
            g.setColor(0x404040);
            g.drawLine(waveX, waveY + waveHeight / 2, waveX + waveWidth, waveY + waveHeight / 2);
        }

        public override void updateKey()
        {
            if (GameCanvas.currentDialog != null) return;
            if (GameCanvas.isPointerHoldIn(xM, yM, wM, hM))
            {
                focus = true;
                if (GameCanvas.isPointerDown)
                {
                    if (!hasDown)
                        StartRecording();
                    isRecording = true;
                }
                if (!GameCanvas.isPointerDown)
                    StopRecording();
            }
            else
            {
                focus = false;
                if (GameCanvas.isPointerClick)
                {
                    clearTab();
                    GameCanvas.clearAllPointerEvent();
                }
            }

            base.updateKey();
        }
        public void clearTab()
        {
            GameCanvas.panel.chatTField.isShow = false;
            GameCanvas.panel.chatTField.parentScreen.onCancelChat();
            GameScr.gI().switchToMe();

        }
        public override void update()
        {
            GameScr.gI().update();

            if (isRecording)
            {
                totalTime = mSystem.currentTimeMillis() - timeRecorder;

                // Giới hạn thời gian thu âm tối đa (60 giây)
                if (totalTime > 30000)
                {
                    StopRecording();
                }
            }

            // Drain encoder task if needed (kept here for backward compatibility)
            //VoiceGlobalChatScreen.UpdateTasksOpus();

            base.update();
        }

        // Cho phép gọi từ main update để đảm bảo encode hoàn tất vẫn gửi được dù không ở màn hình này
        public static void UpdateTasksOpus()
        {
            if (encodeTask != null && encodeTask.IsCompleted)
            {
                try
                {
                    if (encodedGlobalData != null && encodedGlobalData.Length > 0)
                    {
                        Service.gI().sendVoiceChat(encodedGlobalData, 4,lastStr);
                        Debug.Log($"Global Opus async sent: {encodedGlobalData.Length} bytes");
                    }
                }
                finally
                {
                    encodedGlobalData = null;
                    encodeTask = null;
                }
            }
        }

        private void StartRecording()
        {
            focus = hasDown = true;
            timeRecorder = mSystem.currentTimeMillis();
            maxAmplitude = 0f;
            smoothedAmplitude = 0f;

            // Reset lịch sử âm thanh
            for (int i = 0; i < audioLevels.Length; i++)
            {
                audioLevels[i] = 0f;
            }
            currentLevelIndex = 0;

            Debug.LogError("recording");

            // Tạm dừng VoiceChatRecorder realtime nếu đang chạy để tránh xung đột Microphone
            shouldResumeRealtimeAfter = false;
            if (VoiceChatRecorder.instance != null && VoiceChatRecorder.instance.IsVoiceChatActive)
            {
                shouldResumeRealtimeAfter = true;
                VoiceChatRecorder.instance.StopVoiceChat();
            }

            // Chọn thiết bị mic
            if (Microphone.devices.Length == 0)
            {
                GameCanvas.startOKDlg("Không tìm thấy microphone!");
                return;
            }
            micDeviceName = Microphone.devices[0];

            // Lấy capabilities và chọn sample rate phù hợp
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(micDeviceName, out minFreq, out maxFreq);
            int targetFreq = localSampleRate;
            if (maxFreq > 0)
            {
                targetFreq = Mathf.Clamp(localSampleRate, minFreq, maxFreq);
            }

            // Bắt đầu thu âm cục bộ (không loop)
            localRecordingClip = Microphone.Start(micDeviceName, false, 60, targetFreq);
            localRecordedSamples = 0;

            // Khởi tạo bộ lọc nhiễu cho Global Chat
            if (enableNoiseReduction)
            {
                noiseGate = enableNoiseGate ? new AdaptiveNoiseGate(noiseGateThreshold, targetFreq) : null;
                highPassFilter = enableHighPassFilter ? new HighPassFilter(highPassCutoff, targetFreq) : null;
                lowPassFilter = enableLowPassFilter ? new LowPassFilter(lowPassCutoff, targetFreq) : null;
            }
        }
        static string lastStr;
        private void StopRecording()
        {
            GameCanvas.clearAllPointerEvent();
            float time = (float)(totalTime / 1000f);
            string timeStr = lastStr = time.ToString("0.0s");
             
            GameCanvas.startYesNoDlg
                (
                $"Bạn Có Chắc Chắn Muốn Gửi\nĐoạn thu âm dài <color=red>{timeStr}</color>?",
                new Command("Gửi", () => { Handle(); }),
                new Command("Không", () =>
                {
                    GameCanvas.currentDialog = null;
                }));
            hasDown = focus = isRecording = false;
            totalTime = timeRecorder = 0;

            // Reset amplitude
            smoothedAmplitude = 0f;
            maxAmplitude = 0f;

            Debug.LogError("stopRecording");

            // Kết thúc thu âm cục bộ và lấy số samples thực
            if (!string.IsNullOrEmpty(micDeviceName))
            {
                localRecordedSamples = Microphone.GetPosition(micDeviceName);
                Microphone.End(micDeviceName);
            }

            // Sau khi dừng, nếu trước đó realtime đang chạy thì KHÔNG bật lại ngay,
            // chờ xử lý xong (Handle) rồi mới khởi động lại để tránh lấy mic trong lúc chuyển đổi.
        }
        public void Handle()
        {
            try
            {
                if (localRecordingClip != null && localRecordedSamples > 0)
                {
                    int channels = localRecordingClip.channels;
                    int frequency = localRecordingClip.frequency;

                    // Bảo vệ: số samples thực tế không vượt dung lượng clip
                    int samplesToRead = Mathf.Min(localRecordedSamples, localRecordingClip.samples);
                    float[] audioSamples = new float[samplesToRead * channels];
                    localRecordingClip.GetData(audioSamples, 0);

                    // Chuẩn hóa: luôn phát ở 16kHz, mono để đồng bộ với trình phát
                    int targetRate = 16000;
                    float[] mono = channels > 1 ? ToMono(audioSamples, channels) : audioSamples;
                    float[] resampled = frequency != targetRate ? ResampleLinear(mono, frequency, targetRate) : mono;

                    // Áp dụng fade-out ngắn để đảm bảo không bị cắt đột ngột cuối gói
                    ApplyFadeOut(resampled, targetRate, 8f);

                    // Áp dụng lọc nhiễu và AGC cho Global Chat (nếu bật)
                    if (enableNoiseReduction)
                    {
                        if (highPassFilter != null) highPassFilter.ProcessSamples(resampled);
                        if (lowPassFilter != null) lowPassFilter.ProcessSamples(resampled);
                        if (noiseGate != null) noiseGate.ProcessSamples(resampled);
                    }

                    // AGC đơn giản để ổn định âm lượng
                    float rms = 0f;
                    for (int i = 0; i < resampled.Length; i++) rms += resampled[i] * resampled[i];
                    rms = Mathf.Sqrt(rms / Mathf.Max(1, resampled.Length));
                    if (rms > 0f)
                    {
                        float gain = Mathf.Clamp(targetRmsForAGC / rms, 0.1f, 3.0f);
                        for (int i = 0; i < resampled.Length; i++)
                        {
                            float v = resampled[i] * gain;
                            resampled[i] = Mathf.Clamp(v, -1f, 1f);
                        }
                    }

                    // Chuyển đổi sang PCM16 theo số samples thực tế
                    byte[] pcmData = new byte[resampled.Length * 2];
                    for (int i = 0; i < resampled.Length; i++)
                    {
                        short sample = (short)(Mathf.Clamp(resampled[i], -1f, 1f) * 32767f);
                        pcmData[i * 2] = (byte)(sample & 0xFF);
                        pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
                    }

                    // Nén Opus (Concentus) cho Global (type 4), frame 20ms length-prefixed
                    // Encode Opus off the main thread, then send in update loop
                    var resampledCopy = resampled; // capture for Task
                    int tr = targetRate;
                    encodeTask = System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            // Convert to short[]
                            short[] pcmShort = new short[resampledCopy.Length];
                            for (int i = 0; i < resampledCopy.Length; i++)
                            {
                                pcmShort[i] = (short)(Mathf.Clamp(resampledCopy[i], -1f, 1f) * 32767f);
                            }
                            byte[] encodeBuf = new byte[4000];
                            using (var ms = new System.IO.MemoryStream())
                            {
                                int offset = 0;
                                int frameSize = globalOpusFrameSize; // based on 16kHz
                                while (offset + frameSize <= pcmShort.Length && GameCanvas.gameStarted)
                                {
                                    int encoded = globalOpusEncoder.Encode(pcmShort, offset, frameSize, encodeBuf, 0, encodeBuf.Length);
                                    ms.WriteByte((byte)(encoded & 0xFF));
                                    ms.WriteByte((byte)((encoded >> 8) & 0xFF));
                                    ms.Write(encodeBuf, 0, encoded);
                                    offset += frameSize;
                                }
                                encodedGlobalData = ms.ToArray();
                            }
                        }
                        catch
                        {
                            encodedGlobalData = pcmData; // fallback
                        }
                    });
                    // Dọn state local
                    localRecordingClip = null;
                    localRecordedSamples = 0;

                    // Nếu trước đó realtime voice chat đang bật, bật lại sau khi xử lý xong
                    if (shouldResumeRealtimeAfter && VoiceChatRecorder.instance != null)
                    {
                        VoiceChatRecorder.instance.StartVoiceChat();
                        shouldResumeRealtimeAfter = false;
                    }

                    GameCanvas.currentDialog = null;
                    clearTab();
                }
                else
                {
                    Debug.LogWarning("Không có dữ liệu ghi âm để xử lý!");
                    GameCanvas.currentDialog = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi xử lý voice data: {e.Message}");
                GameCanvas.currentDialog = null;
            }
        }
    }
}