//using System;
//using UnityEngine;

//namespace Assets.Scripts.VoiceChat
//{
//    /// <summary>
//    /// Voice Chat Optimizer - Tối ưu hóa toàn bộ hệ thống voice chat
//    /// </summary>
//    public class VoiceChatOptimizer : MonoBehaviour
//    {
//        [Header("Optimization Settings")]
//        [SerializeField] private bool autoOptimize = true;
//        [SerializeField] private float optimizationInterval = 5.0f;
        
//        [Header("Performance Targets")]
//        [SerializeField] private float targetLatency = 100f; // ms
//        [SerializeField] private float maxJitter = 50f; // ms
//        [SerializeField] private float minBufferFill = 0.3f; // 30%
        
//        private VoiceChatRecorder recorder;
//        private VoiceChatManager manager;
//        private float lastOptimizationTime = 0f;
        
//        // Performance tracking
//        private float[] latencyHistory = new float[10];
//        private int latencyIndex = 0;
//        private float averageLatency = 0f;
        
//        private float[] jitterHistory = new float[10];
//        private int jitterIndex = 0;
//        private float averageJitter = 0f;
        
//        void Start()
//        {
//            recorder = VoiceChatRecorder.gI();
//            manager = VoiceChatManager.gI();
            
//            if (autoOptimize)
//            {
//                InvokeRepeating(nameof(OptimizeSystem), 1f, optimizationInterval);
//            }
            
//            Debug.Log("VoiceChatOptimizer initialized");
//        }
        
//        void Update()
//        {
//            if (autoOptimize && Time.time - lastOptimizationTime > optimizationInterval)
//            {
//                OptimizeSystem();
//                lastOptimizationTime = Time.time;
//            }
//        }
        
//        /// <summary>
//        /// Tối ưu hóa toàn bộ hệ thống voice chat
//        /// </summary>
//        public void OptimizeSystem()
//        {
//            try
//            {
//                // 1. Tối ưu hóa recording
//                OptimizeRecording();
                
//                // 2. Tối ưu hóa playback
//                OptimizePlayback();
                
//                // 3. Tối ưu hóa network
//                OptimizeNetwork();
                
//                // 4. Tối ưu hóa memory
//                OptimizeMemory();
                
//                Debug.Log("Voice chat system optimized");
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"Error during optimization: {e.Message}");
//            }
//        }
        
//        /// <summary>
//        /// Tối ưu hóa phần thu âm
//        /// </summary>
//        private void OptimizeRecording()
//        {
//            if (recorder == null) return;
            
//            // Điều chỉnh send interval dựa trên performance
//            float currentLatency = recorder.GetAverageLatency();
//            UpdateLatencyHistory(currentLatency);
            
//            if (averageLatency > targetLatency)
//            {
//                // Giảm send interval để giảm latency
//                float newInterval = Mathf.Max(0.02f, recorder.SendInterval * 0.9f);
//                recorder.SendInterval = newInterval;
//                Debug.Log($"Reduced send interval to {newInterval * 1000}ms for lower latency");
//            }
//            else if (averageLatency < targetLatency * 0.5f)
//            {
//                // Tăng send interval để tiết kiệm bandwidth
//                float newInterval = Mathf.Min(0.1f, recorder.SendInterval * 1.1f);
//                recorder.SendInterval = newInterval;
//                Debug.Log($"Increased send interval to {newInterval * 1000}ms for bandwidth efficiency");
//            }
            
//            // Điều chỉnh VAD threshold
//            float currentVolume = recorder.GetCurrentVolume();
//            if (currentVolume > 0.1f)
//            {
//                // Tăng threshold nếu có nhiều noise
//                recorder.VADThreshold = Mathf.Min(0.05f, recorder.VADThreshold * 1.1f);
//            }
//            else if (currentVolume < 0.01f)
//            {
//                // Giảm threshold nếu quá yên lặng
//                recorder.VADThreshold = Mathf.Max(0.005f, recorder.VADThreshold * 0.9f);
//            }
//        }
        
//        /// <summary>
//        /// Tối ưu hóa phần phát âm thanh
//        /// </summary>
//        private void OptimizePlayback()
//        {
//            if (manager == null) return;
            
//            int bufferSize = manager.GetBufferSize();
//            float bufferFillRatio = (float)bufferSize / 10f; // Assuming max 10 chunks
            
//            // Điều chỉnh playback mode
//            if (bufferFillRatio > minBufferFill)
//            {
//                // Buffer đủ, sử dụng continuous playback
//                manager.EnableContinuousPlayback();
//            }
//            else
//            {
//                // Buffer thấp, sử dụng adaptive playback
//                manager.EnableAdaptivePlayback();
//            }
            
//            // Điều chỉnh play interval
//            if (bufferFillRatio > 0.8f)
//            {
//                // Buffer đầy, phát nhanh hơn
//                manager.SetPlayInterval(0.02f);
//            }
//            else if (bufferFillRatio < 0.2f)
//            {
//                // Buffer thấp, phát chậm hơn
//                manager.SetPlayInterval(0.05f);
//            }
//            else
//            {
//                // Buffer bình thường
//                manager.SetPlayInterval(0.025f);
//            }
//        }
        
//        /// <summary>
//        /// Tối ưu hóa network
//        /// </summary>
//        private void OptimizeNetwork()
//        {
//            // Tính toán jitter từ latency history
//            CalculateJitter();
            
//            if (averageJitter > maxJitter)
//            {
//                // Jitter cao, tăng buffer size
//                if (recorder != null)
//                {
//                    int newBufferSize = Mathf.Min(8192, recorder.BufferSize * 2);
//                    recorder.BufferSize = newBufferSize;
//                    Debug.Log($"Increased buffer size to {newBufferSize} due to high jitter");
//                }
//            }
//            else if (averageJitter < maxJitter * 0.5f)
//            {
//                // Jitter thấp, có thể giảm buffer size
//                if (recorder != null)
//                {
//                    int newBufferSize = Mathf.Max(2048, recorder.BufferSize / 2);
//                    recorder.BufferSize = newBufferSize;
//                    Debug.Log($"Decreased buffer size to {newBufferSize} due to low jitter");
//                }
//            }
//        }
        
//        /// <summary>
//        /// Tối ưu hóa memory
//        /// </summary>
//        private void OptimizeMemory()
//        {
//            // Clear old buffers nếu cần
//            if (manager != null)
//            {
//                int bufferSize = manager.GetBufferSize();
//                if (bufferSize > 20) // Quá nhiều chunks
//                {
//                    manager.ClearSoundBuffer();
//                    Debug.Log("Cleared sound buffer due to memory pressure");
//                }
//            }
            
//            // Force garbage collection nếu cần
//            if (Time.frameCount % 300 == 0) // Mỗi 300 frames
//            {
//                System.GC.Collect();
//                Debug.Log("Forced garbage collection");
//            }
//        }
        
//        /// <summary>
//        /// Cập nhật lịch sử latency
//        /// </summary>
//        private void UpdateLatencyHistory(float latency)
//        {
//            latencyHistory[latencyIndex] = latency;
//            latencyIndex = (latencyIndex + 1) % latencyHistory.Length;
            
//            // Tính average
//            float sum = 0f;
//            for (int i = 0; i < latencyHistory.Length; i++)
//            {
//                sum += latencyHistory[i];
//            }
//            averageLatency = sum / latencyHistory.Length;
//        }
        
//        /// <summary>
//        /// Tính toán jitter từ latency history
//        /// </summary>
//        private void CalculateJitter()
//        {
//            if (latencyIndex < 2) return;
            
//            float jitter = 0f;
//            int count = 0;
            
//            for (int i = 1; i < latencyHistory.Length; i++)
//            {
//                float diff = Mathf.Abs(latencyHistory[i] - latencyHistory[i - 1]);
//                jitter += diff;
//                count++;
//            }
            
//            if (count > 0)
//            {
//                jitterHistory[jitterIndex] = jitter / count;
//                jitterIndex = (jitterIndex + 1) % jitterHistory.Length;
                
//                // Tính average jitter
//                float sum = 0f;
//                for (int i = 0; i < jitterHistory.Length; i++)
//                {
//                    sum += jitterHistory[i];
//                }
//                averageJitter = sum / jitterHistory.Length;
//            }
//        }
        
//        /// <summary>
//        /// Lấy thông tin performance
//        /// </summary>
//        public string GetPerformanceInfo()
//        {
//            string info = $"Voice Chat Performance:\n";
//            info += $"Average Latency: {averageLatency:F1}ms\n";
//            info += $"Average Jitter: {averageJitter:F1}ms\n";
            
//            if (recorder != null)
//            {
//                info += $"Send Interval: {recorder.SendInterval * 1000:F1}ms\n";
//                info += $"Buffer Size: {recorder.BufferSize}\n";
//                info += $"VAD Threshold: {recorder.VADThreshold:F3}\n";
//                info += $"Current Volume: {recorder.GetCurrentVolume():F3}\n";
//            }
            
//            if (manager != null)
//            {
//                info += $"Buffer Fill: {manager.GetBufferSize()}/10 chunks\n";
//                info += $"Playback Stats: {manager.GetPlaybackStats()}\n";
//            }
            
//            return info;
//        }
        
//        /// <summary>
//        /// Reset tất cả optimization
//        /// </summary>
//        public void ResetOptimization()
//        {
//            if (recorder != null)
//            {
//                recorder.SendInterval = 0.05f;
//                recorder.BufferSize = 4096;
//                recorder.VADThreshold = 0.015f;
//            }
            
//            if (manager != null)
//            {
//                manager.SetPlayInterval(0.025f);
//                manager.SetMinBufferChunks(2);
//                manager.EnableAdaptivePlayback();
//            }
            
//            // Reset history
//            Array.Clear(latencyHistory, 0, latencyHistory.Length);
//            Array.Clear(jitterHistory, 0, jitterHistory.Length);
//            latencyIndex = 0;
//            jitterIndex = 0;
//            averageLatency = 0f;
//            averageJitter = 0f;
            
//            Debug.Log("Voice chat optimization reset to defaults");
//        }
        
//        /// <summary>
//        /// Force optimization ngay lập tức
//        /// </summary>
//        public void ForceOptimization()
//        {
//            OptimizeSystem();
//        }
        
//        /// <summary>
//        /// Bật/tắt auto optimization
//        /// </summary>
//        public void SetAutoOptimize(bool enabled)
//        {
//            autoOptimize = enabled;
//            if (enabled)
//            {
//                InvokeRepeating(nameof(OptimizeSystem), 1f, optimizationInterval);
//            }
//            else
//            {
//                CancelInvoke(nameof(OptimizeSystem));
//            }
            
//            Debug.Log($"Auto optimization {(enabled ? "enabled" : "disabled")}");
//        }
//    }
//} 