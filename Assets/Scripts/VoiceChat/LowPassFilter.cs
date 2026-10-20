using System;
using UnityEngine;

namespace Assets.Scripts.VoiceChat
{
    public class LowPassFilter
    {
        private float cutoffFrequency;
        private float sampleRate;
        private float x1 = 0f, x2 = 0f;
        private float y1 = 0f, y2 = 0f;
        private float a0, a1, a2, b1, b2;
        
        public LowPassFilter(float cutoffFrequency, float sampleRate)
        {
            this.cutoffFrequency = cutoffFrequency;
            this.sampleRate = sampleRate;
            CalculateCoefficients();
        }
        
        private void CalculateCoefficients()
        {
            float omega = 2f * Mathf.PI * cutoffFrequency / sampleRate;
            float alpha = Mathf.Sin(omega) / (2f * 0.707f); // Q = 0.707
            
            float cosw = Mathf.Cos(omega);
            float alpha2 = alpha * alpha;
            
            a0 = 1f + alpha;
            a1 = 2f * cosw;
            a2 = 1f - alpha;
            b1 = a1;
            b2 = a2;
            
            // Normalize
            float norm = 1f / a0;
            a0 *= norm;
            a1 *= norm;
            a2 *= norm;
            b1 *= norm;
            b2 *= norm;
        }
        
        public void ProcessSamples(float[] samples)
        {
            if (samples == null || samples.Length == 0)
                return;
                
            for (int i = 0; i < samples.Length; i++)
            {
                float input = samples[i];
                float output = a0 * input + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
                
                // Update history
                x2 = x1;
                x1 = input;
                y2 = y1;
                y1 = output;
                
                samples[i] = output;
            }
        }
        
        public void SetCutoffFrequency(float newCutoff)
        {
            cutoffFrequency = Mathf.Clamp(newCutoff, 20f, sampleRate / 2f);
            CalculateCoefficients();
        }
    }
} 