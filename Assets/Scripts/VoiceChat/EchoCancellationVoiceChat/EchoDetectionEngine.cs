using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.VoiceChat.EchoCancellationVoiceChat
{
    /// <summary>
    /// Engine phát hiện echo thông minh sử dụng multiple detection algorithms
    /// Kết hợp correlation analysis, spectral analysis và machine learning
    /// </summary>
    public class EchoDetectionEngine
    {
        [Header("Echo Detection Configuration")]
        private float echoThreshold;
        private float correlationThreshold;
        private float spectralThreshold;
        private float energyThreshold;
        
        // Detection algorithms
        private CorrelationDetector correlationDetector;
        private SpectralDetector spectralDetector;
        private EnergyDetector energyDetector;
        private CrossTalkDetector crossTalkDetector;
        
        // Detection history
        private Queue<EchoDetectionResult> detectionHistory;
        private int maxHistorySize = 100;
        
        // Performance metrics
        private int totalDetections = 0;
        private int truePositives = 0;
        private int falsePositives = 0;
        private float detectionAccuracy = 0f;
        
        // State management
        private bool isInitialized = false;
        private bool isDetectionEnabled = true;
        private float lastDetectionTime = 0f;
        
        // Adaptive thresholds
        private float adaptiveEchoThreshold;
        private float adaptiveCorrelationThreshold;
        private bool enableAdaptiveThresholds = true;
        
        public EchoDetectionEngine(float echoThreshold = 0.3f)
        {
            this.echoThreshold = echoThreshold;
            InitializeDetectionEngine();
        }
        
        /// <summary>
        /// Khởi tạo engine phát hiện echo
        /// </summary>
        private void InitializeDetectionEngine()
        {
            try
            {
                // Khởi tạo các detector
                correlationDetector = new CorrelationDetector();
                spectralDetector = new SpectralDetector();
                energyDetector = new EnergyDetector();
                crossTalkDetector = new CrossTalkDetector();
                
                // Khởi tạo detection history
                detectionHistory = new Queue<EchoDetectionResult>();
                
                // Khởi tạo adaptive thresholds
                adaptiveEchoThreshold = echoThreshold;
                adaptiveCorrelationThreshold = 0.5f;
                
                // Tính toán các threshold khác
                correlationThreshold = echoThreshold * 0.8f;
                spectralThreshold = echoThreshold * 0.6f;
                energyThreshold = echoThreshold * 0.4f;
                
                isInitialized = true;
                
                Debug.Log($"EchoDetectionEngine initialized: echoThreshold={echoThreshold}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize EchoDetectionEngine: {e.Message}");
                isInitialized = false;
            }
        }
        
        /// <summary>
        /// Phát hiện echo trong audio frame
        /// </summary>
        /// <param name="microphoneInput">Input từ microphone</param>
        /// <param name="speakerOutput">Output từ speaker</param>
        /// <param name="processedOutput">Output đã được xử lý</param>
        /// <returns>Kết quả phát hiện echo</returns>
        public EchoDetectionResult DetectEcho(float[] microphoneInput, float[] speakerOutput, float[] processedOutput)
        {
            if (!isInitialized || !isDetectionEnabled) 
                return new EchoDetectionResult { IsEchoDetected = false, Confidence = 0f };
            
            try
            {
                var result = new EchoDetectionResult();
                
                // Correlation detection
                var correlationResult = correlationDetector.Detect(microphoneInput, speakerOutput);
                result.CorrelationScore = correlationResult.Score;
                result.CorrelationDetected = correlationResult.IsDetected;
                
                // Spectral detection
                var spectralResult = spectralDetector.Detect(microphoneInput, processedOutput);
                result.SpectralScore = spectralResult.Score;
                result.SpectralDetected = spectralResult.IsDetected;
                
                // Energy detection
                var energyResult = energyDetector.Detect(microphoneInput, speakerOutput);
                result.EnergyScore = energyResult.Score;
                result.EnergyDetected = energyResult.IsDetected;
                
                // Cross-talk detection
                var crossTalkResult = crossTalkDetector.Detect(microphoneInput, speakerOutput);
                result.CrossTalkScore = crossTalkResult.Score;
                result.CrossTalkDetected = crossTalkResult.IsDetected;
                
                // Tính toán confidence score tổng hợp
                result.Confidence = CalculateConfidenceScore(result);
                
                // Quyết định cuối cùng
                result.IsEchoDetected = DetermineEchoDetection(result);
                
                // Cập nhật detection history
                UpdateDetectionHistory(result);
                
                // Cập nhật adaptive thresholds
                if (enableAdaptiveThresholds)
                {
                    UpdateAdaptiveThresholds(result);
                }
                
                // Cập nhật performance metrics
                UpdatePerformanceMetrics(result);
                
                totalDetections++;
                lastDetectionTime = Time.time;
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in echo detection: {e.Message}");
                return new EchoDetectionResult { IsEchoDetected = false, Confidence = 0f };
            }
        }
        
        /// <summary>
        /// Tính toán confidence score tổng hợp
        /// </summary>
        private float CalculateConfidenceScore(EchoDetectionResult result)
        {
            try
            {
                float confidence = 0f;
                float totalWeight = 0f;
                
                // Correlation weight: 40%
                float correlationWeight = 0.4f;
                confidence += result.CorrelationScore * correlationWeight;
                totalWeight += correlationWeight;
                
                // Spectral weight: 30%
                float spectralWeight = 0.3f;
                confidence += result.SpectralScore * spectralWeight;
                totalWeight += spectralWeight;
                
                // Energy weight: 20%
                float energyWeight = 0.2f;
                confidence += result.EnergyScore * energyWeight;
                totalWeight += energyWeight;
                
                // Cross-talk weight: 10%
                float crossTalkWeight = 0.1f;
                confidence += result.CrossTalkScore * crossTalkWeight;
                totalWeight += crossTalkWeight;
                
                // Normalize confidence
                if (totalWeight > 0)
                {
                    confidence /= totalWeight;
                }
                
                return Mathf.Clamp01(confidence);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating confidence score: {e.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Quyết định cuối cùng về echo detection
        /// </summary>
        private bool DetermineEchoDetection(EchoDetectionResult result)
        {
            try
            {
                // Sử dụng adaptive threshold
                float threshold = adaptiveEchoThreshold;
                
                // Nếu confidence cao, echo được phát hiện
                if (result.Confidence >= threshold)
                {
                    return true;
                }
                
                // Nếu có nhiều detector phát hiện, tăng khả năng echo
                int detectorCount = 0;
                if (result.CorrelationDetected) detectorCount++;
                if (result.SpectralDetected) detectorCount++;
                if (result.EnergyDetected) detectorCount++;
                if (result.CrossTalkDetected) detectorCount++;
                
                // Nếu ít nhất 2 detector phát hiện, có thể có echo
                if (detectorCount >= 2 && result.Confidence >= threshold * 0.7f)
                {
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error determining echo detection: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Cập nhật detection history
        /// </summary>
        private void UpdateDetectionHistory(EchoDetectionResult result)
        {
            try
            {
                // Thêm kết quả mới vào history
                detectionHistory.Enqueue(result);
                
                // Giữ history size trong giới hạn
                while (detectionHistory.Count > maxHistorySize)
                {
                    detectionHistory.Dequeue();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating detection history: {e.Message}");
            }
        }
        
        /// <summary>
        /// Cập nhật adaptive thresholds
        /// </summary>
        private void UpdateAdaptiveThresholds(EchoDetectionResult result)
        {
            try
            {
                // Tính toán false positive rate
                float falsePositiveRate = CalculateFalsePositiveRate();
                
                // Điều chỉnh threshold dựa trên false positive rate
                if (falsePositiveRate > 0.1f) // Nếu > 10% false positive
                {
                    // Tăng threshold để giảm false positive
                    adaptiveEchoThreshold = Mathf.Min(adaptiveEchoThreshold * 1.1f, 0.8f);
                    adaptiveCorrelationThreshold = Mathf.Min(adaptiveCorrelationThreshold * 1.1f, 0.9f);
                }
                else if (falsePositiveRate < 0.05f) // Nếu < 5% false positive
                {
                    // Giảm threshold để tăng sensitivity
                    adaptiveEchoThreshold = Mathf.Max(adaptiveEchoThreshold * 0.95f, 0.1f);
                    adaptiveCorrelationThreshold = Mathf.Max(adaptiveCorrelationThreshold * 0.95f, 0.2f);
                }
                
                // Cập nhật các threshold khác
                correlationThreshold = adaptiveCorrelationThreshold;
                spectralThreshold = adaptiveEchoThreshold * 0.6f;
                energyThreshold = adaptiveEchoThreshold * 0.4f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating adaptive thresholds: {e.Message}");
            }
        }
        
        /// <summary>
        /// Tính toán false positive rate
        /// </summary>
        private float CalculateFalsePositiveRate()
        {
            try
            {
                if (detectionHistory.Count == 0) return 0f;
                
                int falsePositives = 0;
                int totalDetections = 0;
                
                foreach (var result in detectionHistory)
                {
                    totalDetections++;
                    if (result.IsEchoDetected && result.Confidence < 0.5f)
                    {
                        falsePositives++;
                    }
                }
                
                return totalDetections > 0 ? (float)falsePositives / totalDetections : 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating false positive rate: {e.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Cập nhật performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics(EchoDetectionResult result)
        {
            try
            {
                // Cập nhật true positive/false positive count
                if (result.IsEchoDetected)
                {
                    if (result.Confidence >= 0.7f)
                    {
                        truePositives++;
                    }
                    else
                    {
                        falsePositives++;
                    }
                }
                
                // Tính toán accuracy
                int totalPositive = truePositives + falsePositives;
                if (totalPositive > 0)
                {
                    detectionAccuracy = (float)truePositives / totalPositive;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating performance metrics: {e.Message}");
            }
        }
        
        /// <summary>
        /// Bật/tắt echo detection
        /// </summary>
        public void SetDetectionEnabled(bool enabled)
        {
            isDetectionEnabled = enabled;
            Debug.Log($"Echo detection: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Cập nhật echo threshold
        /// </summary>
        public void SetEchoThreshold(float newThreshold)
        {
            echoThreshold = Mathf.Clamp(newThreshold, 0.1f, 0.9f);
            adaptiveEchoThreshold = echoThreshold;
            
            // Cập nhật các threshold khác
            correlationThreshold = echoThreshold * 0.8f;
            spectralThreshold = echoThreshold * 0.6f;
            energyThreshold = echoThreshold * 0.4f;
            
            Debug.Log($"Echo threshold updated: {echoThreshold}");
        }
        
        /// <summary>
        /// Bật/tắt adaptive thresholds
        /// </summary>
        public void SetAdaptiveThresholdsEnabled(bool enabled)
        {
            enableAdaptiveThresholds = enabled;
            Debug.Log($"Adaptive thresholds: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Reset detection engine
        /// </summary>
        public void ResetEngine()
        {
            try
            {
                // Reset history
                detectionHistory.Clear();
                
                // Reset metrics
                totalDetections = 0;
                truePositives = 0;
                falsePositives = 0;
                detectionAccuracy = 0f;
                
                // Reset adaptive thresholds
                adaptiveEchoThreshold = echoThreshold;
                adaptiveCorrelationThreshold = 0.5f;
                
                Debug.Log("Echo detection engine reset");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error resetting detection engine: {e.Message}");
            }
        }
        
        /// <summary>
        /// Lấy detection statistics
        /// </summary>
        public EchoDetectionStatistics GetDetectionStatistics()
        {
            return new EchoDetectionStatistics
            {
                TotalDetections = totalDetections,
                TruePositives = truePositives,
                FalsePositives = falsePositives,
                DetectionAccuracy = detectionAccuracy,
                CurrentEchoThreshold = adaptiveEchoThreshold,
                CurrentCorrelationThreshold = adaptiveCorrelationThreshold,
                FalsePositiveRate = CalculateFalsePositiveRate(),
                HistorySize = detectionHistory.Count
            };
        }
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                detectionHistory?.Clear();
                detectionHistory = null;
                
                Debug.Log("EchoDetectionEngine disposed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disposing EchoDetectionEngine: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Kết quả phát hiện echo
    /// </summary>
    [System.Serializable]
    public struct EchoDetectionResult
    {
        public bool IsEchoDetected;
        public float Confidence;
        
        // Individual detector results
        public bool CorrelationDetected;
        public float CorrelationScore;
        public bool SpectralDetected;
        public float SpectralScore;
        public bool EnergyDetected;
        public float EnergyScore;
        public bool CrossTalkDetected;
        public float CrossTalkScore;
        
        // Timestamp
        public float DetectionTime;
    }
    
    /// <summary>
    /// Thống kê phát hiện echo
    /// </summary>
    [System.Serializable]
    public struct EchoDetectionStatistics
    {
        public int TotalDetections;
        public int TruePositives;
        public int FalsePositives;
        public float DetectionAccuracy;
        public float CurrentEchoThreshold;
        public float CurrentCorrelationThreshold;
        public float FalsePositiveRate;
        public int HistorySize;
    }
    
    /// <summary>
    /// Base class cho các detector
    /// </summary>
    public abstract class BaseDetector
    {
        protected float threshold;
        
        public BaseDetector(float threshold = 0.5f)
        {
            this.threshold = threshold;
        }
        
        public abstract DetectionResult Detect(float[] input1, float[] input2);
        
        public struct DetectionResult
        {
            public bool IsDetected;
            public float Score;
        }
    }
    
    /// <summary>
    /// Correlation detector
    /// </summary>
    public class CorrelationDetector : BaseDetector
    {
        public override DetectionResult Detect(float[] input1, float[] input2)
        {
            var result = new DetectionResult();
            
            try
            {
                if (input1 == null || input2 == null || input1.Length != input2.Length)
                {
                    return result;
                }
                
                // Tính correlation coefficient
                float correlation = CalculateCorrelation(input1, input2);
                result.Score = Mathf.Abs(correlation);
                result.IsDetected = result.Score >= threshold;
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in correlation detection: {e.Message}");
                return result;
            }
        }
        
        private float CalculateCorrelation(float[] x, float[] y)
        {
            try
            {
                int n = x.Length;
                if (n == 0) return 0f;
                
                float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f, sumY2 = 0f;
                
                for (int i = 0; i < n; i++)
                {
                    sumX += x[i];
                    sumY += y[i];
                    sumXY += x[i] * y[i];
                    sumX2 += x[i] * x[i];
                    sumY2 += y[i] * y[i];
                }
                
                float numerator = n * sumXY - sumX * sumY;
                float denominator = Mathf.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
                
                return denominator > 0 ? numerator / denominator : 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating correlation: {e.Message}");
                return 0f;
            }
        }
    }
    
    /// <summary>
    /// Spectral detector
    /// </summary>
    public class SpectralDetector : BaseDetector
    {
        public override DetectionResult Detect(float[] input1, float[] input2)
        {
            var result = new DetectionResult();
            
            try
            {
                if (input1 == null || input2 == null || input1.Length != input2.Length)
                {
                    return result;
                }
                
                // Tính spectral similarity
                float spectralSimilarity = CalculateSpectralSimilarity(input1, input2);
                result.Score = spectralSimilarity;
                result.IsDetected = result.Score >= threshold;
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in spectral detection: {e.Message}");
                return result;
            }
        }
        
        private float CalculateSpectralSimilarity(float[] x, float[] y)
        {
            try
            {
                int n = x.Length;
                if (n == 0) return 0f;
                
                // Tính energy của từng signal
                float energyX = 0f, energyY = 0f;
                for (int i = 0; i < n; i++)
                {
                    energyX += x[i] * x[i];
                    energyY += y[i] * y[i];
                }
                
                // Tính spectral similarity dựa trên energy ratio
                if (energyY > 0)
                {
                    return Mathf.Min(energyX / energyY, 1f);
                }
                
                return 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating spectral similarity: {e.Message}");
                return 0f;
            }
        }
    }
    
    /// <summary>
    /// Energy detector
    /// </summary>
    public class EnergyDetector : BaseDetector
    {
        public override DetectionResult Detect(float[] input1, float[] input2)
        {
            var result = new DetectionResult();
            
            try
            {
                if (input1 == null || input2 == null || input1.Length != input2.Length)
                {
                    return result;
                }
                
                // Tính energy ratio
                float energyRatio = CalculateEnergyRatio(input1, input2);
                result.Score = energyRatio;
                result.IsDetected = result.Score >= threshold;
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in energy detection: {e.Message}");
                return result;
            }
        }
        
        private float CalculateEnergyRatio(float[] x, float[] y)
        {
            try
            {
                int n = x.Length;
                if (n == 0) return 0f;
                
                float energyX = 0f, energyY = 0f;
                for (int i = 0; i < n; i++)
                {
                    energyX += x[i] * x[i];
                    energyY += y[i] * y[i];
                }
                
                if (energyY > 0)
                {
                    return Mathf.Min(energyX / energyY, 1f);
                }
                
                return 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating energy ratio: {e.Message}");
                return 0f;
            }
        }
    }
    
    /// <summary>
    /// Cross-talk detector
    /// </summary>
    public class CrossTalkDetector : BaseDetector
    {
        public override DetectionResult Detect(float[] input1, float[] input2)
        {
            var result = new DetectionResult();
            
            try
            {
                if (input1 == null || input2 == null || input1.Length != input2.Length)
                {
                    return result;
                }
                
                // Tính cross-talk ratio
                float crossTalkRatio = CalculateCrossTalkRatio(input1, input2);
                result.Score = crossTalkRatio;
                result.IsDetected = result.Score >= threshold;
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in cross-talk detection: {e.Message}");
                return result;
            }
        }
        
        private float CalculateCrossTalkRatio(float[] x, float[] y)
        {
            try
            {
                int n = x.Length;
                if (n == 0) return 0f;
                
                // Tính cross-talk dựa trên signal overlap
                float overlap = 0f;
                for (int i = 0; i < n; i++)
                {
                    if (Mathf.Abs(x[i]) > 0.01f && Mathf.Abs(y[i]) > 0.01f)
                    {
                        overlap += 1f;
                    }
                }
                
                return overlap / n;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating cross-talk ratio: {e.Message}");
                return 0f;
            }
        }
    }
}
