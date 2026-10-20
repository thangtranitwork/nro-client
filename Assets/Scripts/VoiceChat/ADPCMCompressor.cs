using System;
using UnityEngine;

namespace Assets.Scripts.VoiceChat
{
    public class ADPCMCompressor
    {
        private static readonly int[] stepTable = {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        private static readonly int[] indexTable = {
            -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8
        };

        public byte[] Compress(byte[] pcmData, int sampleRate)
        {
            if (pcmData == null || pcmData.Length == 0)
                return new byte[0];

            // Convert PCM16 to 16-bit samples
            int sampleCount = pcmData.Length / 2;
            short[] samples = new short[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = (short)((pcmData[i * 2 + 1] << 8) | pcmData[i * 2]);
            }

            // Compress to ADPCM: 2 samples per output byte
            int outLen = (sampleCount + 1) / 2;
            byte[] adpcmData = new byte[outLen];
            int adpcmIndex = 0;
            
            int predictor = 0;
            int step = stepTable[0];
            int index = 0;

            for (int i = 0; i < sampleCount; i += 2)
            {
                byte nibble1 = CompressSample(samples[i], ref predictor, ref step, ref index);
                byte nibble2 = (i + 1 < sampleCount) ? CompressSample(samples[i + 1], ref predictor, ref step, ref index) : (byte)0;
                
                adpcmData[adpcmIndex++] = (byte)((nibble2 << 4) | nibble1);
            }

            return adpcmData;
        }

        public byte[] Decompress(byte[] adpcmData)
        {
            if (adpcmData == null || adpcmData.Length == 0)
                return new byte[0];

            // Each ADPCM byte stores 2 samples (4-bit each)
            int sampleCount = adpcmData.Length * 2;
            byte[] pcmData = new byte[sampleCount * 2];
            int pcmIndex = 0;

            int predictor = 0;
            int step = stepTable[0];
            int index = 0;

            for (int i = 0; i < adpcmData.Length; i++)
            {
                byte byteData = adpcmData[i];
                // In our encoder we stored (nibble2 << 4) | nibble1
                byte nibble1 = (byte)(byteData & 0x0F);           // first sample
                byte nibble2 = (byte)((byteData >> 4) & 0x0F);    // second sample

                short sample1 = DecompressSample(nibble1, ref predictor, ref step, ref index);
                short sample2 = DecompressSample(nibble2, ref predictor, ref step, ref index);

                pcmData[pcmIndex++] = (byte)(sample1 & 0xFF);
                pcmData[pcmIndex++] = (byte)((sample1 >> 8) & 0xFF);
                pcmData[pcmIndex++] = (byte)(sample2 & 0xFF);
                pcmData[pcmIndex++] = (byte)((sample2 >> 8) & 0xFF);
            }

            return pcmData;
        }

        // Helper: safe try-decompress to PCM16; return null if fails
        public static byte[] TryDecompressToPCM16(byte[] input, int sampleRate)
        {
            try
            {
                if (input == null || input.Length == 0) return null;
                var c = new ADPCMCompressor();
                return c.Decompress(input);
            }
            catch
            {
                return null;
            }
        }

        private byte CompressSample(short sample, ref int predictor, ref int step, ref int index)
        {
            int stepValue = stepTable[index];
            int diff = sample - predictor;
            byte code = 0;

            if (diff < 0)
            {
                code = 8;
                diff = -diff;
            }

            int tempStep = stepValue;
            if (diff >= tempStep) { code |= 4; diff -= tempStep; }
            tempStep >>= 1;
            if (diff >= tempStep) { code |= 2; diff -= tempStep; }
            tempStep >>= 1;
            if (diff >= tempStep) { code |= 1; }

            int diffq = stepValue >> 3;
            if ((code & 4) != 0) diffq += stepValue;
            if ((code & 2) != 0) diffq += stepValue >> 1;
            if ((code & 1) != 0) diffq += stepValue >> 2;

            if ((code & 8) != 0) predictor -= diffq; else predictor += diffq;
            if (predictor > 32767) predictor = 32767;
            if (predictor < -32768) predictor = -32768;

            index += indexTable[code & 0x0F];
            if (index < 0) index = 0;
            if (index > 88) index = 88;
            step = stepTable[index];

            return (byte)(code & 0x0F);
        }

        private short DecompressSample(byte code, ref int predictor, ref int step, ref int index)
        {
            int stepValue = step;
            int diffq = stepValue >> 3;
            if ((code & 4) != 0) diffq += stepValue;
            if ((code & 2) != 0) diffq += stepValue >> 1;
            if ((code & 1) != 0) diffq += stepValue >> 2;

            if ((code & 8) != 0) predictor -= diffq; else predictor += diffq;
            if (predictor > 32767) predictor = 32767;
            if (predictor < -32768) predictor = -32768;

            index += indexTable[code & 0x0F];
            if (index < 0) index = 0;
            if (index > 88) index = 88;
            step = stepTable[index];

            return (short)predictor;
        }
    }
} 