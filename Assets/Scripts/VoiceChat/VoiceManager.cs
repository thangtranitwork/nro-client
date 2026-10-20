using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class VoiceManager
{
    private static VoiceManager _Instance;
    public static VoiceManager Instance => _Instance ?? (_Instance = new VoiceManager());

    [Header("Recording Properties")]
    public int sampleRate = 8000;
    public int maxRecordTime = 60;
    public AudioClip recordedClip;
    private AudioSource audioSource;
    public bool isRecording;
    public List<VoiceData> listVoices = new List<VoiceData>();

    [Header("Ultra Compression Settings")]
    [Range(1, 10)]
    public int compressionLevel = 8;
    public bool enableVAD = true;
    public bool enableNoiseReduction = true;
    public bool enableAGC = true;
    private const float VAD_THRESHOLD = 0.02f;
    private const int SILENCE_FRAMES = 10;

    public void StartRecording()
    {
        if (isRecording) return;

        string micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (micName == null)
        {
            GameCanvas.startOKDlg("Không tìm thấy microphone!");
            return;
        }

        int minFreq, maxFreq;
        Microphone.GetDeviceCaps(micName, out minFreq, out maxFreq);
        int targetFreq = sampleRate;
        if (maxFreq > 0)
        {
            targetFreq = Mathf.Clamp(sampleRate, minFreq, maxFreq);
        }

        Debug.Log($"Recording optimized for voice: {targetFreq}Hz");
        recordedClip = Microphone.Start(micName, false, maxRecordTime, targetFreq);
        isRecording = true;
    }

    public void StopRecording(int currentTabIndex, int charID)
    {
        if (!isRecording) return;

        string micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (micName == null)
        {
            Debug.LogError("Không tìm thấy microphone!");
            return;
        }

        int recordedSamples = Microphone.GetPosition(micName);
        Microphone.End(micName);
        isRecording = false;

        if (recordedSamples <= 0 || recordedClip == null)
        {
            Debug.LogWarning("Không có dữ liệu ghi âm.");
            return;
        }
        AudioClip processedClip = ProcessRecordedAudio(recordedClip, recordedSamples);
        recordedClip = processedClip;

        SendVoiceMessage(ref recordedClip, currentTabIndex, charID);
    }

    private AudioClip ProcessRecordedAudio(AudioClip originalClip, int actualSamples)
    {
        int channels = originalClip.channels;
        int frequency = originalClip.frequency;
        float[] samples = new float[actualSamples * channels];
        originalClip.GetData(samples, 0);
        samples = ApplyVoiceProcessing(samples, frequency);
        AudioClip processedClip = AudioClip.Create("ProcessedVoice", samples.Length / channels, channels, frequency, false);
        processedClip.SetData(samples, 0);
        return processedClip;
    }

    private float[] ApplyVoiceProcessing(float[] samples, int sampleRate)
    {
        if (enableNoiseReduction)
        {
            samples = ApplyNoiseReduction(samples);
        }
        if (enableAGC)
        {
            samples = ApplyAGC(samples);
        }
        if (enableVAD)
        {
            samples = ApplyVAD(samples);
        }
        samples = ApplyVoiceFilter(samples, sampleRate);

        return samples;
    }

    private float[] ApplyNoiseReduction(float[] samples)
    {
        for (int i = 1; i < samples.Length - 1; i++)
        {
            float current = samples[i];
            if (Mathf.Abs(current) < 0.005f)
            {
                samples[i] = (samples[i - 1] + current + samples[i + 1]) * 0.2f;
            }
        }
        return samples;
    }

    private float[] ApplyAGC(float[] samples)
    {
        float rms = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            rms += samples[i] * samples[i];
        }
        rms = Mathf.Sqrt(rms / samples.Length);
        float targetRMS = 0.2f;
        float gain = rms > 0 ? targetRMS / rms : 1f;
        gain = Mathf.Clamp(gain, 0.1f, 3f);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= gain;
            samples[i] = Mathf.Clamp(samples[i], -1f, 1f);
        }
        return samples;
    }

    private float[] ApplyVAD(float[] samples)
    {
        List<float> voiceSegments = new List<float>();
        int frameSize = 160;
        int silenceCount = 0;

        for (int i = 0; i < samples.Length; i += frameSize)
        {
            int currentFrameSize = Mathf.Min(frameSize, samples.Length - i);
            float energy = 0f;
            for (int j = i; j < i + currentFrameSize; j++)
            {
                energy += samples[j] * samples[j];
            }
            energy = Mathf.Sqrt(energy / currentFrameSize);
            if (energy > VAD_THRESHOLD)
            {
                for (int j = i; j < i + currentFrameSize; j++)
                {
                    voiceSegments.Add(samples[j]);
                }
                silenceCount = 0;
            }
            else
            {
                silenceCount++;
                if (silenceCount <= 2)
                {
                    for (int j = i; j < i + currentFrameSize; j++)
                    {
                        voiceSegments.Add(samples[j] * 0.1f);
                    }
                }
            }
        }

        float compressionRatio = samples.Length > 0 ? (float)voiceSegments.Count / samples.Length : 1f;

        return voiceSegments.ToArray();
    }

    private float[] ApplyVoiceFilter(float[] samples, int sampleRate)
    {
        float highPassAlpha = 0.95f;
        float lowPassAlpha = 0.3f;

        float[] filtered = new float[samples.Length];
        float highPassPrev = 0f;
        float lowPassPrev = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float highPassed = highPassAlpha * (highPassPrev + samples[i] - (i > 0 ? samples[i - 1] : 0));
            highPassPrev = highPassed;
            float lowPassed = lowPassAlpha * highPassed + (1f - lowPassAlpha) * lowPassPrev;
            lowPassPrev = lowPassed;

            filtered[i] = lowPassed;
        }

        return filtered;
    }

    public void SendVoiceMessage(ref AudioClip aClip, int currentTabIndex, int charID)
    {
        //try
        //{
        //    Message msg = new(-50);
        //    msg.writer().writeByte(0);
        //    msg.writer().writeInt(charID);
        //    int originalSize = EstimateUncompressedSize(aClip);
        //    sbyte[] audioData = UltraCompressAudio(aClip);
        //    Debug.LogError("size audio :" + audioData.Length);
        //    msg.writer().writeFloat(aClip.length);

        //    msg.writer().writeInt(audioData.Length);
        //    for (int i = 0; i < audioData.Length; i++)
        //    {
        //        msg.writer().writeByte(audioData[i]);
        //    }

        //    Service.gI().session.sendMessage(msg);
        //    GameCanvas.panel.doSendMessage(currentTabIndex, mResources.VOICE_NAME);
        //    aClip = null;
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError($"Lỗi gửi voice message: {e.Message}");
        //    Debug.LogException(e);
        //}
    }

    private int EstimateUncompressedSize(AudioClip clip)
    {
        return clip.samples * clip.channels * 2;
    }

    private sbyte[] UltraCompressAudio(AudioClip clip)
    {
        if (clip == null || clip.samples == 0)
        {
            return new sbyte[0];
        }
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        byte[] compressed = null;
        compressed = CompressAdvancedADPCM(samples);
        compressed = ApplyDeltaEncoding(compressed);
        compressed = ApplyRunLengthEncoding(compressed);
        compressed = CompressFinal(compressed);
        return ArrayCast.cast(compressed);
    }

    private byte[] CompressAdvancedADPCM(float[] samples)
    {
        List<byte> compressed = new List<byte>();
        float predictor = 0f;
        float stepSize = 0.007f;
        int stepIndex = 0;
        int[] stepTable = { 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552 };
        int[] indexTable = { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8 };
        for (int i = 0; i < samples.Length; i += 2)
        {
            byte nibble1 = EncodeADPCMNibble(samples[i], ref predictor, ref stepSize, ref stepIndex, stepTable, indexTable);
            byte nibble2 = i + 1 < samples.Length ?
                EncodeADPCMNibble(samples[i + 1], ref predictor, ref stepSize, ref stepIndex, stepTable, indexTable) : (byte)0;

            compressed.Add((byte)((nibble1 << 4) | nibble2));
        }

        return compressed.ToArray();
    }

    private byte EncodeADPCMNibble(float sample, ref float predictor, ref float stepSize, ref int stepIndex, int[] stepTable, int[] indexTable)
    {
        float diff = sample - predictor;
        byte code = 0;

        if (diff >= 0)
        {
            code = 0;
        }
        else
        {
            code = 8;
            diff = -diff;
        }

        float step = stepSize;
        if (diff >= step)
        {
            code |= 4;
            diff -= step;
        }
        step *= 0.5f;
        if (diff >= step)
        {
            code |= 2;
            diff -= step;
        }
        step *= 0.5f;
        if (diff >= step)
        {
            code |= 1;
        }
        float diffq = stepSize * 0.125f;
        if ((code & 4) != 0) diffq += stepSize;
        if ((code & 2) != 0) diffq += stepSize * 0.5f;
        if ((code & 1) != 0) diffq += stepSize * 0.25f;

        if ((code & 8) != 0)
            predictor -= diffq;
        else
            predictor += diffq;

        predictor = Mathf.Clamp(predictor, -1f, 1f);
        stepIndex += indexTable[code];
        stepIndex = Mathf.Clamp(stepIndex, 0, stepTable.Length - 1);
        stepSize = stepTable[stepIndex] / 32768f;

        return code;
    }

    private byte[] ApplyDeltaEncoding(byte[] data)
    {
        if (data.Length == 0) return data;

        byte[] delta = new byte[data.Length];
        delta[0] = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            delta[i] = (byte)(data[i] - data[i - 1]);
        }

        return delta;
    }

    private byte[] ApplyRunLengthEncoding(byte[] data)
    {
        List<byte> rle = new List<byte>();

        for (int i = 0; i < data.Length; i++)
        {
            byte currentByte = data[i];
            int count = 1;
            while (i + count < data.Length && data[i + count] == currentByte && count < 255 && GameCanvas.gameStarted)
            {
                count++;
            }

            if (count >= 4 || currentByte == 0)
            {
                rle.Add(255);
                rle.Add((byte)count);
                rle.Add(currentByte);
                i += count - 1;
            }
            else
            {
                rle.Add(currentByte);
            }
        }

        return rle.ToArray();
    }

    private byte[] CompressFinal(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }
    public void PlayVoiceMessage(byte[] compressedData)
    {
        if (audioSource == null)
        {
            GameObject go = new GameObject("VoiceAudioSource");
            audioSource = go.AddComponent<AudioSource>();
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        AudioClip clip = DecompressAudio(compressedData, "VoiceMessage");
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private AudioClip DecompressAudio(byte[] compressedData, string clipName)
    {
        try
        {
            byte[] data = DecompressFinal(compressedData);
            data = ReverseRunLengthEncoding(data);
            data = ReverseDeltaEncoding(data);
            float[] samples = DecompressAdvancedADPCM(data);

            if (samples.Length == 0) return null;

            AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);

            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError($"Decompression error: {e.Message}");
            return null;
        }
    }

    private byte[] DecompressFinal(byte[] gzipData)
    {
        using (var input = new MemoryStream(gzipData))
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
    private byte[] ReverseRunLengthEncoding(byte[] rleData)
    {
        List<byte> decoded = new List<byte>();

        for (int i = 0; i < rleData.Length; i++)
        {
            if (rleData[i] == 255 && i + 2 < rleData.Length)
            {
                int count = rleData[i + 1];
                byte value = rleData[i + 2];

                for (int j = 0; j < count; j++)
                {
                    decoded.Add(value);
                }
                i += 2;
            }
            else
            {
                decoded.Add(rleData[i]);
            }
        }

        return decoded.ToArray();
    }

    private byte[] ReverseDeltaEncoding(byte[] deltaData)
    {
        if (deltaData.Length == 0) return deltaData;

        byte[] original = new byte[deltaData.Length];
        original[0] = deltaData[0];

        for (int i = 1; i < deltaData.Length; i++)
        {
            original[i] = (byte)(original[i - 1] + deltaData[i]);
        }

        return original;
    }

    private float[] DecompressAdvancedADPCM(byte[] compressed)
    {
        List<float> samples = new List<float>();

        float predictor = 0f;
        float stepSize = 0.007f;
        int stepIndex = 0;

        int[] stepTable = { 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552 };
        int[] indexTable = { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8 };

        foreach (byte b in compressed)
        {
            byte nibble1 = (byte)((b >> 4) & 0x0F);
            byte nibble2 = (byte)(b & 0x0F);

            samples.Add(DecodeADPCMNibble(nibble1, ref predictor, ref stepSize, ref stepIndex, stepTable, indexTable));
            samples.Add(DecodeADPCMNibble(nibble2, ref predictor, ref stepSize, ref stepIndex, stepTable, indexTable));
        }

        return samples.ToArray();
    }

    private float DecodeADPCMNibble(byte code, ref float predictor, ref float stepSize, ref int stepIndex, int[] stepTable, int[] indexTable)
    {
        float diffq = stepSize * 0.125f;
        if ((code & 4) != 0) diffq += stepSize;
        if ((code & 2) != 0) diffq += stepSize * 0.5f;
        if ((code & 1) != 0) diffq += stepSize * 0.25f;

        if ((code & 8) != 0)
            predictor -= diffq;
        else
            predictor += diffq;

        predictor = Mathf.Clamp(predictor, -1f, 1f);

        stepIndex += indexTable[code];
        stepIndex = Mathf.Clamp(stepIndex, 0, stepTable.Length - 1);
        stepSize = stepTable[stepIndex] / 32768f;

        return predictor;
    }
    public void ReceiveVoiceMessage(Message msg)
    {
        try
        {
            sbyte type = msg.reader().readByte();
            switch (type)
            {
                case 0:
                    addVoiceToList(msg);
                    break;
                case 1:
                    addVoiceData(msg);
                    break;
                case 2:
                    int messageID = msg.reader().readInt();
                    int charID = msg.reader().readInt();
                    string times = msg.reader().readUTF();
                    VoiceData voiceData = new VoiceData
                    {
                        messageID = messageID,
                        charID = charID,
                        time = times
                    };
                    listVoices.Add(voiceData);
                    Debug.LogError($"get Voice Message:" + messageID);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi nhận voice message: {e.Message}");
        }
    }

    private void addVoiceData(Message msg)
    {
        int messageID = msg.reader().readInt();
        if (checkVoiceData(messageID)) return;
        int dataLength = msg.reader().readInt();
        sbyte[] audioData = new sbyte[dataLength];
        for (int i = 0; i < dataLength; i++)
        {
            audioData[i] = msg.reader().readByte();
        }
        addDataToVoiceData(messageID, audioData);
    }

    private bool checkVoiceData(int messageID)
    {
        for (int i = 0; i < listVoices.Count; i++)
        {
            var v = listVoices[i];
            if (v != null && v.messageID == messageID && v.voiceData == null)
                return true;
        }
        return false;
    }

    private void addDataToVoiceData(int messageID, sbyte[] data)
    {
        for (int i = 0; i < listVoices.Count; i++)
        {
            if (listVoices != null && listVoices[i].messageID == messageID)
            {
                listVoices[i].voiceData = data;
                //GameCanvas.panel.addVoiceChat(listVoices[i], this);
                break;
            }
        }
    }

    private void addVoiceToList(Message msg)
    {
        listVoices.Clear();
        int size = msg.reader().readInt();
        for (int i = 0; i < size; i++)
        {
            int messageID = msg.reader().readInt();
            int charID = msg.reader().readInt();
            string times = msg.reader().readUTF();
            VoiceData voiceData = new VoiceData
            {
                messageID = messageID,
                charID = charID,
                time = times
            };
            listVoices.Add(voiceData);
        }
    }

    public void requestVoiceMessage(int messageID)
    {
        //Message msg = new Message(-50);
        //msg.writer().writeByte(1);
        //msg.writer().writeInt(messageID);
        //Service.gI().session.sendMessage(msg);
    }
    private AudioClip SByteArrayToAudioClip(byte[] sbytes, string clipName)
    {
        return DecompressAudio(sbytes, clipName);
    }
}