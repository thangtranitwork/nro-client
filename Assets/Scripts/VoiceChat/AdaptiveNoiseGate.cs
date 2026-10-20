using System;
using UnityEngine;

namespace Assets.Scripts.VoiceChat
{
    public class AdaptiveNoiseGate
    {
        private float threshold;
        private float sampleRate;
        private float attackTime = 0.01f; // 10ms
        private float releaseTime = 0.1f; // 100ms
        private float currentGain = 1.0f;
        private float targetGain = 1.0f;
        
        public AdaptiveNoiseGate(float threshold, float sampleRate)
        {
            this.threshold = threshold;
            this.sampleRate = sampleRate;
        }
        
        public void ProcessSamples(float[] samples)
        {
            if (samples == null || samples.Length == 0)
                return;
                
            for (int i = 0; i < samples.Length; i++)
            {
                float sample = samples[i];
                float amplitude = Mathf.Abs(sample);
                
                // Tính toán target gain dựa trên amplitude
                if (amplitude > threshold)
                {
                    targetGain = 1.0f;
                }
                else
                {
                    targetGain = 0.0f;
                }
                
                // Smooth transition
                float attackRate = 1.0f / (attackTime * sampleRate);
                float releaseRate = 1.0f / (releaseTime * sampleRate);
                
                if (targetGain > currentGain)
                {
                    currentGain = Mathf.Min(targetGain, currentGain + attackRate);
                }
                else
                {
                    currentGain = Mathf.Max(targetGain, currentGain - releaseRate);
                }
                
                // Áp dụng gain
                samples[i] = sample * currentGain;
            }
        }
        
        public void SetThreshold(float newThreshold)
        {
            threshold = Mathf.Clamp(newThreshold, 0.001f, 1.0f);
        }
        
        public void SetAttackTime(float attackTimeMs)
        {
            attackTime = Mathf.Clamp(attackTimeMs / 1000f, 0.001f, 1.0f);
        }
        
        public void SetReleaseTime(float releaseTimeMs)
        {
            releaseTime = Mathf.Clamp(releaseTimeMs / 1000f, 0.001f, 1.0f);
        }
    }
} 