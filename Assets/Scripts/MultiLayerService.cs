using System;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;

/// <summary>
/// Service tích hợp hệ thống xác thực đa lớp với network hiện tại
/// </summary>
public class MultiLayerService
{
    // Singleton instance
    private static MultiLayerService instance;
    public static MultiLayerService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MultiLayerService();
            }
            return instance;
        }
    }
    
    // Constants
    private const byte MULTI_LAYER_CHALLENGE = 59;
    private const byte MULTI_LAYER_RESPONSE = 60;
    
    // Components
    private MultiLayerCryptoClient cryptoClient;
    private ISession currentSession;
    
    // State
    private bool isAuthenticated = false;
    private string currentChallengeId;
    private long challengeStartTime;
    private const long CHALLENGE_TIMEOUT_MS = 60000; // 60 seconds
    
    // Events
    public event Action<bool, string> OnAuthenticationResult;
    public event Action<long> OnPerformanceResult;
    public event Action<string> OnError;
    
    // Private constructor for singleton
    private MultiLayerService()
    {
        cryptoClient = MultiLayerCryptoClient.Instance;
        
        // Subscribe to crypto client events
        cryptoClient.OnResponseReady += HandleCryptoResponse;
        cryptoClient.OnError += HandleCryptoError;
        cryptoClient.OnTotalTime += HandlePerformanceResult;
        
        // Get current session
        currentSession = Session_ME.gI();
        if (currentSession == null)
        {
            currentSession = Session_ME2.gI();
        }
    }
    
    /// <summary>
    /// Bắt đầu quá trình xác thực đa lớp
    /// </summary>
    public void StartMultiLayerAuthentication()
    {
        try
        {
            if (!currentSession.isConnected())
            {
                OnError?.Invoke("Not connected to server");
                return;
            }
            
            if (isAuthenticated)
            {
                OnError?.Invoke("Already authenticated");
                return;
            }
            
            MultiLayerLogger.Log("RequestChallenge: sending MULTI_LAYER_CHALLENGE");
            // Gửi yêu cầu challenge
            RequestChallenge();
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"StartMultiLayerAuthentication error: {ex}");
            OnError?.Invoke($"Error starting authentication: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Yêu cầu challenge từ server
    /// </summary>
    private void RequestChallenge()
    {
        try
        {
            Message message = new Message(MULTI_LAYER_CHALLENGE);
            currentSession.sendMessage(message);
            
            challengeStartTime = mSystem.currentTimeMillis();
            MultiLayerLogger.Log($"Challenge requested at {challengeStartTime}");
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"RequestChallenge error: {ex}");
            OnError?.Invoke($"Error requesting challenge: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Xử lý challenge từ server
    /// </summary>
    public void HandleChallenge(Message message)
    {
        try
        {
            MultiLayerLogger.Log("HandleChallenge: parsing challenge");
            var challenge = ParseChallenge(message);
            if (challenge == null)
            {
                MultiLayerLogger.Log("HandleChallenge: parse failed");
                OnError?.Invoke("Failed to parse challenge");
                return;
            }
            
            currentChallengeId = challenge.challengeId;
            MultiLayerLogger.Log($"Received challengeId={currentChallengeId}, ts={challenge.timestamp}");
            
            // Bắt đầu xử lý challenge
            cryptoClient.ProcessChallenge(challenge);
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"HandleChallenge error: {ex}");
            OnError?.Invoke($"Error handling challenge: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Xử lý response từ crypto client
    /// </summary>
    private void HandleCryptoResponse(MultiLayerResponse response)
    {
        try
        {
            // Kiểm tra timeout
            if (mSystem.currentTimeMillis() - challengeStartTime > CHALLENGE_TIMEOUT_MS)
            {
                MultiLayerLogger.Log("HandleCryptoResponse: timeout");
                OnError?.Invoke("Challenge timeout");
                return;
            }
            
            MultiLayerLogger.Log("SendResponse: sending results to server");
            // Gửi response về server
            SendResponse(response);
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"HandleCryptoResponse error: {ex}");
            OnError?.Invoke($"Error handling crypto response: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gửi response về server
    /// </summary>
    private void SendResponse(MultiLayerResponse response)
    {
        try
        {
            Message message = new Message(MULTI_LAYER_RESPONSE);
            var writer = message.writer();
            
            // Gửi challenge ID
            writer.writeUTF(response.challengeId);
            
            // Gửi VRF response
            writer.writeUTF(response.vrfResponse.proof);
            
            // Gửi Commitment response
            writer.writeInt(response.commitmentResponse.proofs.Length);
            foreach (var proof in response.commitmentResponse.proofs)
            {
                writer.writeUTF(proof);
            }
            
            // Gửi ECC response
            writer.writeInt(response.eccResponse.results.Length);
            foreach (var result in response.eccResponse.results)
            {
                writer.writeUTF(result.x);
                writer.writeUTF(result.y);
            }
            
            // Gửi Pairing response
            writer.writeUTF(response.pairingResponse.pairingResult);
            
            // Gửi ZK response
            writer.writeUTF(response.zkResponse.proof);
            writer.writeUTF(response.zkResponse.challenge);
            
            currentSession.sendMessage(message);
            message.cleanup();
            
            MultiLayerLogger.Log("SendResponse: done");
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"SendResponse error: {ex}");
            OnError?.Invoke($"Error sending response: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Xử lý kết quả xác minh từ server
    /// </summary>
    public void HandleVerificationResult(Message message)
    {
        try
        {
            var reader = message.reader();
            
            bool success = reader.readBoolean();
            string messageText = reader.readUTF();
            long verificationTime = reader.readLong();
            
            isAuthenticated = success;
            MultiLayerLogger.Log($"VerificationResult: success={success}, msg={messageText}, time={verificationTime}ms");
            
            if (success)
            {
                Debug.Log($"Multi-layer authentication successful in {verificationTime}ms");
                OnAuthenticationResult?.Invoke(true, "Authentication successful");
            }
            else
            {
                Debug.LogError($"Multi-layer authentication failed: {messageText}");
                OnAuthenticationResult?.Invoke(false, messageText);
            }
            
        }
        catch (Exception ex)
        {
            MultiLayerLogger.Log($"HandleVerificationResult error: {ex}");
            OnError?.Invoke($"Error handling verification result: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Xử lý lỗi từ crypto client
    /// </summary>
    private void HandleCryptoError(string error)
    {
        MultiLayerLogger.Log($"Crypto error: {error}");
        OnError?.Invoke($"Crypto error: {error}");
    }
    
    /// <summary>
    /// Xử lý kết quả hiệu suất
    /// </summary>
    private void HandlePerformanceResult(long totalTime)
    {
        OnPerformanceResult?.Invoke(totalTime);
        
        // Log performance stats
        var stats = cryptoClient.GetPerformanceStats();
        MultiLayerLogger.Log($"Performance total={totalTime}ms, details={stats.Count}");
        foreach (var stat in stats)
        {
            MultiLayerLogger.Log($"Layer {stat.Key}: {stat.Value}ms");
        }
    }
    
    /// <summary>
    /// Parse challenge từ message
    /// </summary>
    private MultiLayerChallenge ParseChallenge(Message message)
    {
        try
        {
            var reader = message.reader();
            
            var challenge = new MultiLayerChallenge
            {
                challengeId = reader.readUTF(),
                timestamp = reader.readLong()
            };
            
            // Parse VRF challenge
            challenge.vrfChallenge = new VRFChallenge
            {
                p = reader.readUTF(),
                q = reader.readUTF(),
                g = reader.readUTF(),
                y = reader.readUTF(),
                x = reader.readUTF()
            };
            
            // Parse Commitment challenge
            challenge.commitmentChallenge = new CommitmentChallenge
            {
                p = reader.readUTF(),
                q = reader.readUTF(),
                g = reader.readUTF()
            };
            
            int commitmentCount = reader.readInt();
            challenge.commitmentChallenge.challenges = new string[commitmentCount];
            challenge.commitmentChallenge.commitments = new string[commitmentCount];
            
            for (int i = 0; i < commitmentCount; i++)
            {
                challenge.commitmentChallenge.challenges[i] = reader.readUTF();
            }
            for (int i = 0; i < commitmentCount; i++)
            {
                challenge.commitmentChallenge.commitments[i] = reader.readUTF();
            }
            
            // Parse ECC challenge
            challenge.eccChallenge = new ECCChallenge
            {
                p = reader.readUTF(),
                a = reader.readUTF(),
                b = reader.readUTF(),
                gx = reader.readUTF(),
                gy = reader.readUTF()
            };
            
            int eccCount = reader.readInt();
            challenge.eccChallenge.scalars = new string[eccCount];
            challenge.eccChallenge.points = new ECCPoint[eccCount];
            
            for (int i = 0; i < eccCount; i++)
            {
                challenge.eccChallenge.scalars[i] = reader.readUTF();
            }
            for (int i = 0; i < eccCount; i++)
            {
                challenge.eccChallenge.points[i] = new ECCPoint
                {
                    x = reader.readUTF(),
                    y = reader.readUTF()
                };
            }
            
            // Parse Pairing challenge
            challenge.pairingChallenge = new PairingChallenge();
            
            int group1Count = reader.readInt();
            challenge.pairingChallenge.group1Elements = new string[group1Count];
            for (int i = 0; i < group1Count; i++)
            {
                challenge.pairingChallenge.group1Elements[i] = reader.readUTF();
            }
            
            int group2Count = reader.readInt();
            challenge.pairingChallenge.group2Elements = new string[group2Count];
            for (int i = 0; i < group2Count; i++)
            {
                challenge.pairingChallenge.group2Elements[i] = reader.readUTF();
            }
            
            // Parse ZK challenge
            challenge.zkChallenge = new ZKChallenge
            {
                p = reader.readUTF(),
                q = reader.readUTF(),
                g = reader.readUTF(),
                h = reader.readUTF(),
                commitment = reader.readUTF(),
                secret = reader.readUTF()
            };
            
            return challenge;
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing challenge: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Kiểm tra trạng thái xác thực
    /// </summary>
    public bool IsAuthenticated()
    {
        return isAuthenticated;
    }
    
    /// <summary>
    /// Reset service
    /// </summary>
    public void Reset()
    {
        isAuthenticated = false;
        currentChallengeId = null;
        challengeStartTime = 0;
        
        if (cryptoClient != null)
        {
            cryptoClient.Reset();
        }
    }
    
    /// <summary>
    /// Lấy thống kê hiệu suất
    /// </summary>
    public Dictionary<string, long> GetPerformanceStats()
    {
        return cryptoClient?.GetPerformanceStats() ?? new Dictionary<string, long>();
    }
    
    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Cleanup()
    {
        // Unsubscribe from events
        if (cryptoClient != null)
        {
            cryptoClient.OnResponseReady -= HandleCryptoResponse;
            cryptoClient.OnError -= HandleCryptoError;
            cryptoClient.OnTotalTime -= HandlePerformanceResult;
        }
    }
}
