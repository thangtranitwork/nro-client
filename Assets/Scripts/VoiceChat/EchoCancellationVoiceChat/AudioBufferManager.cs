using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.VoiceChat.EchoCancellationVoiceChat
{
    /// <summary>
    /// Quản lý audio buffer cho hệ thống Echo Cancellation
    /// Xử lý circular buffer, frame alignment và buffer synchronization
    /// </summary>
    public class AudioBufferManager
    {
        [Header("Buffer Configuration")]
        private int frameSize;
        private int bufferCapacity;
        private int readIndex;
        private int writeIndex;
        private int availableSamples;
        
        // Circular buffers
        private float[] microphoneBuffer;
        private float[] speakerBuffer;
        private float[] processedBuffer;
        
        // Buffer synchronization
        private readonly object bufferLock = new object();
        private bool isBufferFull = false;
        private bool isBufferEmpty = true;
        
        // Performance monitoring
        private int totalSamplesWritten = 0;
        private int totalSamplesRead = 0;
        private int bufferOverflows = 0;
        private int bufferUnderflows = 0;
        
        // Frame alignment
        private int frameAlignmentOffset = 0;
        private bool enableFrameAlignment = true;
        
        public AudioBufferManager(int frameSize = 512, int bufferCapacity = 10)
        {
            this.frameSize = frameSize;
            this.bufferCapacity = bufferCapacity;
            InitializeBuffers();
        }
        
        /// <summary>
        /// Khởi tạo audio buffers
        /// </summary>
        private void InitializeBuffers()
        {
            try
            {
                int totalBufferSize = frameSize * bufferCapacity;
                
                microphoneBuffer = new float[totalBufferSize];
                speakerBuffer = new float[totalBufferSize];
                processedBuffer = new float[totalBufferSize];
                
                // Reset indices
                readIndex = 0;
                writeIndex = 0;
                availableSamples = 0;
                
                // Clear buffers
                Array.Clear(microphoneBuffer, 0, microphoneBuffer.Length);
                Array.Clear(speakerBuffer, 0, speakerBuffer.Length);
                Array.Clear(processedBuffer, 0, processedBuffer.Length);
                
                Debug.Log($"AudioBufferManager initialized: frameSize={frameSize}, capacity={bufferCapacity}, totalSize={totalBufferSize}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize AudioBufferManager: {e.Message}");
            }
        }
        
        /// <summary>
        /// Ghi microphone data vào buffer
        /// </summary>
        public bool WriteMicrophoneData(float[] data, int offset = 0, int length = -1)
        {
            if (data == null || data.Length == 0) return false;
            
            lock (bufferLock)
            {
                try
                {
                    int dataLength = length > 0 ? length : data.Length;
                    int samplesToWrite = Mathf.Min(dataLength, frameSize);
                    
                    // Kiểm tra buffer capacity
                    if (availableSamples + samplesToWrite > microphoneBuffer.Length)
                    {
                        bufferOverflows++;
                        Debug.LogWarning($"Microphone buffer overflow: {availableSamples + samplesToWrite} > {microphoneBuffer.Length}");
                        return false;
                    }
                    
                    // Ghi data vào buffer
                    for (int i = 0; i < samplesToWrite; i++)
                    {
                        int bufferIndex = (writeIndex + i) % microphoneBuffer.Length;
                        microphoneBuffer[bufferIndex] = data[offset + i];
                    }
                    
                    // Cập nhật indices
                    writeIndex = (writeIndex + samplesToWrite) % microphoneBuffer.Length;
                    availableSamples += samplesToWrite;
                    
                    // Cập nhật buffer state
                    isBufferEmpty = false;
                    if (availableSamples >= microphoneBuffer.Length)
                    {
                        isBufferFull = true;
                    }
                    
                    totalSamplesWritten += samplesToWrite;
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error writing microphone data: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Ghi speaker data vào buffer
        /// </summary>
        public bool WriteSpeakerData(float[] data, int offset = 0, int length = -1)
        {
            if (data == null || data.Length == 0) return false;
            
            lock (bufferLock)
            {
                try
                {
                    int dataLength = length > 0 ? length : data.Length;
                    int samplesToWrite = Mathf.Min(dataLength, frameSize);
                    
                    // Kiểm tra buffer capacity
                    if (availableSamples + samplesToWrite > speakerBuffer.Length)
                    {
                        bufferOverflows++;
                        Debug.LogWarning($"Speaker buffer overflow: {availableSamples + samplesToWrite} > {speakerBuffer.Length}");
                        return false;
                    }
                    
                    // Ghi data vào buffer
                    for (int i = 0; i < samplesToWrite; i++)
                    {
                        int bufferIndex = (writeIndex + i) % speakerBuffer.Length;
                        speakerBuffer[bufferIndex] = data[offset + i];
                    }
                    
                    // Cập nhật indices
                    writeIndex = (writeIndex + samplesToWrite) % speakerBuffer.Length;
                    availableSamples += samplesToWrite;
                    
                    // Cập nhật buffer state
                    isBufferEmpty = false;
                    if (availableSamples >= speakerBuffer.Length)
                    {
                        isBufferFull = true;
                    }
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error writing speaker data: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Đọc microphone data từ buffer
        /// </summary>
        public bool ReadMicrophoneData(float[] output, int offset = 0)
        {
            if (output == null || output.Length < frameSize) return false;
            
            lock (bufferLock)
            {
                try
                {
                    if (availableSamples < frameSize)
                    {
                        bufferUnderflows++;
                        return false;
                    }
                    
                    // Đọc data từ buffer
                    for (int i = 0; i < frameSize; i++)
                    {
                        int bufferIndex = (readIndex + i) % microphoneBuffer.Length;
                        output[offset + i] = microphoneBuffer[bufferIndex];
                    }
                    
                    // Cập nhật indices
                    readIndex = (readIndex + frameSize) % microphoneBuffer.Length;
                    availableSamples -= frameSize;
                    
                    // Cập nhật buffer state
                    isBufferFull = false;
                    if (availableSamples <= 0)
                    {
                        isBufferEmpty = true;
                    }
                    
                    totalSamplesRead += frameSize;
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading microphone data: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Đọc speaker data từ buffer
        /// </summary>
        public bool ReadSpeakerData(float[] output, int offset = 0)
        {
            if (output == null || output.Length < frameSize) return false;
            
            lock (bufferLock)
            {
                try
                {
                    if (availableSamples < frameSize)
                    {
                        bufferUnderflows++;
                        return false;
                    }
                    
                    // Đọc data từ buffer
                    for (int i = 0; i < frameSize; i++)
                    {
                        int bufferIndex = (readIndex + i) % speakerBuffer.Length;
                        output[offset + i] = speakerBuffer[bufferIndex];
                    }
                    
                    // Cập nhật indices
                    readIndex = (readIndex + frameSize) % speakerBuffer.Length;
                    availableSamples -= frameSize;
                    
                    // Cập nhật buffer state
                    isBufferFull = false;
                    if (availableSamples <= 0)
                    {
                        isBufferEmpty = true;
                    }
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading speaker data: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Lấy microphone frame với frame alignment
        /// </summary>
        public bool GetMicrophoneFrame(float[] output, bool enableAlignment = true)
        {
            if (output == null || output.Length < frameSize) return false;
            
            lock (bufferLock)
            {
                try
                {
                    if (availableSamples < frameSize)
                    {
                        return false;
                    }
                    
                    // Frame alignment nếu được bật
                    if (enableAlignment && enableFrameAlignment)
                    {
                        // Tìm frame boundary tốt nhất
                        int alignedReadIndex = FindFrameBoundary(readIndex);
                        if (alignedReadIndex != readIndex)
                        {
                            frameAlignmentOffset = alignedReadIndex - readIndex;
                            readIndex = alignedReadIndex;
                        }
                    }
                    
                    // Đọc frame
                    for (int i = 0; i < frameSize; i++)
                    {
                        int bufferIndex = (readIndex + i) % microphoneBuffer.Length;
                        output[i] = microphoneBuffer[bufferIndex];
                    }
                    
                    // Cập nhật indices
                    readIndex = (readIndex + frameSize) % microphoneBuffer.Length;
                    availableSamples -= frameSize;
                    
                    // Cập nhật buffer state
                    isBufferFull = false;
                    if (availableSamples <= 0)
                    {
                        isBufferEmpty = true;
                    }
                    
                    totalSamplesRead += frameSize;
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting microphone frame: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Lấy speaker frame với frame alignment
        /// </summary>
        public bool GetSpeakerFrame(float[] output, bool enableAlignment = true)
        {
            if (output == null || output.Length < frameSize) return false;
            
            lock (bufferLock)
            {
                try
                {
                    if (availableSamples < frameSize)
                    {
                        return false;
                    }
                    
                    // Frame alignment nếu được bật
                    if (enableAlignment && enableFrameAlignment)
                    {
                        // Tìm frame boundary tốt nhất
                        int alignedReadIndex = FindFrameBoundary(readIndex);
                        if (alignedReadIndex != readIndex)
                        {
                            frameAlignmentOffset = alignedReadIndex - readIndex;
                            readIndex = alignedReadIndex;
                        }
                    }
                    
                    // Đọc frame
                    for (int i = 0; i < frameSize; i++)
                    {
                        int bufferIndex = (readIndex + i) % speakerBuffer.Length;
                        output[i] = speakerBuffer[bufferIndex];
                    }
                    
                    // Cập nhật indices
                    readIndex = (readIndex + frameSize) % speakerBuffer.Length;
                    availableSamples -= frameSize;
                    
                    // Cập nhật buffer state
                    isBufferFull = false;
                    if (availableSamples <= 0)
                    {
                        isBufferEmpty = true;
                    }
                    
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting speaker frame: {e.Message}");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Tìm frame boundary tốt nhất
        /// </summary>
        private int FindFrameBoundary(int currentIndex)
        {
            try
            {
                // Tìm frame boundary dựa trên zero-crossing hoặc energy threshold
                int bestBoundary = currentIndex;
                float bestScore = float.MaxValue;
                
                // Tìm trong range ±frameSize/2
                int searchRange = frameSize / 2;
                for (int offset = -searchRange; offset <= searchRange; offset++)
                {
                    int testIndex = (currentIndex + offset + microphoneBuffer.Length) % microphoneBuffer.Length;
                    
                    if (testIndex + frameSize <= microphoneBuffer.Length)
                    {
                        float score = CalculateFrameBoundaryScore(testIndex);
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestBoundary = testIndex;
                        }
                    }
                }
                
                return bestBoundary;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error finding frame boundary: {e.Message}");
                return currentIndex;
            }
        }
        
        /// <summary>
        /// Tính score cho frame boundary
        /// </summary>
        private float CalculateFrameBoundaryScore(int startIndex)
        {
            try
            {
                float score = 0f;
                
                // Tính energy discontinuity
                if (startIndex > 0)
                {
                    float energyBefore = 0f;
                    float energyAfter = 0f;
                    
                    // Energy trước boundary
                    for (int i = Mathf.Max(0, startIndex - frameSize/4); i < startIndex; i++)
                    {
                        energyBefore += microphoneBuffer[i] * microphoneBuffer[i];
                    }
                    
                    // Energy sau boundary
                    for (int i = startIndex; i < Mathf.Min(microphoneBuffer.Length, startIndex + frameSize/4); i++)
                    {
                        energyAfter += microphoneBuffer[i] * microphoneBuffer[i];
                    }
                    
                    // Score dựa trên energy discontinuity
                    score += Mathf.Abs(energyAfter - energyBefore);
                }
                
                // Score dựa trên zero-crossing
                int zeroCrossings = 0;
                for (int i = startIndex; i < Mathf.Min(microphoneBuffer.Length - 1, startIndex + frameSize/8); i++)
                {
                    if ((microphoneBuffer[i] >= 0 && microphoneBuffer[i + 1] < 0) ||
                        (microphoneBuffer[i] < 0 && microphoneBuffer[i + 1] >= 0))
                    {
                        zeroCrossings++;
                    }
                }
                
                score += zeroCrossings * 0.1f;
                
                return score;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calculating frame boundary score: {e.Message}");
                return float.MaxValue;
            }
        }
        
        /// <summary>
        /// Lấy buffer status
        /// </summary>
        public AudioBufferStatus GetBufferStatus()
        {
            lock (bufferLock)
            {
                return new AudioBufferStatus
                {
                    AvailableSamples = availableSamples,
                    BufferCapacity = microphoneBuffer.Length,
                    BufferUsage = (float)availableSamples / microphoneBuffer.Length,
                    IsBufferFull = isBufferFull,
                    IsBufferEmpty = isBufferEmpty,
                    ReadIndex = readIndex,
                    WriteIndex = writeIndex,
                    FrameAlignmentOffset = frameAlignmentOffset,
                    TotalSamplesWritten = totalSamplesWritten,
                    TotalSamplesRead = totalSamplesRead,
                    BufferOverflows = bufferOverflows,
                    BufferUnderflows = bufferUnderflows
                };
            }
        }
        
        /// <summary>
        /// Reset buffer
        /// </summary>
        public void ResetBuffer()
        {
            lock (bufferLock)
            {
                try
                {
                    // Reset indices
                    readIndex = 0;
                    writeIndex = 0;
                    availableSamples = 0;
                    
                    // Clear buffers
                    Array.Clear(microphoneBuffer, 0, microphoneBuffer.Length);
                    Array.Clear(speakerBuffer, 0, speakerBuffer.Length);
                    Array.Clear(processedBuffer, 0, processedBuffer.Length);
                    
                    // Reset state
                    isBufferFull = false;
                    isBufferEmpty = true;
                    frameAlignmentOffset = 0;
                    
                    // Reset metrics
                    totalSamplesWritten = 0;
                    totalSamplesRead = 0;
                    bufferOverflows = 0;
                    bufferUnderflows = 0;
                    
                    Debug.Log("AudioBufferManager reset");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error resetting buffer: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Cập nhật frame size
        /// </summary>
        public void SetFrameSize(int newFrameSize)
        {
            if (newFrameSize <= 0 || newFrameSize == frameSize) return;
            
            lock (bufferLock)
            {
                try
                {
                    frameSize = newFrameSize;
                    
                    // Reinitialize buffers với frame size mới
                    InitializeBuffers();
                    
                    Debug.Log($"Frame size updated: {frameSize}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error updating frame size: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Bật/tắt frame alignment
        /// </summary>
        public void SetFrameAlignmentEnabled(bool enabled)
        {
            enableFrameAlignment = enabled;
            Debug.Log($"Frame alignment: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                microphoneBuffer = null;
                speakerBuffer = null;
                processedBuffer = null;
                
                Debug.Log("AudioBufferManager disposed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disposing AudioBufferManager: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Status của audio buffer
    /// </summary>
    [System.Serializable]
    public struct AudioBufferStatus
    {
        public int AvailableSamples;
        public int BufferCapacity;
        public float BufferUsage;
        public bool IsBufferFull;
        public bool IsBufferEmpty;
        public int ReadIndex;
        public int WriteIndex;
        public int FrameAlignmentOffset;
        public int TotalSamplesWritten;
        public int TotalSamplesRead;
        public int BufferOverflows;
        public int BufferUnderflows;
    }
}
