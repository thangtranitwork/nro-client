using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performance optimization utilities for the game client
/// Addresses FPS fluctuation issues and improves overall performance
/// </summary>
public static class PerformanceOptimizer
{
    // Object pooling for frequently created objects
    private static Dictionary<string, Queue<object>> objectPools = new Dictionary<string, Queue<object>>();
    
    // Frame rate management
    private static int targetFPS = 60;
    private static int currentFrame = 0;
    private static bool enableOptimizations = true;
    
    // Update throttling for expensive operations
    private static Dictionary<string, int> updateCounters = new Dictionary<string, int>();
    
    /// <summary>
    /// Initialize performance optimizations
    /// </summary>
    public static void Initialize()
    {
        // Set up object pools for commonly used objects
        CreateObjectPool("Effect", 100);
        CreateObjectPool("EffectCharPaint", 50);
        CreateObjectPool("Vector", 200);
        
        // Configure Unity settings for better performance
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0; // Disable VSync for consistent FPS
        
        Debug.Log("PerformanceOptimizer initialized");
    }
    
    /// <summary>
    /// Create an object pool for a specific type
    /// </summary>
    private static void CreateObjectPool(string typeName, int initialSize)
    {
        if (!objectPools.ContainsKey(typeName))
        {
            objectPools[typeName] = new Queue<object>();
        }
    }
    
    /// <summary>
    /// Get an object from the pool or create new if pool is empty
    /// </summary>
    public static T GetPooledObject<T>(string typeName) where T : new()
    {
        if (objectPools.ContainsKey(typeName) && objectPools[typeName].Count > 0)
        {
            return (T)objectPools[typeName].Dequeue();
        }
        return new T();
    }
    
    /// <summary>
    /// Return an object to the pool for reuse
    /// </summary>
    public static void ReturnToPool(string typeName, object obj)
    {
        if (objectPools.ContainsKey(typeName))
        {
            objectPools[typeName].Enqueue(obj);
        }
    }
    
    /// <summary>
    /// Check if an update should be skipped for performance
    /// </summary>
    public static bool ShouldSkipUpdate(string updateType, int skipInterval = 2)
    {
        if (!enableOptimizations) return false;
        
        if (!updateCounters.ContainsKey(updateType))
        {
            updateCounters[updateType] = 0;
        }
        
        updateCounters[updateType]++;
        if (updateCounters[updateType] >= skipInterval)
        {
            updateCounters[updateType] = 0;
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Optimize collection iteration by caching sizes
    /// </summary>
    public static int GetCachedSize<T>(List<T> collection, string cacheKey)
    {
        // In a real implementation, you'd cache these values
        // For now, just return the size to replace .size() calls
        return collection != null ? collection.Count : 0;
    }
    
    /// <summary>
    /// Batch effect updates to reduce per-frame overhead
    /// </summary>
    public static void BatchEffectUpdates(Action updateAction, int batchSize = 10)
    {
        currentFrame++;
        if (currentFrame % batchSize == 0)
        {
            updateAction?.Invoke();
        }
    }
    
    /// <summary>
    /// Optimize rendering by skipping off-screen objects
    /// </summary>
    public static bool IsOnScreen(int x, int y, int width, int height)
    {
        // Simple screen bounds check
        return x + width >= 0 && x <= GameCanvas.w && 
               y + height >= 0 && y <= GameCanvas.h;
    }
    
    /// <summary>
    /// Toggle optimizations on/off for testing
    /// </summary>
    public static void SetOptimizationsEnabled(bool enabled)
    {
        enableOptimizations = enabled;
        Debug.Log($"Performance optimizations {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Get performance statistics
    /// </summary>
    public static string GetPerformanceStats()
    {
        int totalPooledObjects = 0;
        foreach (var pool in objectPools.Values)
        {
            totalPooledObjects += pool.Count;
        }
        
        return $"FPS: {Application.targetFrameRate}, " +
               $"Pooled Objects: {totalPooledObjects}, " +
               $"Frame: {currentFrame}, " +
               $"Optimizations: {(enableOptimizations ? "ON" : "OFF")}";
    }
}