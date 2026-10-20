using System;
using UnityEngine;

/// <summary>
/// Controller tích hợp hệ thống xác thực đa lớp với Controller hiện tại
/// </summary>
public class MultiLayerController
{
    // Singleton instance
    private static MultiLayerController instance;
    public static MultiLayerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MultiLayerController();
            }
            return instance;
        }
    }
    
    private MultiLayerService multiLayerService;
    
    // Private constructor for singleton
    private MultiLayerController()
    {
        multiLayerService = MultiLayerService.Instance;
        
        // Subscribe to events
        multiLayerService.OnAuthenticationResult += HandleAuthenticationResult;
        multiLayerService.OnPerformanceResult += HandlePerformanceResult;
        multiLayerService.OnError += HandleError;
    }
    
    /// <summary>
    /// Xử lý message từ server
    /// </summary>
    public void HandleMessage(Message message)
    {
        try
        {
            sbyte command = message.command;
            
            switch (command)
            {
                case 59: // MULTI_LAYER_CHALLENGE
                    Debug.LogError("getChallenge");
                    multiLayerService.HandleChallenge(message);
                    break;
                    
                case 60: // MULTI_LAYER_RESPONSE
                    Debug.LogError("getChallenge2");
                    multiLayerService.HandleVerificationResult(message);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown multi-layer command: {command}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling multi-layer message: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Bắt đầu xác thực đa lớp
    /// </summary>
    public void StartAuthentication()
    {
        Debug.LogError("StartMultiLayerAuthentication");
        multiLayerService.StartMultiLayerAuthentication();
    }
    
    /// <summary>
    /// Xử lý kết quả xác thực
    /// </summary>
    private void HandleAuthenticationResult(bool success, string message)
    {
        if (success)
        {
            Debug.Log($"Multi-layer authentication successful: {message}");
            // Có thể thêm logic game ở đây
        }
        else
        {
            Debug.LogError($"Multi-layer authentication failed: {message}");
        }
    }
    
    /// <summary>
    /// Xử lý kết quả hiệu suất
    /// </summary>
    private void HandlePerformanceResult(long totalTime)
    {
        Debug.Log($"Multi-layer processing completed in {totalTime}ms");
        
        // Log performance stats
        var stats = multiLayerService.GetPerformanceStats();
        foreach (var stat in stats)
        {
            Debug.Log($"  {stat.Key}: {stat.Value}ms");
        }
    }
    
    /// <summary>
    /// Xử lý lỗi
    /// </summary>
    private void HandleError(string error)
    {
        Debug.LogError($"Multi-layer error: {error}");
    }
    
    /// <summary>
    /// Kiểm tra trạng thái xác thực
    /// </summary>
    public bool IsAuthenticated()
    {
        return multiLayerService.IsAuthenticated();
    }
    
    /// <summary>
    /// Reset controller
    /// </summary>
    public void Reset()
    {
        multiLayerService.Reset();
    }
    
    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Cleanup()
    {
        // Unsubscribe from events
        if (multiLayerService != null)
        {
            multiLayerService.OnAuthenticationResult -= HandleAuthenticationResult;
            multiLayerService.OnPerformanceResult -= HandlePerformanceResult;
            multiLayerService.OnError -= HandleError;
        }
    }
}
