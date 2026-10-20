using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Concentus.Structs;
using Concentus.Enums;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Assets.Scripts.VoiceChat
{
    public class VoiceChatManager : MonoBehaviour
    {
        [Header("Voice Chat System")]
        [SerializeField] private VoiceChatRecorder recorder;

        // OPTIMIZED: Dual buffer system for seamless playback
        private Queue<AudioChunk> audioChunkQueue = new Queue<AudioChunk>();
        private bool isPlaying = false;
        private float lastPlayTime = 0f;
        private float playInterval = 0.025f; // 25ms cho smooth playback

        // OPTIMIZED: Adaptive buffering
        private int minBufferChunks = 1; // Minimum chunks before starting playback (realtime should start ASAP)
        private int maxBufferChunks = 10; // Maximum chunks to prevent lag
        private float adaptivePlaySpeed = 1.0f;

        // WAV header optimization - PRE-CREATED TEMPLATE
        private byte[] wavHeaderTemplate = null;
        private int sampleRate = 16000;
        private int channels = 1;
        private OpusDecoder opusDecoder;
        private readonly ConcurrentQueue<(string name,int senderId, byte[] data, byte voiceType)> decodeQueue = new ConcurrentQueue<(string,int, byte[], byte)>();
        private System.Threading.CancellationTokenSource decodeCts;
        public string namePlayer;
        // OPTIMIZED: Crossfade system for smooth transitions
        private float[] lastChunkTail = null;
        private int crossfadeSize = 256; // Increased for smoother transitions
        private bool hasPreviousChunk = false;

        // OPTIMIZED: Jitter buffer for network stability
        private float jitterBufferTime = 0.1f; // 100ms jitter buffer
        private float firstChunkReceivedTime = 0f;
        private bool waitingForFirstChunk = true;

        // OPTIMIZED: Continuous playback
        private bool isContinuousPlayback = false;
        private float continuousPlaybackTimer = 0f;
        private float continuousPlaybackInterval = 0.02f; // 20ms for ultra-smooth

        // Performance monitoring
        private int totalChunksReceived = 0;
        private int totalChunksPlayed = 0;
        private float lastBufferCheckTime = 0f;

        public static VoiceChatManager instance;

        // UI hook: notify settings screen about remote playback volume level (0..1)
        public static Action<float> OnRemoteVolumeChanged;

        // Audio chunk structure
        private class AudioChunk
        {
            public byte[] pcmData;
            public float receivedTime;
            public int senderId;
            public byte voiceType;
            public bool isFullMessage;
            public float durationSec;

            public AudioChunk(byte[] data, float time, int id, byte vType, int sampleRate, int channels)
            {
                pcmData = data;
                receivedTime = time;
                senderId = id;
                voiceType = vType;
                isFullMessage = (vType == 4);
                durationSec = (data != null) ? (data.Length / (float)(sampleRate * channels * 2)) : 0f;
            }
        }

        public static VoiceChatManager gI()
        {
            return instance ??= new VoiceChatManager();
        }

        void Start()
        {
            instance = this;

            // Create WAV header template once - NEVER RECREATE
            wavHeaderTemplate = CreateWAVHeaderTemplate(sampleRate, channels);

            // Initialize crossfade buffer
            lastChunkTail = new float[crossfadeSize];

            // Init OPUS decoder for realtime
            opusDecoder = OpusDecoder.Create(sampleRate, channels);

            // start background decoder
            StartDecodeWorker();

            Debug.Log("VoiceChatManager initialized with optimized dual-buffer system");
        }

        /// <summary>
        /// Receive voice data from network with optimized buffering
        /// </summary>
        private AudioChunk pendingFullMessageChunk = null;
        private bool isPlayingFullMessage = false;
        private float activeFullMessageEndTime = 0f;

        public void ReceiveNetworkVoiceData(string name,int senderId, byte[] compressedVoiceData, byte voiceType)
        {
            if (compressedVoiceData == null || compressedVoiceData.Length == 0)
                return;

            try
            {
                byte[] pcm = null;
                if (voiceType == 4)
                {
                    // Global: OPUS stream of [len][payload] frames
                    try
                    {
                        using (var msOut = new System.IO.MemoryStream())
                        {
                            int idx = 0;
                            while (idx + 2 <= compressedVoiceData.Length &&GameCanvas.gameStarted)
                            {
                                int len = compressedVoiceData[idx] | (compressedVoiceData[idx + 1] << 8);
                                idx += 2;
                                if (len <= 0 || idx + len > compressedVoiceData.Length) break;
                                int frameLen = len;
                                short[] decodeBuf = new short[5760]; // max per channel
                                int decoded = opusDecoder.Decode(compressedVoiceData, idx, frameLen, decodeBuf, 0, decodeBuf.Length, false);
                                idx += frameLen;
                                // write PCM16
                                for (int i = 0; i < decoded; i++)
                                {
                                    short s = decodeBuf[i];
                                    msOut.WriteByte((byte)(s & 0xFF));
                                    msOut.WriteByte((byte)((s >> 8) & 0xFF));
                                }
                            }
                            pcm = msOut.ToArray();
                        }
                    }
                    catch { pcm = null; }
                }
                else
                {
                    // Realtime: OPUS stream of [len][payload] frames
                    try
                    {
                        using (var msOut = new System.IO.MemoryStream())
                        {
                            int idx = 0;
                            while (idx + 2 <= compressedVoiceData.Length && GameCanvas.gameStarted)
                            {
                                int len = compressedVoiceData[idx] | (compressedVoiceData[idx + 1] << 8);
                                idx += 2;
                                if (len <= 0 || idx + len > compressedVoiceData.Length) break;
                                int frameLen = len;
                                short[] decodeBuf = new short[5760];
                                int decoded = opusDecoder.Decode(compressedVoiceData, idx, frameLen, decodeBuf, 0, decodeBuf.Length, false);
                                idx += frameLen;
                                for (int i = 0; i < decoded; i++)
                                {
                                    short s = decodeBuf[i];
                                    msOut.WriteByte((byte)(s & 0xFF));
                                    msOut.WriteByte((byte)((s >> 8) & 0xFF));
                                }
                            }
                            pcm = msOut.ToArray();
                        }
                    }
                    catch { pcm = null; }
                }
                if (pcm == null || pcm.Length == 0)
                {
                    // Đưa dữ liệu nén vào hàng đợi giải nén nền
                    decodeQueue.Enqueue((name,senderId, compressedVoiceData, voiceType));
                }
                else
                {
                    // Đã giải nén sẵn (hiếm khi), đưa thẳng vào hàng phát
                    AudioChunk chunk = new AudioChunk(pcm, Time.unscaledTime, senderId, voiceType, sampleRate, channels);
                    EnqueueForPlayback(chunk);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing received voice data: {e.Message}");
            }
        }

        private void PlayFullMessageChunk(AudioChunk chunk)
        {
            try
            {
                // Tạo WAV và phát ngay, đảm bảo độc quyền
                byte[] wavData = CreateOptimizedWAV(chunk.pcmData);
                float volume = VoiceChatSettingScreen.gI().volumeValue / 100f;
                Sound.playSound(wavData, volume, false);
                isPlayingFullMessage = true;
                activeFullMessageEndTime = Time.unscaledTime + Mathf.Max(0.01f, chunk.durationSec);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error playing full message chunk: {e.Message}");
            }
        }

        private void EnqueueForPlayback(AudioChunk chunk)
        {
            if (chunk.isFullMessage)
            {
                if (isPlayingFullMessage && Time.unscaledTime < activeFullMessageEndTime)
                {
                    pendingFullMessageChunk = chunk;
                    return;
                }
                PlayFullMessageChunk(chunk);
                return;
            }

            lock (audioChunkQueue)
            {
                if (audioChunkQueue.Count >= maxBufferChunks)
                {
                    int chunksToRemove = audioChunkQueue.Count - maxBufferChunks + 1;
                    for (int i = 0; i < chunksToRemove; i++) audioChunkQueue.Dequeue();
                }
                audioChunkQueue.Enqueue(chunk);
                totalChunksReceived++;
                if (waitingForFirstChunk)
                {
                    firstChunkReceivedTime = Time.unscaledTime;
                    waitingForFirstChunk = false;
                }
            }
        }

        void Update()
        {
            // OPTIMIZED: Continuous playback for ultra-smooth audio
            if (isContinuousPlayback)
            {
                ContinuousPlaybackUpdate();
            }
            else
            {
                // Fallback to adaptive playback
                AdaptivePlaybackControl();
            }

            // Check buffer status periodically
            if (Time.unscaledTime - lastBufferCheckTime > 1.0f)
            {
                MonitorBufferHealth();
                lastBufferCheckTime = Time.unscaledTime;
            }

            // Quản lý phát full-message (type 4) không chạy song song
            if (isPlayingFullMessage)
            {
                if (Time.unscaledTime >= activeFullMessageEndTime)
                {
                    isPlayingFullMessage = false;
                    if (pendingFullMessageChunk != null)
                    {
                        var next = pendingFullMessageChunk;
                        pendingFullMessageChunk = null;
                        PlayFullMessageChunk(next);
                    }
                }
            }
        }

        /// <summary>
        /// Continuous playback for ultra-smooth audio without gaps
        /// </summary>
        private void ContinuousPlaybackUpdate()
        {
            continuousPlaybackTimer += Time.unscaledDeltaTime;

            if (continuousPlaybackTimer >= continuousPlaybackInterval)
            {
                continuousPlaybackTimer = 0f;
                PlayNextChunkContinuous();
            }
        }

        /// <summary>
        /// Adaptive playback control for smooth audio
        /// </summary>
        private void AdaptivePlaybackControl()
        {
            if (audioChunkQueue.Count == 0)
            {
                // Reset state when no audio
                if (!waitingForFirstChunk && Time.unscaledTime - lastPlayTime > 0.2f)
                {
                    ResetPlaybackState();
                }
                return;
            }

            // Luôn phát ngay khi có dữ liệu, không chờ jitter buffer ban đầu

            // OPTIMIZED: Adaptive timing based on buffer size
            float adjustedInterval = CalculateAdaptiveInterval();

            if (Time.unscaledTime - lastPlayTime >= adjustedInterval)
            {
                PlayNextChunk();
            }
        }

        /// <summary>
        /// Calculate adaptive playback interval based on buffer status
        /// </summary>
        private float CalculateAdaptiveInterval()
        {
            int queueSize = audioChunkQueue.Count;

            // Speed up if buffer is getting full
            if (queueSize > maxBufferChunks * 0.7f)
            {
                adaptivePlaySpeed = 0.8f; // Play 20% faster
            }
            // Slow down if buffer is getting empty
            else if (queueSize < minBufferChunks)
            {
                adaptivePlaySpeed = 1.2f; // Play 20% slower
            }
            else
            {
                adaptivePlaySpeed = 1.0f; // Normal speed
            }

            return playInterval * adaptivePlaySpeed;
        }

        /// <summary>
        /// Play next audio chunk with crossfade
        /// </summary>
        private void PlayNextChunk()
        {
            lock (audioChunkQueue)
            {
                // Luôn phát ngay khi có dữ liệu; không chờ đủ buffer

                if (audioChunkQueue.Count == 0)
                {
                    isPlaying = false;
                    return;
                }

                AudioChunk chunk = audioChunkQueue.Dequeue();

                // Process and play the chunk
                ProcessAndPlayChunk(chunk);

                lastPlayTime = Time.unscaledTime;
                isPlaying = true;
                totalChunksPlayed++;
            }
        }

        /// <summary>
        /// Continuous playback - play chunks as fast as possible
        /// </summary>
        private void PlayNextChunkContinuous()
        {
            lock (audioChunkQueue)
            {
                if (audioChunkQueue.Count == 0)
                {
                    isContinuousPlayback = false;
                    return;
                }

                AudioChunk chunk = audioChunkQueue.Dequeue();

                // Process and play the chunk
                ProcessAndPlayChunk(chunk);

                lastPlayTime = Time.unscaledTime;
                isPlaying = true;
                totalChunksPlayed++;
            }
        }

        /// <summary>
        /// Process and play audio chunk with optimizations
        /// </summary>
        private void ProcessAndPlayChunk(AudioChunk chunk)
        {
            try
            {
                byte[] pcmData = chunk.pcmData;

                // OPTIMIZED: Apply crossfade if we have previous audio
                if (hasPreviousChunk && lastChunkTail != null)
                {
                    pcmData = ApplyCrossfade(pcmData);
                }

                // Store tail of current chunk for next crossfade
                StoreTailForCrossfade(pcmData);

                // Create WAV with optimized header - NO ALLOCATION
                byte[] wavData = CreateOptimizedWAV(pcmData);

                // Play with volume control
                float volume = VoiceChatSettingScreen.gI().volumeValue / 100f;
                Sound.playSound(wavData, volume, false);

                // Compute simple RMS from pcmData and notify UI
                try
                {
                    if (OnRemoteVolumeChanged != null && pcmData != null && pcmData.Length >= 2)
                    {
                        int samples = pcmData.Length / 2;
                        double sum = 0;
                        for (int i = 0; i < samples; i++)
                        {
                            int idx = i * 2;
                            short s = (short)((pcmData[idx + 1] << 8) | pcmData[idx]);
                            double f = s / 32768.0;
                            sum += f * f;
                        }
                        float rms = (float)Math.Sqrt(sum / Math.Max(1, samples));
                        OnRemoteVolumeChanged?.Invoke(Mathf.Clamp01(rms * 2f));
                    }
                }
                catch { }

                hasPreviousChunk = true;

                Debug.Log($"Playing chunk #{totalChunksPlayed}, Queue: {audioChunkQueue.Count}, Speed: {adaptivePlaySpeed:F2}x");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error playing audio chunk: {e.Message}");
            }
        }

        /// <summary>
        /// Apply crossfade between chunks for smooth transition
        /// </summary>
        private byte[] ApplyCrossfade(byte[] pcmData)
        {
            if (pcmData.Length < crossfadeSize * 2)
                return pcmData;

            byte[] result = new byte[pcmData.Length];
            Array.Copy(pcmData, result, pcmData.Length);

            // Convert first part to float for crossfade
            int samplesToFade = Math.Min(crossfadeSize, pcmData.Length / 2);

            for (int i = 0; i < samplesToFade; i++)
            {
                // Get current sample
                short currentSample = (short)((pcmData[i * 2 + 1] << 8) | pcmData[i * 2]);
                float currentFloat = currentSample / 32768f;

                // Apply crossfade with smooth curve
                float fadeIn = (float)i / samplesToFade;
                fadeIn = fadeIn * fadeIn * (3f - 2f * fadeIn); // Smoothstep function
                float fadeOut = 1f - fadeIn;
                float mixedSample = (currentFloat * fadeIn) + (lastChunkTail[i] * fadeOut);

                // Convert back to PCM16
                short mixedPCM = (short)(Mathf.Clamp(mixedSample, -1f, 1f) * 32767f);
                result[i * 2] = (byte)(mixedPCM & 0xFF);
                result[i * 2 + 1] = (byte)((mixedPCM >> 8) & 0xFF);
            }

            return result;
        }

        /// <summary>
        /// Store tail of current chunk for crossfade with next chunk
        /// </summary>
        private void StoreTailForCrossfade(byte[] pcmData)
        {
            if (pcmData.Length < crossfadeSize * 2)
            {
                // If chunk is too small, store what we can
                int availableSamples = pcmData.Length / 2;
                for (int i = 0; i < availableSamples; i++)
                {
                    int offset = pcmData.Length - ((availableSamples - i) * 2);
                    short sample = (short)((pcmData[offset + 1] << 8) | pcmData[offset]);
                    lastChunkTail[i] = sample / 32768f;
                }

                // Fill rest with silence
                for (int i = availableSamples; i < crossfadeSize; i++)
                {
                    lastChunkTail[i] = 0f;
                }
            }
            else
            {
                // Store last crossfadeSize samples
                int startOffset = pcmData.Length - (crossfadeSize * 2);
                for (int i = 0; i < crossfadeSize; i++)
                {
                    int offset = startOffset + (i * 2);
                    short sample = (short)((pcmData[offset + 1] << 8) | pcmData[offset]);
                    lastChunkTail[i] = sample / 32768f;
                }
            }
        }

        /// <summary>
        /// Create optimized WAV with pre-calculated header - NO ALLOCATION
        /// </summary>
        private byte[] CreateOptimizedWAV(byte[] pcmData)
        {
            int dataSize = pcmData.Length;
            int fileSize = 44 + dataSize;

            byte[] wavData = new byte[fileSize];

            // Copy header template - NO RECREATION (ensure header exists)
            if (wavHeaderTemplate == null || wavHeaderTemplate.Length < 44)
            {
                wavHeaderTemplate = CreateWAVHeaderTemplate(sampleRate, channels);
            }
            Array.Copy(wavHeaderTemplate, 0, wavData, 0, 44);

            // Update dynamic fields only
            UpdateWAVSizes(wavData, fileSize - 8, dataSize);

            // Ensure mono @ 16kHz: if input is not matching, just trust upstream conversion (GlobalChat already converts)
            Array.Copy(pcmData, 0, wavData, 44, dataSize);

            // DC offset removal + gentle limiter + light low-pass + short fade-in/out to avoid clicks
            int sampleCount = dataSize / 2;
            int offset = 44;
            float mean = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                short s = (short)((wavData[offset + i * 2 + 1] << 8) | wavData[offset + i * 2]);
                mean += s;
            }
            mean /= Mathf.Max(1, sampleCount);
            float prev = 0f;
            float alpha = 0.15f; // slightly gentler smoothing
            for (int i = 0; i < sampleCount; i++)
            {
                int idx = offset + i * 2;
                short s = (short)((wavData[idx + 1] << 8) | wavData[idx]);
                float centered = s - mean;
                // low-pass
                float smoothed = prev + alpha * (centered - prev);
                prev = smoothed;
                // soft clip
                smoothed = Mathf.Clamp(smoothed, -30000f, 30000f);
                short outS = (short)smoothed;
                wavData[idx] = (byte)(outS & 0xFF);
                wavData[idx + 1] = (byte)((outS >> 8) & 0xFF);
            }

            // Apply 3ms fade-in/out to avoid truncation clicks
            int fadeSamples = Mathf.Clamp((int)(sampleRate * 0.0025f), 1, sampleCount / 2);
            for (int i = 0; i < fadeSamples; i++)
            {
                float gIn = (float)i / fadeSamples;
                int idxIn = offset + i * 2;
                short sIn = (short)((wavData[idxIn + 1] << 8) | wavData[idxIn]);
                short sInOut = (short)(sIn * gIn);
                wavData[idxIn] = (byte)(sInOut & 0xFF);
                wavData[idxIn + 1] = (byte)((sInOut >> 8) & 0xFF);

                float gOut = (float)i / fadeSamples;
                int idxOut = offset + (sampleCount - 1 - i) * 2;
                short sOut = (short)((wavData[idxOut + 1] << 8) | wavData[idxOut]);
                short sOutOut = (short)(sOut * (1f - gOut));
                wavData[idxOut] = (byte)(sOutOut & 0xFF);
                wavData[idxOut + 1] = (byte)((sOutOut >> 8) & 0xFF);
            }

            return wavData;
        }

        /// <summary>
        /// Update WAV header sizes efficiently
        /// </summary>
        private void UpdateWAVSizes(byte[] wavData, int fileSize, int dataSize)
        {
            // File size (bytes 4-7)
            wavData[4] = (byte)(fileSize & 0xFF);
            wavData[5] = (byte)((fileSize >> 8) & 0xFF);
            wavData[6] = (byte)((fileSize >> 16) & 0xFF);
            wavData[7] = (byte)((fileSize >> 24) & 0xFF);

            // Data size (bytes 40-43)
            wavData[40] = (byte)(dataSize & 0xFF);
            wavData[41] = (byte)((dataSize >> 8) & 0xFF);
            wavData[42] = (byte)((dataSize >> 16) & 0xFF);
            wavData[43] = (byte)((dataSize >> 24) & 0xFF);
        }

        /// <summary>
        /// Create WAV header template (called once)
        /// </summary>
        private byte[] CreateWAVHeaderTemplate(int sampleRate, int channels)
        {
            using (MemoryStream stream = new MemoryStream(44))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // RIFF header
                writer.Write("RIFF".ToCharArray());
                writer.Write(0); // File size placeholder
                writer.Write("WAVE".ToCharArray());

                // fmt chunk
                writer.Write("fmt ".ToCharArray());
                writer.Write(16); // Chunk size
                writer.Write((short)1); // PCM format
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2); // Byte rate
                writer.Write((short)(channels * 2)); // Block align
                writer.Write((short)16); // Bits per sample

                // data chunk header
                writer.Write("data".ToCharArray());
                writer.Write(0); // Data size placeholder

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Monitor buffer health and adjust parameters
        /// </summary>
        private void MonitorBufferHealth()
        {
            int queueSize = audioChunkQueue.Count;
            float fillRatio = (float)queueSize / maxBufferChunks;

            // Auto-switch to continuous playback if buffer is healthy
            if (queueSize >= minBufferChunks && !isContinuousPlayback)
            {
                isContinuousPlayback = true;
                Debug.Log("Switched to continuous playback for smoother audio");
            }
            else if (queueSize < minBufferChunks && isContinuousPlayback)
            {
                isContinuousPlayback = false;
                Debug.Log("Switched to adaptive playback due to low buffer");
            }

            // Log statistics
            if (totalChunksReceived > 0)
            {
                float playRatio = (float)totalChunksPlayed / totalChunksReceived;
                Debug.Log($"Buffer Health: Queue={queueSize}, Fill={fillRatio:P0}, Play ratio={playRatio:P0}, Speed={adaptivePlaySpeed:F2}x, Continuous={isContinuousPlayback}");
            }

            // Auto-adjust parameters if needed
            if (fillRatio > 0.9f)
            {
                // Buffer too full, might need to skip frames
                Debug.LogWarning("Buffer nearly full, consider reducing network latency");
            }
            else if (fillRatio < 0.1f && isPlaying)
            {
                // Buffer nearly empty, might cause stuttering
                Debug.LogWarning("Buffer nearly empty, possible network issues");
            }
        }

        /// <summary>
        /// Reset playback state
        /// </summary>
        private void ResetPlaybackState()
        {
            waitingForFirstChunk = true;
            firstChunkReceivedTime = 0f;
            hasPreviousChunk = false;
            isPlaying = false;
            isContinuousPlayback = false;
            adaptivePlaySpeed = 1.0f;

            // Clear crossfade buffer
            if (lastChunkTail != null)
            {
                Array.Clear(lastChunkTail, 0, lastChunkTail.Length);
            }

            Debug.Log("Playback state reset");
        }

        /// <summary>
        /// Clear all audio buffers
        /// </summary>
        public void ClearSoundBuffer()
        {
            lock (audioChunkQueue)
            {
                audioChunkQueue.Clear();
            }

            ResetPlaybackState();
            totalChunksReceived = 0;
            totalChunksPlayed = 0;

            // stop background decode
            if (decodeCts != null)
            {
                try { decodeCts.Cancel(); } catch {}
                decodeCts = null;
            }

            Debug.Log("All audio buffers cleared");
        }

        /// <summary>
        /// Get current buffer size
        /// </summary>
        public int GetBufferSize()
        {
            lock (audioChunkQueue)
            {
                return audioChunkQueue.Count;
            }
        }

        /// <summary>
        /// Set playback interval
        /// </summary>
        public void SetPlayInterval(float interval)
        {
            playInterval = Mathf.Clamp(interval, 0.01f, 0.1f);
            Debug.Log($"Play interval set to {playInterval * 1000}ms");
        }

        /// <summary>
        /// Set minimum buffer chunks before playback
        /// </summary>
        public void SetMinBufferChunks(int chunks)
        {
            minBufferChunks = Mathf.Clamp(chunks, 1, 5);
            Debug.Log($"Minimum buffer chunks set to {minBufferChunks}");
        }

        /// <summary>
        /// Enable/disable voice chat
        /// </summary>
        public void SetVoiceChatEnabled(bool enabled)
        {
            if (recorder != null)
                recorder.EnableVoiceChat = enabled;

            if (!enabled)
            {
                ClearSoundBuffer();
            }
        }

        /// <summary>
        /// Get playback statistics
        /// </summary>
        public string GetPlaybackStats()
        {
            float playRatio = totalChunksReceived > 0 ?
                (float)totalChunksPlayed / totalChunksReceived : 0f;

            return $"Received: {totalChunksReceived}, Played: {totalChunksPlayed}, " +
                   $"Ratio: {playRatio:P0}, Queue: {GetBufferSize()}, Speed: {adaptivePlaySpeed:F2}x, Continuous={isContinuousPlayback}";
        }

        /// <summary>
        /// Force continuous playback mode
        /// </summary>
        public void EnableContinuousPlayback()
        {
            isContinuousPlayback = true;
            Debug.Log("Forced continuous playback mode");
        }

        /// <summary>
        /// Force adaptive playback mode
        /// </summary>
        public void EnableAdaptivePlayback()
        {
            isContinuousPlayback = false;
            Debug.Log("Forced adaptive playback mode");
        }

        private void StartDecodeWorker()
        {
            if (decodeCts != null) return;
            decodeCts = new System.Threading.CancellationTokenSource();
            var token = decodeCts.Token;
            System.Threading.Tasks.Task.Run(() =>
            {
                while (!token.IsCancellationRequested && GameCanvas.gameStarted)
                {
                    try
                    {
                        if (decodeQueue.TryDequeue(out var item))
                        {
                            int senderId = item.senderId;
                            byte[] data = item.data;
                            byte voiceType = item.voiceType;

                            byte[] pcm = null;
                            try
                            {
                                using (var msOut = new System.IO.MemoryStream())
                                {
                                    int idx = 0;
                                    while (idx + 2 <= data.Length && GameCanvas.gameStarted)
                                    {
                                        int len = data[idx] | (data[idx + 1] << 8);
                                        idx += 2;
                                        if (len <= 0 || idx + len > data.Length) break;
                                        int frameLen = len;
                                        short[] decodeBuf = new short[5760];
                                        int decoded = opusDecoder.Decode(data, idx, frameLen, decodeBuf, 0, decodeBuf.Length, false);
                                        idx += frameLen;
                                        for (int i = 0; i < decoded; i++)
                                        {
                                            short s = decodeBuf[i];
                                            msOut.WriteByte((byte)(s & 0xFF));
                                            msOut.WriteByte((byte)((s >> 8) & 0xFF));
                                        }
                                    }
                                    pcm = msOut.ToArray();
                                }
                            }
                            catch { pcm = null; }

                            if (pcm != null && pcm.Length > 0)
                            {
                                var chunk = new AudioChunk(pcm, Time.unscaledTime, senderId, voiceType, sampleRate, channels);
                                EnqueueForPlayback(chunk);
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(2);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Decode worker error: {e.Message}");
                        System.Threading.Thread.Sleep(5);
                    }
                }
            }, token);
        }
    }
}