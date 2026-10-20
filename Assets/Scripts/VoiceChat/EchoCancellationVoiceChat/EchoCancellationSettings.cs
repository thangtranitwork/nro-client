using System;
using UnityEngine;

namespace Assets.Scripts.VoiceChat.EchoCancellationVoiceChat
{
    /// <summary>
    /// Cài đặt và cấu hình cho hệ thống Echo Cancellation
    /// Cho phép người dùng tùy chỉnh các tham số
    /// </summary>
    [CreateAssetMenu(fileName = "EchoCancellationSettings", menuName = "VoiceChat/Echo Cancellation Settings")]
    public class EchoCancellationSettings : ScriptableObject
    {
        [Header("Core Settings")]
        [SerializeField] private bool enableEchoCancellation = true;
        [SerializeField] private bool enableAdaptiveLearning = true;
        [SerializeField] private bool enableRealTimeMonitoring = true;
        
        [Header("Filter Configuration")]
        [SerializeField, Range(256, 4096)] private int filterLength = 1024;
        [SerializeField, Range(0.01f, 1.0f)] private float learningRate = 0.1f;
        [SerializeField, Range(1e-8f, 1e-4f)] private float regularization = 1e-6f;
        
        [Header("Detection Settings")]
        [SerializeField, Range(0.1f, 0.9f)] private float echoThreshold = 0.3f;
        [SerializeField, Range(0.1f, 0.9f)] private float correlationThreshold = 0.24f;
        [SerializeField, Range(0.1f, 0.9f)] private float spectralThreshold = 0.18f;
        [SerializeField, Range(0.1f, 0.9f)] private float energyThreshold = 0.12f;
        [SerializeField] private bool enableAdaptiveThresholds = true;
        
        [Header("Buffer Settings")]
        [SerializeField, Range(256, 2048)] private int frameSize = 512;
        [SerializeField, Range(5, 20)] private int bufferCapacity = 10;
        [SerializeField] private bool enableFrameAlignment = true;
        [SerializeField] private bool enableCircularBuffer = true;
        
        [Header("Performance Settings")]
        [SerializeField, Range(0.01f, 0.5f)] private float updateInterval = 0.1f;
        [SerializeField, Range(10, 1000)] private int maxHistorySize = 100;
        [SerializeField] private bool enablePerformanceLogging = true;
        
        [Header("Advanced Settings")]
        [SerializeField] private bool enableCrossTalkDetection = true;
        [SerializeField] private bool enableSpectralAnalysis = true;
        [SerializeField] private bool enableEnergyAnalysis = true;
        [SerializeField] private bool enableCorrelationAnalysis = true;
        
        // Events
        public static Action<EchoCancellationSettings> OnSettingsChanged;
        
        // Default values
        private const int DEFAULT_FILTER_LENGTH = 1024;
        private const float DEFAULT_LEARNING_RATE = 0.1f;
        private const float DEFAULT_ECHO_THRESHOLD = 0.3f;
        private const int DEFAULT_FRAME_SIZE = 512;
        
        /// <summary>
        /// Khởi tạo settings với giá trị mặc định
        /// </summary>
        private void OnEnable()
        {
            // Đảm bảo các giá trị trong range hợp lệ
            ValidateSettings();
        }
        
        /// <summary>
        /// Validate và cập nhật settings
        /// </summary>
        private void ValidateSettings()
        {
            try
            {
                // Validate filter length
                filterLength = Mathf.Clamp(filterLength, 256, 4096);
                
                // Validate learning rate
                learningRate = Mathf.Clamp(learningRate, 0.01f, 1.0f);
                
                // Validate regularization
                regularization = Mathf.Clamp(regularization, 1e-8f, 1e-4f);
                
                // Validate thresholds
                echoThreshold = Mathf.Clamp(echoThreshold, 0.1f, 0.9f);
                correlationThreshold = Mathf.Clamp(correlationThreshold, 0.1f, 0.9f);
                spectralThreshold = Mathf.Clamp(spectralThreshold, 0.1f, 0.9f);
                energyThreshold = Mathf.Clamp(energyThreshold, 0.1f, 0.9f);
                
                // Validate frame size
                frameSize = Mathf.Clamp(frameSize, 256, 2048);
                
                // Validate buffer capacity
                bufferCapacity = Mathf.Clamp(bufferCapacity, 5, 20);
                
                // Validate update interval
                updateInterval = Mathf.Clamp(updateInterval, 0.01f, 0.5f);
                
                // Validate history size
                maxHistorySize = Mathf.Clamp(maxHistorySize, 10, 1000);
                
                // Auto-adjust related thresholds
                if (enableAdaptiveThresholds)
                {
                    correlationThreshold = echoThreshold * 0.8f;
                    spectralThreshold = echoThreshold * 0.6f;
                    energyThreshold = echoThreshold * 0.4f;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error validating EchoCancellationSettings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Reset settings về giá trị mặc định
        /// </summary>
        public void ResetToDefaults()
        {
            try
            {
                enableEchoCancellation = true;
                enableAdaptiveLearning = true;
                enableRealTimeMonitoring = true;
                
                filterLength = DEFAULT_FILTER_LENGTH;
                learningRate = DEFAULT_LEARNING_RATE;
                regularization = 1e-6f;
                
                echoThreshold = DEFAULT_ECHO_THRESHOLD;
                correlationThreshold = DEFAULT_ECHO_THRESHOLD * 0.8f;
                spectralThreshold = DEFAULT_ECHO_THRESHOLD * 0.6f;
                energyThreshold = DEFAULT_ECHO_THRESHOLD * 0.4f;
                
                frameSize = DEFAULT_FRAME_SIZE;
                bufferCapacity = 10;
                enableFrameAlignment = true;
                enableCircularBuffer = true;
                
                updateInterval = 0.1f;
                maxHistorySize = 100;
                enablePerformanceLogging = true;
                
                enableCrossTalkDetection = true;
                enableSpectralAnalysis = true;
                enableEnergyAnalysis = true;
                enableCorrelationAnalysis = true;
                
                ValidateSettings();
                
                // Thông báo settings đã thay đổi
                OnSettingsChanged?.Invoke(this);
                
                Debug.Log("EchoCancellationSettings reset to defaults");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error resetting settings to defaults: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load settings từ PlayerPrefs
        /// </summary>
        public void LoadFromPlayerPrefs()
        {
            try
            {
                enableEchoCancellation = PlayerPrefs.GetInt("EC_EnableEchoCancellation", 1) == 1;
                enableAdaptiveLearning = PlayerPrefs.GetInt("EC_EnableAdaptiveLearning", 1) == 1;
                enableRealTimeMonitoring = PlayerPrefs.GetInt("EC_EnableRealTimeMonitoring", 1) == 1;
                
                filterLength = PlayerPrefs.GetInt("EC_FilterLength", DEFAULT_FILTER_LENGTH);
                learningRate = PlayerPrefs.GetFloat("EC_LearningRate", DEFAULT_LEARNING_RATE);
                regularization = PlayerPrefs.GetFloat("EC_Regularization", 1e-6f);
                
                echoThreshold = PlayerPrefs.GetFloat("EC_EchoThreshold", DEFAULT_ECHO_THRESHOLD);
                correlationThreshold = PlayerPrefs.GetFloat("EC_CorrelationThreshold", DEFAULT_ECHO_THRESHOLD * 0.8f);
                spectralThreshold = PlayerPrefs.GetFloat("EC_SpectralThreshold", DEFAULT_ECHO_THRESHOLD * 0.6f);
                energyThreshold = PlayerPrefs.GetFloat("EC_EnergyThreshold", DEFAULT_ECHO_THRESHOLD * 0.4f);
                
                frameSize = PlayerPrefs.GetInt("EC_FrameSize", DEFAULT_FRAME_SIZE);
                bufferCapacity = PlayerPrefs.GetInt("EC_BufferCapacity", 10);
                enableFrameAlignment = PlayerPrefs.GetInt("EC_EnableFrameAlignment", 1) == 1;
                enableCircularBuffer = PlayerPrefs.GetInt("EC_EnableCircularBuffer", 1) == 1;
                
                updateInterval = PlayerPrefs.GetFloat("EC_UpdateInterval", 0.1f);
                maxHistorySize = PlayerPrefs.GetInt("EC_MaxHistorySize", 100);
                enablePerformanceLogging = PlayerPrefs.GetInt("EC_EnablePerformanceLogging", 1) == 1;
                
                enableCrossTalkDetection = PlayerPrefs.GetInt("EC_EnableCrossTalkDetection", 1) == 1;
                enableSpectralAnalysis = PlayerPrefs.GetInt("EC_EnableSpectralAnalysis", 1) == 1;
                enableEnergyAnalysis = PlayerPrefs.GetInt("EC_EnableEnergyAnalysis", 1) == 1;
                enableCorrelationAnalysis = PlayerPrefs.GetInt("EC_EnableCorrelationAnalysis", 1) == 1;
                
                ValidateSettings();
                
                Debug.Log("EchoCancellationSettings loaded from PlayerPrefs");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading settings from PlayerPrefs: {e.Message}");
            }
        }
        
        /// <summary>
        /// Save settings vào PlayerPrefs
        /// </summary>
        public void SaveToPlayerPrefs()
        {
            try
            {
                PlayerPrefs.SetInt("EC_EnableEchoCancellation", enableEchoCancellation ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableAdaptiveLearning", enableAdaptiveLearning ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableRealTimeMonitoring", enableRealTimeMonitoring ? 1 : 0);
                
                PlayerPrefs.SetInt("EC_FilterLength", filterLength);
                PlayerPrefs.SetFloat("EC_LearningRate", learningRate);
                PlayerPrefs.SetFloat("EC_Regularization", regularization);
                
                PlayerPrefs.SetFloat("EC_EchoThreshold", echoThreshold);
                PlayerPrefs.SetFloat("EC_CorrelationThreshold", correlationThreshold);
                PlayerPrefs.SetFloat("EC_SpectralThreshold", spectralThreshold);
                PlayerPrefs.SetFloat("EC_EnergyThreshold", energyThreshold);
                
                PlayerPrefs.SetInt("EC_FrameSize", frameSize);
                PlayerPrefs.SetInt("EC_BufferCapacity", bufferCapacity);
                PlayerPrefs.SetInt("EC_EnableFrameAlignment", enableFrameAlignment ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableCircularBuffer", enableCircularBuffer ? 1 : 0);
                
                PlayerPrefs.SetFloat("EC_UpdateInterval", updateInterval);
                PlayerPrefs.SetInt("EC_MaxHistorySize", maxHistorySize);
                PlayerPrefs.SetInt("EC_EnablePerformanceLogging", enablePerformanceLogging ? 1 : 0);
                
                PlayerPrefs.SetInt("EC_EnableCrossTalkDetection", enableCrossTalkDetection ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableSpectralAnalysis", enableSpectralAnalysis ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableEnergyAnalysis", enableEnergyAnalysis ? 1 : 0);
                PlayerPrefs.SetInt("EC_EnableCorrelationAnalysis", enableCorrelationAnalysis ? 1 : 0);
                
                PlayerPrefs.Save();
                
                Debug.Log("EchoCancellationSettings saved to PlayerPrefs");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving settings to PlayerPrefs: {e.Message}");
            }
        }
        
        /// <summary>
        /// Export settings thành JSON
        /// </summary>
        public string ExportToJSON()
        {
            try
            {
                var settingsData = new
                {
                    enableEchoCancellation,
                    enableAdaptiveLearning,
                    enableRealTimeMonitoring,
                    filterLength,
                    learningRate,
                    regularization,
                    echoThreshold,
                    correlationThreshold,
                    spectralThreshold,
                    energyThreshold,
                    frameSize,
                    bufferCapacity,
                    enableFrameAlignment,
                    enableCircularBuffer,
                    updateInterval,
                    maxHistorySize,
                    enablePerformanceLogging,
                    enableCrossTalkDetection,
                    enableSpectralAnalysis,
                    enableEnergyAnalysis,
                    enableCorrelationAnalysis,
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                return JsonUtility.ToJson(settingsData, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error exporting settings to JSON: {e.Message}");
                return "{}";
            }
        }
        
        /// <summary>
        /// Import settings từ JSON
        /// </summary>
        public bool ImportFromJSON(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData)) return false;
                
                var settingsData = JsonUtility.FromJson<EchoCancellationSettingsData>(jsonData);
                
                if (settingsData != null)
                {
                    enableEchoCancellation = settingsData.enableEchoCancellation;
                    enableAdaptiveLearning = settingsData.enableAdaptiveLearning;
                    enableRealTimeMonitoring = settingsData.enableRealTimeMonitoring;
                    
                    filterLength = settingsData.filterLength;
                    learningRate = settingsData.learningRate;
                    regularization = settingsData.regularization;
                    
                    echoThreshold = settingsData.echoThreshold;
                    correlationThreshold = settingsData.correlationThreshold;
                    spectralThreshold = settingsData.spectralThreshold;
                    energyThreshold = settingsData.energyThreshold;
                    
                    frameSize = settingsData.frameSize;
                    bufferCapacity = settingsData.bufferCapacity;
                    enableFrameAlignment = settingsData.enableFrameAlignment;
                    enableCircularBuffer = settingsData.enableCircularBuffer;
                    
                    updateInterval = settingsData.updateInterval;
                    maxHistorySize = settingsData.maxHistorySize;
                    enablePerformanceLogging = settingsData.enablePerformanceLogging;
                    
                    enableCrossTalkDetection = settingsData.enableCrossTalkDetection;
                    enableSpectralAnalysis = settingsData.enableSpectralAnalysis;
                    enableEnergyAnalysis = settingsData.enableEnergyAnalysis;
                    enableCorrelationAnalysis = settingsData.enableCorrelationAnalysis;
                    
                    ValidateSettings();
                    
                    // Thông báo settings đã thay đổi
                    OnSettingsChanged?.Invoke(this);
                    
                    Debug.Log("EchoCancellationSettings imported from JSON");
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error importing settings from JSON: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate settings và trả về danh sách lỗi
        /// </summary>
        public string[] ValidateSettingsWithErrors()
        {
            var errors = new System.Collections.Generic.List<string>();
            
            try
            {
                if (filterLength < 256 || filterLength > 4096)
                {
                    errors.Add($"Filter length must be between 256 and 4096, current: {filterLength}");
                }
                
                if (learningRate < 0.01f || learningRate > 1.0f)
                {
                    errors.Add($"Learning rate must be between 0.01 and 1.0, current: {learningRate}");
                }
                
                if (echoThreshold < 0.1f || echoThreshold > 0.9f)
                {
                    errors.Add($"Echo threshold must be between 0.1 and 0.9, current: {echoThreshold}");
                }
                
                if (frameSize < 256 || frameSize > 2048)
                {
                    errors.Add($"Frame size must be between 256 and 2048, current: {frameSize}");
                }
                
                if (bufferCapacity < 5 || bufferCapacity > 20)
                {
                    errors.Add($"Buffer capacity must be between 5 and 20, current: {bufferCapacity}");
                }
                
                if (updateInterval < 0.01f || updateInterval > 0.5f)
                {
                    errors.Add($"Update interval must be between 0.01 and 0.5, current: {updateInterval}");
                }
                
                if (maxHistorySize < 10 || maxHistorySize > 1000)
                {
                    errors.Add($"Max history size must be between 10 and 1000, current: {maxHistorySize}");
                }
            }
            catch (Exception e)
            {
                errors.Add($"Error validating settings: {e.Message}");
            }
            
            return errors.ToArray();
        }
        
        /// <summary>
        /// Lấy settings summary
        /// </summary>
        public string GetSettingsSummary()
        {
            try
            {
                return $"Echo Cancellation Settings:\n" +
                       $"Enabled: {enableEchoCancellation}\n" +
                       $"Filter Length: {filterLength}\n" +
                       $"Learning Rate: {learningRate:F3}\n" +
                       $"Echo Threshold: {echoThreshold:F3}\n" +
                       $"Frame Size: {frameSize}\n" +
                       $"Buffer Capacity: {bufferCapacity}\n" +
                       $"Update Interval: {updateInterval:F3}s\n" +
                       $"Adaptive Learning: {enableAdaptiveLearning}\n" +
                       $"Real-time Monitoring: {enableRealTimeMonitoring}";
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting settings summary: {e.Message}");
                return "Error getting settings summary";
            }
        }
        
        // Properties để truy cập settings
        public bool EnableEchoCancellation
        {
            get => enableEchoCancellation;
            set
            {
                if (enableEchoCancellation != value)
                {
                    enableEchoCancellation = value;
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public bool EnableAdaptiveLearning
        {
            get => enableAdaptiveLearning;
            set
            {
                if (enableAdaptiveLearning != value)
                {
                    enableAdaptiveLearning = value;
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public int FilterLength
        {
            get => filterLength;
            set
            {
                int newValue = Mathf.Clamp(value, 256, 4096);
                if (filterLength != newValue)
                {
                    filterLength = newValue;
                    ValidateSettings();
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public float LearningRate
        {
            get => learningRate;
            set
            {
                float newValue = Mathf.Clamp(value, 0.01f, 1.0f);
                if (learningRate != newValue)
                {
                    learningRate = newValue;
                    ValidateSettings();
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public float EchoThreshold
        {
            get => echoThreshold;
            set
            {
                float newValue = Mathf.Clamp(value, 0.1f, 0.9f);
                if (echoThreshold != newValue)
                {
                    echoThreshold = newValue;
                    ValidateSettings();
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public int FrameSize
        {
            get => frameSize;
            set
            {
                int newValue = Mathf.Clamp(value, 256, 2048);
                if (frameSize != newValue)
                {
                    frameSize = newValue;
                    ValidateSettings();
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
        
        public float UpdateInterval
        {
            get => updateInterval;
            set
            {
                float newValue = Mathf.Clamp(value, 0.01f, 0.5f);
                if (updateInterval != newValue)
                {
                    updateInterval = newValue;
                    ValidateSettings();
                    OnSettingsChanged?.Invoke(this);
                }
            }
        }
    }
    
    /// <summary>
    /// Data structure để import/export settings
    /// </summary>
    [System.Serializable]
    public class EchoCancellationSettingsData
    {
        public bool enableEchoCancellation;
        public bool enableAdaptiveLearning;
        public bool enableRealTimeMonitoring;
        public int filterLength;
        public float learningRate;
        public float regularization;
        public float echoThreshold;
        public float correlationThreshold;
        public float spectralThreshold;
        public float energyThreshold;
        public int frameSize;
        public int bufferCapacity;
        public bool enableFrameAlignment;
        public bool enableCircularBuffer;
        public float updateInterval;
        public int maxHistorySize;
        public bool enablePerformanceLogging;
        public bool enableCrossTalkDetection;
        public bool enableSpectralAnalysis;
        public bool enableEnergyAnalysis;
        public bool enableCorrelationAnalysis;
    }
}
