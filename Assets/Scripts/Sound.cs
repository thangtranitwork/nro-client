using System;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Sound
{
	private const int INTERVAL = 5;

	private const int MAXTIME = 100;

	public static int status;

	public static int postem;

	public static int timestart;

	private static string filenametemp;

	private static float volumetem;

	public static bool isSound = true;

	public static bool isNotPlay;

	public static bool stopAll;

	public static AudioSource SoundWater;

	public static AudioSource SoundRun;

	public static AudioSource SoundBGLoop;

	public static AudioClip[] music;

	public static GameObject[] player;

	public static sbyte MLogin;

	public static sbyte MBClick = 1;

	public static sbyte MTone = 2;

	public static sbyte MSanzu = 3;

	public static sbyte MChakumi = 4;

	public static sbyte MChai = 5;

	public static sbyte MOshin = 6;

	public static sbyte MEchigo = 7;

	public static sbyte MKojin = 8;

	public static sbyte MHaruna = 9;

	public static sbyte MHirosaki = 10;

	public static sbyte MOokaza = 11;

	public static sbyte MGiotuyet = 12;

	public static sbyte MHangdong = 13;

	public static sbyte MDeKeu = 14;

	public static sbyte MChimKeu = 15;

	public static sbyte MBuocChan = 16;

	public static sbyte MNuocChay = 17;

	public static sbyte MBomMau = 18;

	public static sbyte MKiemGo = 19;

	public static sbyte MKiem = 20;

	public static sbyte MTieu = 21;

	public static sbyte MKunai = 22;

	public static sbyte MCung = 23;

	public static sbyte MDao = 24;

	public static sbyte MQuat = 25;

	public static sbyte MCung2 = 26;

	public static sbyte MTieu2 = 27;

	public static sbyte MTieu3 = 28;

	public static sbyte MKiem2 = 29;

	public static sbyte MKiem3 = 30;

	public static sbyte MDao2 = 31;

	public static sbyte MDao3 = 32;

	public static sbyte MCung3 = 33;

	public static int l1;

	public static void setActivity(SoundMn.AssetManager ac)
	{
	}

	public static void stop()
	{
		for (int i = 0; i < player.Length; i++)
		{
			if (player[i] != null)
			{
				player[i].GetComponent<AudioSource>().Pause();
			}
		}
	}

	public static bool isPlaying()
	{
		return false;
	}

	public static void init()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "Audio Player";
		gameObject.transform.position = Vector3.zero;
		gameObject.AddComponent<AudioListener>();
		SoundBGLoop = gameObject.AddComponent<AudioSource>();
	}

	public static void init(int[] musicID, int[] sID)
	{
		if (player == null && music == null)
		{
			init();
			l1 = musicID.Length;
			player = new GameObject[musicID.Length + sID.Length];
			music = new AudioClip[musicID.Length + sID.Length];
			for (int i = 0; i < player.Length; i++)
			{
				string fileName = ((i >= l1) ? ("/sound/" + (i - l1)) : ("/music/" + i));
				getAssetSoundFile(fileName, i);
			}
			Debug.LogError("Finish load Sound");
		}
	}

	public static void playSound(int id, float volume)
	{
		play(id + l1, volume);
	}

	public static void playSound1(int id, float volume)
	{
		play(id, volume);
	}

	public static void getAssetSoundFile(string fileName, int pos)
	{
		stop(pos);
		string empty = string.Empty;
		empty = Main.res + fileName;
		load(empty, pos);
	}

	public static void stopAllz()
	{
		for (int i = 0; i < music.Length; i++)
		{
			stop(i);
		}
		for (int j = 0; j < l1; j++)
		{
			sTopSoundBG(j);
		}
	}

	public static void stopAllBg()
	{
		for (int i = 0; i < music.Length; i++)
		{
			stop(i);
		}
		sTopSoundBG(0);
		sTopSoundRun();
		stopSoundNatural(0);
	}

	public static void update()
	{
	}

	public static void stopMusic(int x)
	{
		if (MoreSetting.soundEnabled)
		{
			stop(x);
		}
	}

	public static void play(int id, float volume)
	{
		if (!isNotPlay && MoreSetting.soundEnabled)
		{
			start(volume, id);
	//		Debug.LogError("startsound");
		}
	}

	public static void playSoundRun(int id, float volume)
	{
		if (MoreSetting.soundEnabled && !(SoundRun == null))
		{
			SoundRun.GetComponent<AudioSource>().loop = true;
			SoundRun.GetComponent<AudioSource>().clip = music[id];
			SoundRun.GetComponent<AudioSource>().volume = volume;
			SoundRun.GetComponent<AudioSource>().Play();
		}
	}

	public static void sTopSoundRun()
	{
		SoundRun.GetComponent<AudioSource>().Stop();
	}

	public static bool isPlayingSound()
	{
		if (SoundRun == null)
		{
			return false;
		}
		return SoundRun.GetComponent<AudioSource>().isPlaying;
	}

	public static void playSoundNatural(int id, float volume, bool isLoop)
	{
		if (MoreSetting.soundEnabled && !(SoundBGLoop == null))
		{
			SoundWater.GetComponent<AudioSource>().loop = isLoop;
			SoundWater.GetComponent<AudioSource>().clip = music[id];
			SoundWater.GetComponent<AudioSource>().volume = volume;
			SoundWater.GetComponent<AudioSource>().Play();
		}
	}

	public static void stopSoundNatural(int id)
	{
		SoundWater.GetComponent<AudioSource>().Stop();
	}

	public static bool isPlayingSoundatural(int id)
	{
		if (SoundWater == null)
		{
			return false;
		}
		return SoundWater.GetComponent<AudioSource>().isPlaying;
	}

	public static void playMus(int type, float vl, bool loop)
	{
		if (!isNotPlay)
		{
			vl -= 0.3f;
			if (vl <= 0f)
			{
				vl = 0.01f;
			}
			playSoundBGLoop(type, vl);
		}
	}

	public static void playSoundBGLoop(int id, float volume)
	{
		if (MoreSetting.soundEnabled)
		{
			if (id == SoundMn.AIR_SHIP)
			{
				playSound1(id, volume + 0.2f);
			}
			else if (!(SoundBGLoop == null) && !isPlayingSoundBG(id))
			{
				SoundBGLoop.GetComponent<AudioSource>().loop = true;
				SoundBGLoop.GetComponent<AudioSource>().clip = music[id];
				SoundBGLoop.GetComponent<AudioSource>().volume = volume;
				SoundBGLoop.GetComponent<AudioSource>().Play();
			}
		}
	}

	public static void sTopSoundBG(int id)
	{
		SoundBGLoop.GetComponent<AudioSource>().Stop();
	}

	public static bool isPlayingSoundBG(int id)
	{
		if (SoundBGLoop == null)
		{
			return false;
		}
		return SoundBGLoop.GetComponent<AudioSource>().isPlaying;
	}

	public static void load(string filename, int pos)
	{
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			__load(filename, pos);
		}
		else
		{
			_load(filename, pos);
		}
	}

	private static void _load(string filename, int pos)
	{
		if (status != 0)
		{
			Cout.LogError("CANNOT LOAD AUDIO " + filename + " WHEN LOADING " + filenametemp);
			return;
		}
		filenametemp = filename;
		postem = pos;
		status = 2;
		int i;
		for (i = 0; i < 100; i++)
		{
			Thread.Sleep(5);
			if (status == 0)
			{
				break;
			}
		}
		if (i == 100)
		{
			Cout.LogError("TOO LONG FOR LOAD AUDIO " + filename);
			return;
		}
		Cout.Log("Load Audio " + filename + " done in " + i * 5 + "ms");
	}

	private static void __load(string filename, int pos)
	{
		try
		{
			music[pos] = (AudioClip)Resources.Load(filename, typeof(AudioClip));
			GameObject.Find("Main Camera").AddComponent<AudioSource>();
			player[pos] = GameObject.Find("Main Camera");
		}
		catch(Exception e) { Debug.LogError(e.ToString()); }
	}

	public static void start(float volume, int pos)
	{
		//Debug.LogError("startSound1 : volume :" + volume);
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			//Debug.LogError("startSound2 : volume :" + volume);
			__start(volume, pos);
		}
		else
		{
			//Debug.LogError("startSound3 : volume :" + volume);
			_start(volume, pos);
		}
	}

	public static void _start(float volume, int pos)
	{
		Debug.LogError("startSound4 : volume :" + volume);
		if (status != 0)
		{
			Debug.LogError("CANNOT START AUDIO WHEN STARTING");
			return;
		}
		volumetem = volume;
		postem = pos;
		status = 3;
		int i;
		for (i = 0; i < 100; i++)
		{
			Thread.Sleep(5);
			if (status == 0)
			{
				break;
			}
		}
		if (i == 100)
		{
			Debug.LogError("TOO LONG FOR START AUDIO");
		}
		else
		{
			Debug.Log("Start Audio done in " + i * 5 + "ms");
		}
	}

	public static void __start(float volume, int pos)
    {
        if (player[pos] != null)
        {
            player[pos].GetComponent<AudioSource>().PlayOneShot(music[pos], volume);
           // Debug.LogError("startSound5 : volume :" + volume);
        }
		else
        {
			Debug.LogError(player[pos]);
		}
    }

    public static void stop(int pos)
	{
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			__stop(pos);
		}
		else
		{
			_stop(pos);
		}
	}

	public static void _stop(int pos)
	{
		if (status != 0)
		{
			Debug.LogError("CANNOT STOP AUDIO WHEN STOPPING");
			return;
		}
		postem = pos;
		status = 4;
		int i;
		for (i = 0; i < 100; i++)
		{
			Thread.Sleep(5);
			if (status == 0)
			{
				break;
			}
		}
		if (i == 100)
		{
			Debug.LogError("TOO LONG FOR STOP AUDIO");
		}
		else
		{
			Debug.Log("Stop Audio done in " + i * 5 + "ms");
		}
	}

	public static void __stop(int pos)
	{
		if (player[pos] != null)
		{
			player[pos].GetComponent<AudioSource>().Stop();
		}
	}

	/// <summary>
	/// Phát âm thanh từ dữ liệu byte array (File.ReadAllBytes)
	/// </summary>
	/// <param name="sounds">Dữ liệu âm thanh dưới dạng byte array</param>
	/// <param name="volume">Âm lượng (0.0f - 1.0f)</param>
	/// <param name="loop">Có lặp lại không</param>
	public static void playSound(byte[] sounds, float volume = 1.0f, bool loop = false)
	{
		if (sounds == null || sounds.Length == 0)
		{
			Debug.LogWarning("Sound data is null or empty");
			return;
		}

		if (isNotPlay)
		{
			return;
		}

		try
		{
			// Sử dụng audio pool để tối ưu performance
			AudioSource audioSource = GetAudioSourceFromPool();
			if (audioSource == null)
			{
				Debug.LogError("No available AudioSource in pool");
				return;
			}

			// Tạo hoặc lấy AudioClip từ cache
			AudioClip audioClip = GetOrCreateAudioClip(sounds);
			if (audioClip == null)
			{
				Debug.LogError("Failed to create AudioClip from byte data");
				ReturnAudioSourceToPool(audioSource);
				return;
			}

			// Cấu hình và phát âm thanh
			audioSource.clip = audioClip;
			audioSource.volume = volume;
			audioSource.loop = loop;
			audioSource.Play();

			// Trả về pool sau khi phát xong (nếu không loop)
			if (!loop)
			{
				// Sử dụng MonoBehaviour để chạy coroutine
				MonoBehaviour mb = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
				if (mb != null)
				{
					mb.StartCoroutine(ReturnToPoolAfterPlay(audioSource, audioClip.length));
				}
				else
				{
					// Fallback: trả về pool sau delay
					ReturnAudioSourceToPool(audioSource);
				}
			}

			Debug.Log($"Playing audio from byte data, length: {audioClip.length}s, volume: {volume}");
		}
		catch (Exception e)
		{
			Debug.LogError($"Error playing sound from byte data: {e.Message}");
		}
	}

	// Audio Pool System
	private static Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
	private static Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
	private static GameObject audioPoolParent;
	private const int POOL_SIZE = 10;

	private static void InitializeAudioPool()
	{
		if (audioPoolParent == null)
		{
			audioPoolParent = new GameObject("AudioPool");
			UnityEngine.Object.DontDestroyOnLoad(audioPoolParent);

			// Tạo pool AudioSource
			for (int i = 0; i < POOL_SIZE; i++)
			{
				GameObject audioObj = new GameObject($"AudioSource_{i}");
				audioObj.transform.SetParent(audioPoolParent.transform);
				AudioSource audioSource = audioObj.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSourcePool.Enqueue(audioSource);
			}
		}
	}

	private static AudioSource GetAudioSourceFromPool()
	{
		InitializeAudioPool();

		if (audioSourcePool.Count > 0)
		{
			return audioSourcePool.Dequeue();
		}

		// Nếu pool hết, tạo thêm
		GameObject audioObj = new GameObject($"AudioSource_Extra");
		audioObj.transform.SetParent(audioPoolParent.transform);
		AudioSource audioSource = audioObj.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		return audioSource;
	}

	private static void ReturnAudioSourceToPool(AudioSource audioSource)
	{
		if (audioSource != null)
		{
			audioSource.Stop();
			audioSource.clip = null;
			audioSourcePool.Enqueue(audioSource);
		}
	}

    private static System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, float delay)
    {
        // Dùng thời gian thực để không bị ảnh hưởng bởi Time.timeScale
        yield return new WaitForSecondsRealtime(delay + 0.1f);
        ReturnAudioSourceToPool(audioSource);
    }

	private static AudioClip GetOrCreateAudioClip(byte[] audioData)
	{
		// Tạo hash key cho audio data
		string audioHash = GetAudioHash(audioData);
		
		// Kiểm tra cache
		if (audioClipCache.ContainsKey(audioHash))
		{
			return audioClipCache[audioHash];
		}

		// Tạo AudioClip mới
		AudioClip audioClip = CreateAudioClipFromBytesOptimized(audioData);
		if (audioClip != null)
		{
			// Thêm vào cache (giới hạn cache size)
			if (audioClipCache.Count >= 50)
			{
				// Xóa cache cũ nhất
				var firstKey = audioClipCache.Keys.First();
				audioClipCache.Remove(firstKey);
			}
			audioClipCache[audioHash] = audioClip;
		}

		return audioClip;
	}

	private static string GetAudioHash(byte[] audioData)
	{
		// Tạo hash đơn giản từ 100 bytes đầu tiên
		int hash = 0;
		int sampleSize = Mathf.Min(100, audioData.Length);
		for (int i = 0; i < sampleSize; i += 4)
		{
			hash ^= BitConverter.ToInt32(audioData, i);
		}
		return hash.ToString();
	}

	/// <summary>
	/// Tạo AudioClip từ dữ liệu byte (WAV format) - Tối ưu version
	/// </summary>
	/// <param name="audioData">Dữ liệu âm thanh WAV</param>
	/// <returns>AudioClip được tạo</returns>
	private static AudioClip CreateAudioClipFromBytesOptimized(byte[] audioData)
	{
		try
		{
			// Parse WAV header để lấy thông tin
			if (audioData.Length < 44)
			{
				Debug.LogError("Audio data too short to be a valid WAV file");
				return null;
			}

			// Kiểm tra WAV header nhanh
			if (audioData[0] != 'R' || audioData[1] != 'I' || audioData[2] != 'F' || audioData[3] != 'F' ||
				audioData[8] != 'W' || audioData[9] != 'A' || audioData[10] != 'V' || audioData[11] != 'E')
			{
				Debug.LogError("Invalid WAV file format");
				return null;
			}

			// Đọc thông tin từ WAV header
			int channels = BitConverter.ToInt16(audioData, 22);
			int sampleRate = BitConverter.ToInt32(audioData, 24);
			int bitsPerSample = BitConverter.ToInt16(audioData, 34);

			// Tìm data chunk nhanh
			int dataIndex = 36;
			while (dataIndex < audioData.Length - 4)
			{
				if (audioData[dataIndex] == 'd' && audioData[dataIndex + 1] == 'a' && 
					audioData[dataIndex + 2] == 't' && audioData[dataIndex + 3] == 'a')
				{
					dataIndex += 8;
					break;
				}

				int chunkSize = BitConverter.ToInt32(audioData, dataIndex + 4);
				dataIndex += 8 + chunkSize;
			}

			if (dataIndex >= audioData.Length)
			{
				Debug.LogError("No data chunk found in WAV file");
				return null;
			}

			// Tính số samples
			int bytesPerSample = bitsPerSample / 8;
			int sampleCount = (audioData.Length - dataIndex) / (bytesPerSample * channels);

			// Tạo AudioClip
			AudioClip audioClip = AudioClip.Create("ByteAudio", sampleCount, channels, sampleRate, false);

			// Convert byte data to float array - tối ưu
			float[] samples = new float[sampleCount * channels];

			if (bitsPerSample == 16)
			{
				// Tối ưu: sử dụng unsafe code hoặc Buffer.BlockCopy nếu có thể
				for (int i = 0; i < samples.Length; i++)
				{
					int byteIndex = dataIndex + i * 2;
					if (byteIndex + 1 < audioData.Length)
					{
						short sample = (short)(audioData[byteIndex] | (audioData[byteIndex + 1] << 8));
						samples[i] = sample / 32768.0f;
					}
				}
			}
			else if (bitsPerSample == 8)
			{
				for (int i = 0; i < samples.Length; i++)
				{
					int byteIndex = dataIndex + i;
					if (byteIndex < audioData.Length)
					{
						samples[i] = (audioData[byteIndex] - 128) / 128.0f;
					}
				}
			}

			audioClip.SetData(samples, 0);
			return audioClip;
		}
		catch (Exception e)
		{
			Debug.LogError($"Error creating AudioClip from bytes: {e.Message}");
			return null;
		}
	}

	/// <summary>
	/// Tạo AudioClip từ dữ liệu byte (WAV format) - Legacy method
	/// </summary>
	/// <param name="audioData">Dữ liệu âm thanh WAV</param>
	/// <returns>AudioClip được tạo</returns>
	private static AudioClip CreateAudioClipFromBytes(byte[] audioData)
	{
		return CreateAudioClipFromBytesOptimized(audioData);
	}

}
