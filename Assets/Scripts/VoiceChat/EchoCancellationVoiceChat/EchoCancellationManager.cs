using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Assets.Scripts.VoiceChat.EchoCancellationVoiceChat
{
    /// <summary>
    /// Manager quản lý hệ thống Echo Cancellation cho VoiceChat
    /// Tích hợp với VoiceChatRecorder và VoiceChatManager hiện tại
    /// </summary>
    public class EchoCancellationManager : MonoBehaviour
    {
        [Header("Echo Cancellation Configuration")]
        [SerializeField] private bool enableEchoCancellation = true;
        [SerializeField] private int filterLength = 1024;
        [SerializeField] private float learningRate = 0.1f;
        [SerializeField] private float echoThreshold = 0.3f;
        [SerializeField] private bool enableAdaptiveLearning = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int frameSize = 512;
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private bool enableRealTimeMonitoring = true;
        
        // Core components
        private EchoCancellationCore echoCanceller;
        private AudioBufferManager audioBufferManager;
        private EchoDetectionEngine echoDetector;
        
        // Audio buffers
        private ConcurrentQueue<float[]> microphoneBufferQueue;
        private ConcurrentQueue<float[]> speakerBufferQueue;
        private float[] currentMicrophoneFrame;
        private float[] currentSpeakerFrame;
        
        // State management
        private bool isInitialized = false;
        private bool isProcessing = false;
        private float lastUpdateTime = 0f;
        private int totalFramesProcessed = 0;
        
        // Performance monitoring
        private EchoCancellationMetrics currentMetrics;
        private float averageProcessingTime = 0f;
        private int droppedFrames = 0;
        
        // Events
        public static Action<EchoCancellationMetrics> OnMetricsUpdated;
        public static Action<bool> OnEchoDetected;
        public static Action<float> OnEchoSuppressionChanged;
        
        // Singleton
        public static EchoCancellationManager instance;
        
        public static EchoCancellationManager gI()
        {
            return instance ??= FindObjectOfType<EchoCancellationManager>();
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            InitializeEchoCancellation();
        }
        
        void Update()
        {
            if (!isInitialized || !enableEchoCancellation) return;
            
            // Cập nhật theo interval
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                ProcessAudioFrames();
                UpdatePerformanceMetrics();
                lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Khởi tạo hệ thống Echo Cancellation
        /// </summary>
        private void InitializeEchoCancellation()
        {
            try
            {
                // Khởi tạo core
                echoCanceller = new EchoCancellationCore(filterLength, learningRate);
                
                // Khởi tạo audio buffer manager
                audioBufferManager = new AudioBufferManager(frameSize);
                
                // Khởi tạo echo detector
                echoDetector = new EchoDetectionEngine(echoThreshold);
                
                // Khởi tạo buffer queues
                microphoneBufferQueue = new ConcurrentQueue<float[]>();
                speakerBufferQueue = new ConcurrentQueue<float[]>();
                
                // Khởi tạo frame buffers
                currentMicrophoneFrame = new float[frameSize];
                currentSpeakerFrame = new float[frameSize];
                
                // Hook vào VoiceChat events
                HookIntoVoiceChat();
                
                isInitialized = true;
                
                Debug.Log($"EchoCancellationManager initialized: filterLength={filterLength}, frameSize={frameSize}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize EchoCancellationManager: {e.Message}");
                isInitialized = false;
            }
        }
        
        /// <summary>
        /// Hook vào hệ thống VoiceChat hiện tại
        /// </summary>
        private void HookIntoVoiceChat()
        {
            try
            {
                // Hook vào VoiceChatRecorder để lấy microphone input
                if (VoiceChatRecorder.instance != null)
                {
                    // Override OnVoiceDataSent để xử lý echo cancellation
                    var originalOnVoiceDataSent = VoiceChatRecorder.instance.OnVoiceDataSent;
                    VoiceChatRecorder.instance.OnVoiceDataSent = (data) =>
                    {
                        // Xử lý echo cancellation trước khi gửi
                        ProcessMicrophoneInput(data);
                        
                        // Gọi callback gốc
                        originalOnVoiceDataSent?.Invoke(data);
                    };
                }
                
                // Hook vào VoiceChatManager để lấy speaker output
                if (VoiceChatManager.instance != null)
                {
                    // Monitor khi có audio được phát
                    VoiceChatManager.OnRemoteVolumeChanged += (volume) =>
                    {
                        // Cập nhật speaker activity
                        UpdateSpeakerActivity(volume);
                    };
                }
                
                Debug.Log("EchoCancellationManager hooked into VoiceChat system");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to hook into VoiceChat: {e.Message}");
            }
        }
        
        /// <summary>
        /// Xử lý microphone input với echo cancellation
        /// </summary>
        public void ProcessMicrophoneInput(byte[] rawAudioData)
        {
            if (!isInitialized || !enableEchoCancellation) return;
            
            try
            {
                // Convert byte[] to float[]
                float[] audioFrame = ConvertBytesToFloat(rawAudioData);
                
                // Thêm vào microphone buffer
                microphoneBufferQueue.Enqueue(audioFrame);
                
                // Xử lý nếu có đủ data
                ProcessAudioFrames();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing microphone input: {e.Message}");
            }
        }
        
        /// <summary>
        /// Cập nhật speaker activity
        /// </summary>
        public void UpdateSpeakerActivity(float volumeLevel)
        {
            if (!isInitialized || !enableEchoCancellation) return;
            
            try
            {
                // Tạo dummy speaker frame dựa trên volume level
                float[] speakerFrame = new float[frameSize];
                for (int i = 0; i < frameSize; i++)
                {
                    speakerFrame[i] = volumeLevel * UnityEngine.Random.Range(-0.1f, 0.1f);
                }
                
                // Thêm vào speaker buffer
                speakerBufferQueue.Enqueue(speakerFrame);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating speaker activity: {e.Message}");
            }
        }
        
        /// <summary>
        /// Xử lý audio frames với echo cancellation
        /// </summary>
        private void ProcessAudioFrames()
        {
            if (!isInitialized || !enableEchoCancellation) return;
            
            try
            {
                isProcessing = true;
                float startTime = Time.realtimeSinceStartup;
                
                // Lấy microphone frame
                if (!microphoneBufferQueue.TryDequeue(out float[] micFrame))
                {
                    isProcessing = false;
                    return;
                }
                
                // Lấy speaker frame
                if (!speakerBufferQueue.TryDequeue(out float[] spkFrame))
                {
                    // Nếu không có speaker frame, sử dụng silence
                    spkFrame = new float[frameSize];
                }
                
                // Đảm bảo frame size đúng
                if (micFrame.Length != frameSize || spkFrame.Length != frameSize)
                {
                    droppedFrames++;
                    isProcessing = false;
                    return;
                }
                
                // Xử lý echo cancellation
                float[] processedFrame;
                bool success = echoCanceller.ProcessFrame(micFrame, spkFrame, out processedFrame);
                
                if (success && processedFrame != null)
                {
                    // Cập nhật metrics
                    currentMetrics = echoCanceller.GetMetrics();
                    
                    // Gửi processed frame đi
                    SendProcessedAudio(processedFrame);
                    
                    totalFramesProcessed++;
                }
                else
                {
                    droppedFrames++;
                }
                
                // Tính processing time
                float processingTime = Time.realtimeSinceStartup - startTime;
                averageProcessingTime = Mathf.Lerp(averageProcessingTime, processingTime, 0.1f);
                
                isProcessing = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing audio frames: {e.Message}");
                isProcessing = false;
            }
        }
        
        /// <summary>
        /// Gửi audio đã được xử lý echo cancellation
        /// </summary>
        private void SendProcessedAudio(float[] processedFrame)
        {
            try
            {
                // Convert float[] back to byte[]
                byte[] audioData = ConvertFloatToBytes(processedFrame);
                
                // Gửi qua Service
                if (Service.gI() != null)
                {
                    Service.gI().sendVoiceChat(audioData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending processed audio: {e.Message}");
            }
        }
        
        /// <summary>
        /// Cập nhật performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            if (!enableRealTimeMonitoring) return;
            
            try
            {
                // Cập nhật metrics từ core
                currentMetrics = echoCanceller.GetMetrics();
                
                // Thông báo metrics đã cập nhật
                OnMetricsUpdated?.Invoke(currentMetrics);
                
                // Thông báo echo detection
                if (currentMetrics.EchoDetected)
                {
                    OnEchoDetected?.Invoke(true);
                }
                
                // Thông báo echo suppression
                OnEchoSuppressionChanged?.Invoke(currentMetrics.EchoSuppression);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating performance metrics: {e.Message}");
            }
        }
        
        /// <summary>
        /// Convert byte[] to float[]
        /// </summary>
        private float[] ConvertBytesToFloat(byte[] audioData)
        {
            if (audioData == null || audioData.Length == 0) return new float[frameSize];
            
            int samples = audioData.Length / 2;
            float[] floatData = new float[Mathf.Max(samples, frameSize)];
            
            for (int i = 0; i < samples; i++)
            {
                int byteIndex = i * 2;
                short sample = (short)((audioData[byteIndex + 1] << 8) | audioData[byteIndex]);
                floatData[i] = sample / 32768f;
            }
            
            // Pad với zeros nếu cần
            for (int i = samples; i < frameSize; i++)
            {
                floatData[i] = 0f;
            }
            
            return floatData;
        }
        
        /// <summary>
        /// Convert float[] to byte[]
        /// </summary>
        private byte[] ConvertFloatToBytes(float[] floatData)
        {
            if (floatData == null || floatData.Length == 0) return new byte[0];
            
            byte[] audioData = new byte[floatData.Length * 2];
            
            for (int i = 0; i < floatData.Length; i++)
            {
                short sample = (short)(Mathf.Clamp(floatData[i], -1f, 1f) * 32767f);
                int byteIndex = i * 2;
                audioData[byteIndex] = (byte)(sample & 0xFF);
                audioData[byteIndex + 1] = (byte)((sample >> 8) & 0xFF);
            }
            
            return audioData;
        }
        
        /// <summary>
        /// Bật/tắt echo cancellation
        /// </summary>
        public void SetEchoCancellationEnabled(bool enabled)
        {
            enableEchoCancellation = enabled;
            
            if (enabled && !isInitialized)
            {
                InitializeEchoCancellation();
            }
            
            Debug.Log($"Echo cancellation: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Cập nhật filter length
        /// </summary>
        public void SetFilterLength(int newLength)
        {
            filterLength = Mathf.Clamp(newLength, 256, 4096);
            
            if (isInitialized && echoCanceller != null)
            {
                // Reinitialize với filter length mới
                echoCanceller.Dispose();
                echoCanceller = new EchoCancellationCore(filterLength, learningRate);
            }
            
            Debug.Log($"Filter length updated: {filterLength}");
        }
        
        /// <summary>
        /// Cập nhật learning rate
        /// </summary>
        public void SetLearningRate(float newRate)
        {
            learningRate = Mathf.Clamp(newRate, 0.01f, 1.0f);
            
            if (isInitialized && echoCanceller != null)
            {
                echoCanceller.SetLearningRate(learningRate);
            }
            
            Debug.Log($"Learning rate updated: {learningRate}");
        }
        
        /// <summary>
        /// Reset hệ thống echo cancellation
        /// </summary>
        public void ResetSystem()
        {
            if (isInitialized && echoCanceller != null)
            {
                echoCanceller.ResetFilter();
            }
            
            // Reset performance metrics
            totalFramesProcessed = 0;
            droppedFrames = 0;
            averageProcessingTime = 0f;
            
            Debug.Log("Echo cancellation system reset");
        }
        
        /// <summary>
        /// Lấy performance metrics
        /// </summary>
        public EchoCancellationMetrics GetMetrics()
        {
            return currentMetrics;
        }
        
        /// <summary>
        /// Lấy system status
        /// </summary>
        public EchoCancellationSystemStatus GetSystemStatus()
        {
            return new EchoCancellationSystemStatus
            {
                IsInitialized = isInitialized,
                IsEnabled = enableEchoCancellation,
                IsProcessing = isProcessing,
                TotalFramesProcessed = totalFramesProcessed,
                DroppedFrames = droppedFrames,
                AverageProcessingTime = averageProcessingTime,
                MicrophoneBufferSize = microphoneBufferQueue.Count,
                SpeakerBufferSize = speakerBufferQueue.Count
            };
        }
        
        void OnDestroy()
        {
            try
            {
                if (echoCanceller != null)
                {
                    echoCanceller.Dispose();
                }
                
                Debug.Log("EchoCancellationManager destroyed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error destroying EchoCancellationManager: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// System status của Echo Cancellation
    /// </summary>
    [System.Serializable]
    public struct EchoCancellationSystemStatus
    {
        public bool IsInitialized;
        public bool IsEnabled;
        public bool IsProcessing;
        public int TotalFramesProcessed;
        public int DroppedFrames;
        public float AverageProcessingTime;
        public int MicrophoneBufferSize;
        public int SpeakerBufferSize;
    }
}
