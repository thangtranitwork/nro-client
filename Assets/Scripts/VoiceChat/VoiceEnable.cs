using System;
using UnityEngine;

namespace Assets.Scripts.VoiceChat
{
    public static class VoiceEnable
    {
        // Trạng thái loa (speaker)
        public static bool enableLoaMap = false;
        public static bool enableLoaBang = false;
        
        // Trạng thái microphone
        public static bool enableMICMap = false;
        public static bool enableMICBang = false;
        public static byte getTypeMIC()
        {
            if (enableMICMap && enableMICBang)
            {
                return 3;
            }

            if (enableMICMap && !enableMICBang)
            {
                return 2;
            }

            if (!enableMICMap && enableMICBang)
            {
                return 1;
            }
            return 0;
        }
        // Trạng thái tổng thể
        public static bool IsVoiceChatEnabled => enableLoaMap || enableLoaBang || enableMICMap || enableMICBang;
        
        // Trạng thái phát âm thanh
        public static bool IsPlaybackEnabled => enableLoaMap || enableLoaBang;
        
        // Trạng thái thu âm
        public static bool IsRecordingEnabled => enableMICMap || enableMICBang;
        
        // Reset tất cả trạng thái
        public static void ResetAll()
        {
            enableLoaMap = false;
            enableLoaBang = false;
            enableMICMap = false;
            enableMICBang = false;
            
            Debug.Log("Voice chat states reset");
        }
        
        // Bật/tắt voice chat cho Map
        public static void EnableMapVoiceChat(bool enable)
        {
            enableLoaMap = enable;
            enableMICMap = enable;
            Debug.Log($"Map voice chat: {(enable ? "enabled" : "disabled")}");
        }
        
        // Bật/tắt voice chat cho Bang
        public static void EnableBangVoiceChat(bool enable)
        {
            enableLoaBang = enable;
            enableMICBang = enable;
            Debug.Log($"Bang voice chat: {(enable ? "enabled" : "disabled")}");
        }
        
        // Bật/tắt tất cả voice chat
        public static void EnableAllVoiceChat(bool enable)
        {
            enableLoaMap = enable;
            enableLoaBang = enable;
            enableMICMap = enable;
            enableMICBang = enable;
            Debug.Log($"All voice chat: {(enable ? "enabled" : "disabled")}");
        }
        
        // Lấy thông tin trạng thái
        public static string GetStatusInfo()
        {
            return $"Voice Chat Status:\n" +
                   $"Loa Map: {enableLoaMap}\n" +
                   $"Loa Bang: {enableLoaBang}\n" +
                   $"MIC Map: {enableMICMap}\n" +
                   $"MIC Bang: {enableMICBang}\n" +
                   $"Total Enabled: {IsVoiceChatEnabled}";
        }
    }
} 