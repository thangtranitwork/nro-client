using Assets.Scripts.VoiceChat;
using Concentus.Enums;
using Concentus.Structs;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class VoiceChatRecorder : MonoBehaviour
{
    [Header("Realtime Voice Chat Settings")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private bool enableVoiceChat = false;

    [Header("Performance Settings - OPTIMIZED")]
    [SerializeField] private float maxSegmentSeconds = 0.35f; // Tối đa tích lũy 0.5s cho mỗi gói
    [SerializeField] private int bufferSize = 8192; // Buffer size
    [SerializeField] private bool enableVAD = true;
    [SerializeField] private float vadThreshold = 0.01f; // VAD threshold
    [SerializeField] private float vadHangoverTime = 1.0f; // VAD hangover time

    [Header("Audio Processing")]
    [SerializeField] private bool enableCompression = false;
    [SerializeField] private float volumeMultiplier = 1.0f; // Volume multiplier
    [SerializeField] private bool enableAGC = true; // Automatic Gain Control
    [SerializeField] private float targetRMS = 0.20f; // Target RMS for AGC
    [SerializeField] private float agcSmoothing = 0.2f; // 0..1, mức làm mượt thay đổi gain
    [SerializeField] private float agcMinGain = 0.5f; // hạn chế giảm quá nhiều
    [SerializeField] private float agcMaxGain = 2.5f; // hạn chế tăng quá nhiều

    [Header("Noise Reduction Settings")]
    [SerializeField] private bool enableNoiseReduction = false;
    [SerializeField] private bool enableNoiseGate = true;
    [SerializeField] private bool enableHighPassFilter = true; // High pass filter
    [SerializeField] private bool enableLowPassFilter = true; // Low pass filter
    [SerializeField] private float noiseGateThreshold = 0.0075f; // Noise gate threshold
    [SerializeField] private float highPassCutoff = 60f; // High pass cutoff
    [SerializeField] private float lowPassCutoff = 8000f; // Low pass cutoff

    // Events
    public System.Action<byte[]> OnVoiceDataSent;
    public System.Action<float> OnVolumeChanged;
    public System.Action<bool> OnVoiceActivityDetected;

    // Core components
    private AudioClip recordingClip;
    private string microphoneDevice;
    private bool isInitialized = false;
    private Coroutine voiceChatCoroutine;
    private int lastSamplePosition = 0;

    // OPTIMIZED: Simple buffer system
    private float[] audioBuffer;
    private bool isVoiceActive = false;
    private float lastVoiceTime = 0f;
    private bool isCurrentlySpeaking = false;

    // Segment-based sending state
    private System.Collections.Generic.List<float> segmentBuffer = new System.Collections.Generic.List<float>(8192);
    private bool isCollectingSegment = false;

    // OPUS codec for realtime bandwidth optimization
    private OpusEncoder opusEncoder;
    private int opusFrameMs = 20; // 20ms frames recommended
    private int opusFrameSize;    // samples per channel per frame
    private byte[] opusEncodeBuffer = new byte[4000]; // generous buffer per frame
    // Carryover between chunks to not drop partial frames
    private short[] opusCarryBuffer = new short[0];

    // Pre-roll buffer to prepend a short context before speech start (for seamless attack)
    private int preRollMs = 30; // milliseconds
    private int preRollSamples = 0;
    private float[] preRollBuffer;
    private int preRollWriteIndex = 0;

    // Edge fade to avoid clicks at chunk boundaries
    private int edgeFadeMs = 3; // milliseconds

    // Compression
    private ADPCMCompressor compressor;
    private float agcCurrentGain = 1.0f;

    // Background Opus encode queue to avoid blocking UI
    private readonly ConcurrentQueue<(byte[] data, int samples)> pendingEncodedQueue = new ConcurrentQueue<(byte[] data, int samples)>();
    private System.Threading.CancellationTokenSource encodeCts;
    private readonly object opusEncodeLock = new object();

    // Noise Reduction Filters
    private AdaptiveNoiseGate noiseGate;
    private HighPassFilter highPassFilter;
    private LowPassFilter lowPassFilter;

    // Performance monitoring
    private float totalAudioSent = 0f;
    private int totalChunksSent = 0;

    // Temp read buffer to avoid over-reading
    private float[] tempReadBuffer;

    public static VoiceChatRecorder instance;

    public static VoiceChatRecorder gI()
    {
        return instance ??= new VoiceChatRecorder();
    }

    // Properties để cập nhật real-time
    public int SampleRate
    {
        get => sampleRate;
        set
        {
            if (sampleRate != value)
            {
                sampleRate = Mathf.Clamp(value, 8000, 22050);
                if (isInitialized)
                {
                    ReinitializeAudio();
                }
            }
        }
    }

    // Removed fixed send interval; now segment-based sending

    public int BufferSize
    {
        get => bufferSize;
        set
        {
            if (bufferSize != value)
            {
                bufferSize = Mathf.Clamp(value, 512, 8192);
                if (isInitialized)
                {
                    ReinitializeAudio();
                }
            }
        }
    }

    public bool EnableVAD
    {
        get => enableVAD;
        set
        {
            if (enableVAD != value)
            {
                enableVAD = value;
                Debug.Log($"VAD enabled: {value}");
            }
        }
    }

    public float VADThreshold
    {
        get => vadThreshold;
        set
        {
            if (vadThreshold != value)
            {
                vadThreshold = Mathf.Clamp(value, 0.001f, 1.0f);
                Debug.Log($"VAD threshold: {vadThreshold}");
            }
        }
    }

    public float VADHangoverTime
    {
        get => vadHangoverTime;
        set
        {
            if (vadHangoverTime != value)
            {
                vadHangoverTime = Mathf.Clamp(value, 0.1f, 2.0f);
                Debug.Log($"VAD hangover time: {vadHangoverTime}");
            }
        }
    }

    public bool EnableCompression
    {
        get => enableCompression;
        set
        {
            if (enableCompression != value)
            {
                enableCompression = value;
                if (value && compressor == null)
                {
                    compressor = new ADPCMCompressor();
                }
                Debug.Log($"Compression enabled: {value}");
            }
        }
    }

    public float VolumeMultiplier
    {
        get => volumeMultiplier;
        set
        {
            if (volumeMultiplier != value)
            {
                volumeMultiplier = Mathf.Clamp(value, 0.0f, 5.0f);
                Debug.Log($"Volume multiplier: {volumeMultiplier}");
            }
        }
    }

    public bool EnableVoiceChat
    {
        get => enableVoiceChat;
        set
        {
            if (enableVoiceChat != value)
            {
                enableVoiceChat = value;
                Debug.Log($"VoiceChat enabled: {value}");
                if (value)
                    StartVoiceChat();
                else
                    StopVoiceChat();
            }
        }
    }

    public bool IsVoiceChatActive => enableVoiceChat && voiceChatCoroutine != null;

    void Start()
    {
        InitializeMicrophone();
        instance = this;

        if (enableVoiceChat)
        {
            StartVoiceChat();
        }
    }

    // Tự động cập nhật khi thay đổi trong Inspector
    void OnValidate()
    {
        // Validate và cập nhật các giá trị
        sampleRate = Mathf.Clamp(sampleRate, 8000, 22050);
        bufferSize = Mathf.Clamp(bufferSize, 512, 8192);
        vadThreshold = Mathf.Clamp(vadThreshold, 0.001f, 1.0f);
        vadHangoverTime = Mathf.Clamp(vadHangoverTime, 0.1f, 2.0f);
        volumeMultiplier = Mathf.Clamp(volumeMultiplier, 0.0f, 5.0f);
        targetRMS = Mathf.Clamp(targetRMS, 0.01f, 0.5f);
        noiseGateThreshold = Mathf.Clamp(noiseGateThreshold, 0.001f, 1.0f);
        highPassCutoff = Mathf.Clamp(highPassCutoff, 20f, 2000f);
        lowPassCutoff = Mathf.Clamp(lowPassCutoff, 1000f, 20000f);

        // Cập nhật real-time nếu đã khởi tạo
        if (isInitialized)
        {
            UpdateAudioSettings();
        }
    }

    void OnDestroy()
    {
        StopVoiceChat();
    }

    public void InitializeMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found!");
            return;
        }

        microphoneDevice = Microphone.devices[0];

        // Validate sample rate
        if (sampleRate > 22050) sampleRate = 22050;
        if (sampleRate < 8000) sampleRate = 8000;

        // No fixed send interval in segment mode

        // Initialize compression
        if (enableCompression)
        {
            compressor = new ADPCMCompressor();
        }

        // Initialize noise reduction filters
        InitializeNoiseReductionFilters();

        // Ensure audio buffers are large enough for realtime processing
        int requiredBufferSize = Mathf.Max(bufferSize, sampleRate / 10);
        audioBuffer = new float[requiredBufferSize];
        tempReadBuffer = new float[requiredBufferSize];

        // No smooth transition state in segment mode

        // Initialize OPUS encoder
        opusFrameSize = Mathf.RoundToInt(sampleRate * (opusFrameMs / 1000f));
        opusEncoder = OpusEncoder.Create(sampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
        opusEncoder.Bitrate = 20000; // ~20 kbps for good voice quality
        opusEncoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;

        // Initialize pre-roll buffer sized to ms
        preRollSamples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * (preRollMs / 1000f)));
        preRollBuffer = new float[preRollSamples];

        Debug.Log($"VoiceChat initialized: sampleRate={sampleRate}, bufferSize={requiredBufferSize}");
        Debug.Log($"Realtime VoiceChat initialized: {microphoneDevice}, {sampleRate}Hz, segment-based sending");

        isInitialized = true;
    }

    public void StartVoiceChat()
    {
        if (!isInitialized)
        {
            InitializeMicrophone();
            if (!isInitialized) return;
        }

        if (voiceChatCoroutine != null)
            StopCoroutine(voiceChatCoroutine);

        // Tối ưu hóa buffer size để tránh chập chờn
        int micBufferLength = CalculateOptimalBufferLength();
        recordingClip = Microphone.Start(microphoneDevice, true, micBufferLength, sampleRate);

        if (recordingClip == null)
        {
            Debug.LogError("Failed to start microphone!");
            return;
        }

        lastSamplePosition = 0;
        totalAudioSent = 0f;
        totalChunksSent = 0;

        // Đảm bảo buffer có kích thước phù hợp
        int requiredBufferSize = Mathf.Max(bufferSize, sampleRate / 10);
        audioBuffer = new float[requiredBufferSize];
        tempReadBuffer = new float[requiredBufferSize];

        // Reset segment state
        segmentBuffer.Clear();
        isCollectingSegment = false;
        opusCarryBuffer = System.Array.Empty<short>();

        StartOpusEncodeWorker();
        voiceChatCoroutine = StartCoroutine(SegmentVoiceLoop());
        Debug.Log($"Realtime VoiceChat started with {micBufferLength}s buffer (segment mode)");
    }

    public void StopVoiceChat()
    {
        if (voiceChatCoroutine != null)
        {
            StopCoroutine(voiceChatCoroutine);
            voiceChatCoroutine = null;
        }

        // stop background worker
        if (encodeCts != null)
        {
            try { encodeCts.Cancel(); } catch {}
            encodeCts = null;
        }

        if (recordingClip != null)
        {
            Microphone.End(microphoneDevice);
            recordingClip = null;
        }

        // Reset voice activity state
        isVoiceActive = false;
        isCurrentlySpeaking = false;
        lastVoiceTime = 0f;
        // no-op

        // Reset segment state
        segmentBuffer.Clear();
        isCollectingSegment = false;

        // Reset noise reduction filters
        ResetNoiseReductionFilters();

        Debug.Log("Voice chat stopped");
    }

    private void ResetNoiseReductionFilters()
    {
        if (enableNoiseReduction)
        {
            if (enableNoiseGate && noiseGate != null)
            {
                noiseGate = new AdaptiveNoiseGate(noiseGateThreshold, sampleRate);
            }

            if (enableHighPassFilter && highPassFilter != null)
            {
                highPassFilter = new HighPassFilter(highPassCutoff, sampleRate);
            }

            if (enableLowPassFilter && lowPassFilter != null)
            {
                lowPassFilter = new LowPassFilter(lowPassCutoff, sampleRate);
            }
        }
    }

    private IEnumerator SegmentVoiceLoop()
    {
        while (enableVoiceChat && recordingClip != null && GameCanvas.gameStarted)
        {
            int currentPosition = Microphone.GetPosition(microphoneDevice);
            if (currentPosition >= 0)
            {
                int availableSamples = GetAvailableSamples(currentPosition);
                if (availableSamples > 0)
                {
                    EnsureAudioBufferCapacity(availableSamples);
                    if (ReadAudioChunk(currentPosition, availableSamples))
                    {
                        ProcessIncomingSamples(availableSamples);
                    }
                }
            }

            // ~10ms polling
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Deprecated fixed-interval sender is removed in segment mode

    // removed legacy per-interval path

    private void ApplyNoiseReduction(int samplesToProcess)
    {
        if (highPassFilter != null) { highPassFilter.ProcessSamples(audioBuffer); }
        if (lowPassFilter != null) { lowPassFilter.ProcessSamples(audioBuffer); }
        if (noiseGate != null) { noiseGate.ProcessSamples(audioBuffer); }
    }

    private bool ReadAudioChunk(int currentPosition, int samplesToRead)
    {
        try
        {
            // Validate input parameters
            if (recordingClip == null || audioBuffer == null)
            {
                Debug.LogError("RecordingClip or AudioBuffer is null");
                return false;
            }

            if (samplesToRead > audioBuffer.Length)
            {
                // grow audio buffer to avoid truncation that may cause distortion
                audioBuffer = new float[Mathf.Max(samplesToRead, bufferSize)];
            }

            int startPosition = lastSamplePosition;

            // Tối ưu hóa: đọc khối liên tục hoặc wrap-around
            if (startPosition + samplesToRead <= recordingClip.samples)
            {
                // Simple case: all samples fit in one read
                recordingClip.GetData(audioBuffer, startPosition);
            }
            else
            {
                // Handle buffer wrap-around với tối ưu hóa
                int samplesBeforeLoop = recordingClip.samples - startPosition;
                int samplesAfterLoop = samplesToRead - samplesBeforeLoop;

                // Ensure we have enough space
                if (audioBuffer.Length < samplesToRead)
                {
                    audioBuffer = new float[Mathf.Max(samplesToRead, bufferSize)];
                }

                float[] tempBuffer1 = new float[samplesBeforeLoop];
                float[] tempBuffer2 = new float[samplesAfterLoop];

                recordingClip.GetData(tempBuffer1, startPosition);
                recordingClip.GetData(tempBuffer2, 0);

                Array.Copy(tempBuffer1, 0, audioBuffer, 0, samplesBeforeLoop);
                Array.Copy(tempBuffer2, 0, audioBuffer, samplesBeforeLoop, samplesAfterLoop);
            }

            // Cập nhật vị trí với wrap-around
            lastSamplePosition = (lastSamplePosition + samplesToRead) % recordingClip.samples;

            // Đảm bảo không có gap trong audio
            if (lastSamplePosition < 0)
            {
                lastSamplePosition = 0;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading audio chunk: {e.Message}");
            return false;
        }
    }

    private int GetAvailableSamples(int currentPosition)
    {
        if (currentPosition >= lastSamplePosition)
            return currentPosition - lastSamplePosition;
        else
            return (recordingClip.samples - lastSamplePosition) + currentPosition;
    }

    private float CalculateVolumeLevel(int samplesToProcess)
    {
        if (audioBuffer == null || samplesToProcess <= 0)
            return 0f;

        // Only calculate volume for the samples we actually read
        int samplesToCalculate = Mathf.Min(samplesToProcess, audioBuffer.Length);
        float sum = 0f;
        for (int i = 0; i < samplesToCalculate; i++)
            sum += audioBuffer[i] * audioBuffer[i];
        return Mathf.Sqrt(sum / samplesToCalculate);
    }

    private void ProcessIncomingSamples(int samplesToProcess)
    {
        if (samplesToProcess <= 0) return;

        // Noise reduction on new samples
        if (enableNoiseReduction)
        {
            ApplyNoiseReduction(samplesToProcess);
        }

        // Simple AGC over the new samples (smoothed to avoid pumping and level drop over time)
        if (enableAGC)
        {
            int count = Mathf.Min(samplesToProcess, audioBuffer.Length);
            float sumSquares = 0f;
            for (int i = 0; i < count; i++) sumSquares += audioBuffer[i] * audioBuffer[i];
            float rms = Mathf.Sqrt(sumSquares / Mathf.Max(1, count));
            if (rms > 0f)
            {
                float desiredGain = Mathf.Clamp(targetRMS / rms, agcMinGain, agcMaxGain);
                // Exponential smoothing for stability
                agcCurrentGain = Mathf.Lerp(agcCurrentGain, desiredGain, Mathf.Clamp01(agcSmoothing));
                for (int i = 0; i < count; i++)
                {
                    float v = audioBuffer[i] * agcCurrentGain;
                    // Soft limiter to reduce clipping buzz
                    float abs = Mathf.Abs(v);
                    if (abs > 0.95f)
                    {
                        float sign = Mathf.Sign(v);
                        // soft-knee compression near 1.0
                        v = sign * (0.95f + (abs - 0.95f) * 0.2f);
                    }
                    audioBuffer[i] = Mathf.Clamp(v, -1f, 1f);
                }
            }
        }

        // DC offset removal over the new samples
        int dcCount = Mathf.Min(samplesToProcess, audioBuffer.Length);
        float dcSum = 0f;
        for (int i = 0; i < dcCount; i++) dcSum += audioBuffer[i];
        float dc = dcSum / Mathf.Max(1, dcCount);
        if (Mathf.Abs(dc) > 1e-4f)
        {
            for (int i = 0; i < dcCount; i++) audioBuffer[i] -= dc;
        }

        // Volume and VAD
        float volumeLevel = CalculateVolumeLevel(samplesToProcess);
        OnVolumeChanged?.Invoke(volumeLevel);
        bool currentVoiceActivity = enableVAD ? (volumeLevel >= vadThreshold) : true;

        if (currentVoiceActivity)
            {
                lastVoiceTime = Time.time;
                isCurrentlySpeaking = true;
            if (!isCollectingSegment)
            {
                // start segment and prepend preroll
                isCollectingSegment = true;
                // reset OPUS carry to avoid frame contamination across segments
                opusCarryBuffer = System.Array.Empty<short>();
                PrependPreRoll();
                OnVoiceActivityDetected?.Invoke(true);
            }
            }
            else
            {
            if (isCurrentlySpeaking && Time.time - lastVoiceTime > vadHangoverTime)
                {
                // end of segment
                    isCurrentlySpeaking = false;
                if (isCollectingSegment)
                {
                    FinalizeAndSendSegment();
                    isCollectingSegment = false;
                    OnVoiceActivityDetected?.Invoke(false);
                }
            }
        }

        // Accumulate to segment if collecting
        if (isCollectingSegment)
        {
            int count = Mathf.Min(samplesToProcess, audioBuffer.Length);
            int maxSamples = Mathf.Max(1, Mathf.RoundToInt(maxSegmentSeconds * sampleRate));
            int remainingCapacity = maxSamples - segmentBuffer.Count;
            int toCopy = Mathf.Min(count, remainingCapacity);
            for (int i = 0; i < toCopy; i++)
            {
                float sample = audioBuffer[i] * volumeMultiplier;
                sample = Mathf.Clamp(sample, -1f, 1f);
                segmentBuffer.Add(sample);
            }
            // Nếu vượt quá giới hạn 1s, kết thúc đoạn để tránh gói quá dài
            if (segmentBuffer.Count >= maxSamples)
            {
                FinalizeAndSendSegment();
                isCollectingSegment = false;
                OnVoiceActivityDetected?.Invoke(false);
            }
        }

        // Update preroll buffer
        UpdatePreRollBuffer(samplesToProcess);
    }

    private void EnsureAudioBufferCapacity(int minCapacity)
    {
        if (audioBuffer == null || audioBuffer.Length < minCapacity)
        {
            audioBuffer = new float[Mathf.Max(minCapacity, bufferSize)];
        }
    }

    private void UpdatePreRollBuffer(int samplesToProcess)
    {
        int count = Mathf.Min(samplesToProcess, audioBuffer.Length);
        for (int i = 0; i < count; i++)
        {
            preRollBuffer[preRollWriteIndex] = audioBuffer[i];
            preRollWriteIndex = (preRollWriteIndex + 1) % preRollSamples;
        }
    }

    private void PrependPreRoll()
    {
        if (preRollSamples <= 0 || preRollBuffer == null) return;
        for (int i = 0; i < preRollSamples; i++)
        {
            int idx = (preRollWriteIndex + i) % preRollSamples;
            segmentBuffer.Add(preRollBuffer[idx]);
        }
    }

    private void FinalizeAndSendSegment()
    {
        if (segmentBuffer == null || segmentBuffer.Count == 0) return;

        int total = segmentBuffer.Count;
        int fadeSamples = Mathf.Clamp(Mathf.RoundToInt(sampleRate * 0.003f), 1, total / 4);
        for (int i = 0; i < fadeSamples; i++)
        {
            float wIn = (float)i / fadeSamples;
            segmentBuffer[i] *= wIn;
        }
        for (int i = 0; i < fadeSamples; i++)
        {
            float wOut = 1f - ((float)i / fadeSamples);
            int idx = total - 1 - i;
            if (idx >= 0) segmentBuffer[idx] *= wOut;
        }

        short[] pcmShort = new short[segmentBuffer.Count + (opusCarryBuffer?.Length ?? 0)];
        int si = 0;
        if (opusCarryBuffer != null && opusCarryBuffer.Length > 0)
        {
            Array.Copy(opusCarryBuffer, 0, pcmShort, 0, opusCarryBuffer.Length);
            si = opusCarryBuffer.Length;
        }
        for (int i = 0; i < segmentBuffer.Count; i++)
        {
            short s = (short)(Mathf.Clamp(segmentBuffer[i], -1f, 1f) * 32767f);
            pcmShort[si++] = s;
        }

        // enqueue for background OPUS encoding to avoid blocking UI
        try
        {
            // convert short[] to byte[]
            byte[] pcmBytes = new byte[pcmShort.Length * 2];
            Buffer.BlockCopy(pcmShort, 0, pcmBytes, 0, pcmBytes.Length);
            pendingEncodedQueue.Enqueue((pcmBytes, pcmShort.Length));
        }
        catch (Exception e)
        {
            Debug.LogError($"Enqueue encode failed: {e.Message}");
        }

        segmentBuffer.Clear();
    }

    private byte[] ConvertToPCM16Bytes(int samplesToConvert)
    {
        // Convert với số samples chính xác để tránh chập chờn
        samplesToConvert = Math.Min(samplesToConvert, audioBuffer.Length);
        byte[] pcmData = new byte[samplesToConvert * 2];

        for (int i = 0; i < samplesToConvert; i++)
        {
            short sample = (short)(Mathf.Clamp(audioBuffer[i], -1f, 1f) * 32767f);
            pcmData[i * 2] = (byte)(sample & 0xFF);
            pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return pcmData;
    }

    public void ToggleVoiceChat()
    {
        EnableVoiceChat = !EnableVoiceChat;
    }

    public static byte[] DecompressVoiceData(byte[] compressedData)
    {
        var compressor = new ADPCMCompressor();
        return compressor.Decompress(compressedData);
    }

    // Performance monitoring
    public float GetAverageLatency()
    {
        return 10f; // ~10ms polling estimate in segment mode
    }

    public float GetTotalAudioSent()
    {
        return totalAudioSent;
    }

    public int GetTotalChunksSent()
    {
        return totalChunksSent;
    }

    public float GetCurrentVolume()
    {
        if (audioBuffer == null || audioBuffer.Length == 0) return 0f;

        // Calculate over a small window
        int samplesToProcess = Mathf.Min(1024, audioBuffer.Length);
        return CalculateVolumeLevel(samplesToProcess);
    }

    // Noise Reduction Control Methods
    public void SetNoiseReductionEnabled(bool enabled)
    {
        enableNoiseReduction = enabled;
        if (enabled && isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetNoiseGateEnabled(bool enabled)
    {
        enableNoiseGate = enabled;
        if (enabled && isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetHighPassFilterEnabled(bool enabled)
    {
        enableHighPassFilter = enabled;
        if (enabled && isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetLowPassFilterEnabled(bool enabled)
    {
        enableLowPassFilter = enabled;
        if (enabled && isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetNoiseGateThreshold(float threshold)
    {
        noiseGateThreshold = Mathf.Clamp(threshold, 0.001f, 1.0f);
        if (isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetHighPassCutoff(float cutoff)
    {
        highPassCutoff = Mathf.Clamp(cutoff, 20f, 2000f);
        if (isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    public void SetLowPassCutoff(float cutoff)
    {
        lowPassCutoff = Mathf.Clamp(cutoff, 1000f, 20000f);
        if (isInitialized)
        {
            UpdateNoiseReductionFilters();
        }
    }

    private void InitializeNoiseReductionFilters()
    {
        if (enableNoiseReduction)
        {
            if (enableNoiseGate)
            {
                noiseGate = new AdaptiveNoiseGate(noiseGateThreshold, sampleRate);
            }

            if (enableHighPassFilter)
            {
                highPassFilter = new HighPassFilter(highPassCutoff, sampleRate);
            }

            if (enableLowPassFilter)
            {
                lowPassFilter = new LowPassFilter(lowPassCutoff, sampleRate);
            }
        }
    }

    private void StartOpusEncodeWorker()
    {
        if (encodeCts != null) return;
        encodeCts = new System.Threading.CancellationTokenSource();
        var token = encodeCts.Token;
        System.Threading.Tasks.Task.Run(() =>
        {
            while (!token.IsCancellationRequested && GameCanvas.gameStarted)
            {
                try
                {
                    if (pendingEncodedQueue.TryDequeue(out var item))
                    {
                        byte[] pcmBytes = item.data;
                        int totalSamples = item.samples;
                        // Encode in 20ms OPUS frames with length prefixing
                        using (var ms = new System.IO.MemoryStream())
                        {
                            int sampleOffset = 0;
                            // convert bytes back to short[] view per frame without allocations
                            while (totalSamples - sampleOffset >= opusFrameSize && GameCanvas.gameStarted)
                            {
                                int frameByteOffset = sampleOffset * 2;
                                int encoded;
                                lock (opusEncodeLock)
                                {
                                    // copy frame to a short[] temp for encoder
                                    short[] frameShorts = new short[opusFrameSize];
                                    Buffer.BlockCopy(pcmBytes, frameByteOffset, frameShorts, 0, opusFrameSize * 2);
                                    encoded = opusEncoder.Encode(frameShorts, 0, opusFrameSize, opusEncodeBuffer, 0, opusEncodeBuffer.Length);
                                }
                                ms.WriteByte((byte)(encoded & 0xFF));
                                ms.WriteByte((byte)((encoded >> 8) & 0xFF));
                                ms.Write(opusEncodeBuffer, 0, encoded);
                                sampleOffset += opusFrameSize;
                            }
                            int remaining = totalSamples - sampleOffset;
                            if (remaining > 0)
                            {
                                // zero pad last frame
                                short[] pad = new short[opusFrameSize];
                                Buffer.BlockCopy(pcmBytes, sampleOffset * 2, pad, 0, remaining * 2);
                                int encoded;
                                lock (opusEncodeLock)
                                {
                                    encoded = opusEncoder.Encode(pad, 0, opusFrameSize, opusEncodeBuffer, 0, opusEncodeBuffer.Length);
                                }
                                ms.WriteByte((byte)(encoded & 0xFF));
                                ms.WriteByte((byte)((encoded >> 8) & 0xFF));
                                ms.Write(opusEncodeBuffer, 0, encoded);
                            }

                            byte[] audioData = ms.ToArray();
                            if (audioData != null && audioData.Length > 0)
                            {
                                // Send back on main thread context is not strictly required for Service.gI; call directly
                                try
                                {
                                    Service.gI().sendVoiceChat(audioData);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error sending voice data: {ex.Message}");
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(2);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Encode worker error: {e.Message}");
                    System.Threading.Thread.Sleep(5);
                }
            }
        }, token);
    }

    // Cập nhật cài đặt audio real-time
    private void UpdateAudioSettings()
    {
        // Cập nhật buffer size
        int requiredBufferSize = Mathf.Max(bufferSize, sampleRate / 10);
        if (audioBuffer == null || audioBuffer.Length != requiredBufferSize)
        {
            audioBuffer = new float[requiredBufferSize];
        }
        if (tempReadBuffer == null || tempReadBuffer.Length != requiredBufferSize)
        {
            tempReadBuffer = new float[requiredBufferSize];
        }

        // Cập nhật compression
        if (enableCompression && compressor == null)
        {
            compressor = new ADPCMCompressor();
        }

        // Cập nhật noise reduction filters
        UpdateNoiseReductionFilters();

        Debug.Log($"Audio settings updated: sampleRate={sampleRate}, bufferSize={requiredBufferSize}");
    }

    // Cập nhật noise reduction filters real-time
    private void UpdateNoiseReductionFilters()
    {
        if (enableNoiseReduction)
        {
            if (enableNoiseGate)
            {
                noiseGate = new AdaptiveNoiseGate(noiseGateThreshold, sampleRate);
            }

            if (enableHighPassFilter)
            {
                highPassFilter = new HighPassFilter(highPassCutoff, sampleRate);
            }

            if (enableLowPassFilter)
            {
                lowPassFilter = new LowPassFilter(lowPassCutoff, sampleRate);
            }
        }
    }

    // Khởi tạo lại audio system
    private void ReinitializeAudio()
    {
        bool wasVoiceChatActive = IsVoiceChatActive;

        if (wasVoiceChatActive)
        {
            StopVoiceChat();
        }

        InitializeMicrophone();

        if (wasVoiceChatActive)
        {
            StartVoiceChat();
        }
    }

    // Tính toán buffer length tối ưu (segment mode)
    private int CalculateOptimalBufferLength()
    {
        int optimalLength = 5; // seconds
        int maxLength = 3600; // 1 hour cap
        optimalLength = Mathf.Clamp(optimalLength, 1, maxLength);
        Debug.Log($"Calculated optimal buffer length: {optimalLength}s (segment mode)");
        return optimalLength;
    }
}
