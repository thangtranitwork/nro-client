using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.VoiceChat.EchoCancellationVoiceChat
{
    /// <summary>
    /// Core Echo Cancellation System sử dụng thuật toán NLMS (Normalized Least Mean Squares)
    /// Giải quyết vấn đề vang lại trong VoiceChat realtime
    /// </summary>
    public class EchoCancellationCore
    {
        [Header("Echo Cancellation Settings")]
        [SerializeField] private int filterLength = 1024; // Độ dài filter (samples)
        [SerializeField] private float learningRate = 0.1f; // Tốc độ học
        [SerializeField] private float regularization = 1e-6f; // Regularization để tránh division by zero
        
        // Adaptive Filter
        private float[] adaptiveFilter;
        private float[] filterBuffer;
        private float[] inputBuffer;
        private float[] outputBuffer;
        
        // NLMS Algorithm variables
        private float[] previousInput;
        private float[] previousOutput;
        private float energyEstimate;
        private float adaptationRate;
        
        // Performance monitoring
        private float echoSuppression = 0f;
        private float convergenceRate = 0f;
        private int totalSamplesProcessed = 0;
        
        // State management
        private bool isInitialized = false;
        private bool isAdapting = true;
        private float lastAdaptationTime = 0f;
        
        // Echo detection
        private float echoThreshold = 0.3f;
        private float echoDetectedLevel = 0f;
        private bool echoDetected = false;
        
        public EchoCancellationCore(int filterLength = 1024, float learningRate = 0.1f)
        {
            this.filterLength = filterLength;
            this.learningRate = learningRate;
            Initialize();
        }
        
        /// <summary>
        /// Khởi tạo hệ thống Echo Cancellation
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Khởi tạo adaptive filter
                adaptiveFilter = new float[filterLength];
                filterBuffer = new float[filterLength];
                inputBuffer = new float[filterLength];
                outputBuffer = new float[filterLength];
                
                // Khởi tạo buffer cho NLMS
                previousInput = new float[filterLength];
                previousOutput = new float[filterLength];
                
                // Khởi tạo filter với giá trị ngẫu nhiên nhỏ
                System.Random random = new System.Random();
                for (int i = 0; i < filterLength; i++)
                {
                    adaptiveFilter[i] = (float)(random.NextDouble() - 0.5) * 0.01f;
                }
                
                // Khởi tạo các biến
                energyEstimate = 1e-6f;
                adaptationRate = learningRate;
                
                isInitialized = true;
                
                Debug.Log($"EchoCancellationCore initialized: filterLength={filterLength}, learningRate={learningRate}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize EchoCancellationCore: {e.Message}");
                isInitialized = false;
            }
        }
        
        /// <summary>
        /// Xử lý echo cancellation cho một frame audio
        /// </summary>
        /// <param name="microphoneInput">Input từ microphone (có thể chứa echo)</param>
        /// <param name="speakerOutput">Output từ speaker (reference signal)</param>
        /// <param name="output">Output đã được xử lý (không có echo)</param>
        /// <returns>True nếu xử lý thành công</returns>
        public bool ProcessFrame(float[] microphoneInput, float[] speakerOutput, out float[] output)
        {
            output = null;
            
            if (!isInitialized || microphoneInput == null || speakerOutput == null)
                return false;
                
            try
            {
                int frameLength = Mathf.Min(microphoneInput.Length, speakerOutput.Length);
                if (frameLength <= 0) return false;
                
                // Khởi tạo output buffer
                output = new float[frameLength];
                
                // Xử lý từng sample
                for (int i = 0; i < frameLength; i++)
                {
                    float micSample = microphoneInput[i];
                    float speakerSample = speakerOutput[i];
                    
                    // Dự đoán echo sử dụng adaptive filter
                    float predictedEcho = PredictEcho(speakerSample);
                    
                    // Loại bỏ echo
                    float echoFreeSignal = micSample - predictedEcho;
                    
                    // Cập nhật adaptive filter
                    if (isAdapting)
                    {
                        UpdateFilter(speakerSample, micSample, predictedEcho);
                    }
                    
                    // Lưu output
                    output[i] = echoFreeSignal;
                }
                
                // Cập nhật performance metrics
                UpdatePerformanceMetrics(microphoneInput, output);
                
                totalSamplesProcessed += frameLength;
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in ProcessFrame: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Dự đoán echo sử dụng adaptive filter
        /// </summary>
        private float PredictEcho(float speakerSample)
        {
            float predictedEcho = 0f;
            
            // Shift input buffer
            for (int i = filterLength - 1; i > 0; i--)
            {
                inputBuffer[i] = inputBuffer[i - 1];
            }
            inputBuffer[0] = speakerSample;
            
            // Convolve với adaptive filter
            for (int i = 0; i < filterLength; i++)
            {
                predictedEcho += inputBuffer[i] * adaptiveFilter[i];
            }
            
            return predictedEcho;
        }
        
        /// <summary>
        /// Cập nhật adaptive filter sử dụng thuật toán NLMS
        /// </summary>
        private void UpdateFilter(float speakerSample, float micSample, float predictedEcho)
        {
            // Tính error
            float error = micSample - predictedEcho;
            
            // Cập nhật energy estimate
            energyEstimate = 0.99f * energyEstimate + 0.01f * speakerSample * speakerSample;
            
            // Tính adaptation rate
            float mu = learningRate / (energyEstimate + regularization);
            
            // Cập nhật filter coefficients
            for (int i = 0; i < filterLength; i++)
            {
                adaptiveFilter[i] += mu * error * inputBuffer[i];
            }
            
            // Giới hạn filter coefficients để tránh instability
            for (int i = 0; i < filterLength; i++)
            {
                adaptiveFilter[i] = Mathf.Clamp(adaptiveFilter[i], -1f, 1f);
            }
        }
        
        /// <summary>
        /// Cập nhật performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics(float[] originalInput, float[] processedOutput)
        {
            if (originalInput == null || processedOutput == null) return;
            
            try
            {
                // Tính echo suppression
                float inputEnergy = 0f;
                float outputEnergy = 0f;
                
                for (int i = 0; i < Mathf.Min(originalInput.Length, processedOutput.Length); i++)
                {
                    inputEnergy += originalInput[i] * originalInput[i];
                    outputEnergy += processedOutput[i] * processedOutput[i];
                }
                
                if (inputEnergy > 0)
                {
                    echoSuppression = 10f * Mathf.Log10(inputEnergy / Mathf.Max(outputEnergy, 1e-10f));
                }
                
                // Tính convergence rate
                float filterEnergy = 0f;
                for (int i = 0; i < filterLength; i++)
                {
                    filterEnergy += adaptiveFilter[i] * adaptiveFilter[i];
                }
                
                convergenceRate = filterEnergy / filterLength;
                
                // Phát hiện echo
                DetectEcho(originalInput, processedOutput);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating performance metrics: {e.Message}");
            }
        }
        
        /// <summary>
        /// Phát hiện echo trong tín hiệu
        /// </summary>
        private void DetectEcho(float[] originalInput, float[] processedOutput)
        {
            if (originalInput == null || processedOutput == null) return;
            
            try
            {
                // Tính correlation giữa input và output
                float correlation = 0f;
                int minLength = Mathf.Min(originalInput.Length, processedOutput.Length);
                
                for (int i = 0; i < minLength; i++)
                {
                    correlation += originalInput[i] * processedOutput[i];
                }
                
                correlation /= minLength;
                
                // Nếu correlation cao, có thể có echo
                if (Mathf.Abs(correlation) > echoThreshold)
                {
                    echoDetected = true;
                    echoDetectedLevel = Mathf.Abs(correlation);
                }
                else
                {
                    echoDetected = false;
                    echoDetectedLevel = 0f;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error detecting echo: {e.Message}");
            }
        }
        
        /// <summary>
        /// Reset adaptive filter
        /// </summary>
        public void ResetFilter()
        {
            if (!isInitialized) return;
            
            try
            {
                // Reset filter coefficients
                for (int i = 0; i < filterLength; i++)
                {
                    adaptiveFilter[i] = 0f;
                    inputBuffer[i] = 0f;
                    outputBuffer[i] = 0f;
                }
                
                // Reset performance metrics
                echoSuppression = 0f;
                convergenceRate = 0f;
                totalSamplesProcessed = 0;
                echoDetected = false;
                echoDetectedLevel = 0f;
                
                Debug.Log("Echo cancellation filter reset");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error resetting filter: {e.Message}");
            }
        }
        
        /// <summary>
        /// Bật/tắt adaptation
        /// </summary>
        public void SetAdaptation(bool enabled)
        {
            isAdapting = enabled;
            Debug.Log($"Echo cancellation adaptation: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Cập nhật learning rate
        /// </summary>
        public void SetLearningRate(float newRate)
        {
            learningRate = Mathf.Clamp(newRate, 0.01f, 1.0f);
            adaptationRate = learningRate;
            Debug.Log($"Echo cancellation learning rate updated: {learningRate}");
        }
        
        /// <summary>
        /// Lấy performance metrics
        /// </summary>
        public EchoCancellationMetrics GetMetrics()
        {
            return new EchoCancellationMetrics
            {
                EchoSuppression = echoSuppression,
                ConvergenceRate = convergenceRate,
                TotalSamplesProcessed = totalSamplesProcessed,
                EchoDetected = echoDetected,
                EchoDetectedLevel = echoDetectedLevel,
                FilterLength = filterLength,
                LearningRate = learningRate,
                IsAdapting = isAdapting
            };
        }
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                adaptiveFilter = null;
                filterBuffer = null;
                inputBuffer = null;
                outputBuffer = null;
                previousInput = null;
                previousOutput = null;
                
                isInitialized = false;
                
                Debug.Log("EchoCancellationCore disposed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disposing EchoCancellationCore: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Metrics để monitor hiệu suất của Echo Cancellation
    /// </summary>
    [System.Serializable]
    public struct EchoCancellationMetrics
    {
        public float EchoSuppression;        // dB
        public float ConvergenceRate;        // Filter energy
        public int TotalSamplesProcessed;    // Total samples processed
        public bool EchoDetected;            // Echo detection status
        public float EchoDetectedLevel;      // Echo correlation level
        public int FilterLength;             // Filter length
        public float LearningRate;           // Current learning rate
        public bool IsAdapting;              // Adaptation status
    }
}
