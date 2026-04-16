using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioClipEntry<T>
{
    public T Key;
    public List<AudioClip> Clips;
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private SettingPopup settingPopup;
    public SettingPopup SettingPopup => settingPopup;

    [SerializeField] private AudioMixer audioMixer;
    public AudioMixer AudioMixer => audioMixer;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField, Range(0f, 1f)] private float soundEffectPitchVariance = 0.1f;

    [SerializeField] private AudioClipEntry<BGM>[] bgmEntries;
    [SerializeField] private AudioClipEntry<SFX>[] sfxEntries;

    private readonly Dictionary<BGM, List<AudioClip>> _bgmDict = new();
    private readonly Dictionary<SFX, List<AudioClip>> _sfxDict = new();

    public OptionData NowOptionData;

    protected override void Awake()
    {
        // 중복 체크를 수행하고 인스펙터 데이터가 있는 인스턴스를 유지합니다.
        if (IsDuplicates()) return;
        
        base.Awake();

        // 소스가 없는 경우 자동으로 할당합니다.
        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        Initialize();
    }

    private void Start()
    {
        if (LoadOptionData())
        {
            ApplyLoadedOptions();
        }
        PlayBGM(BGM.BGM);
    }

    private void ApplyLoadedOptions()
    {
        if (audioMixer == null || NowOptionData == null) return;
        
        // 0-1 사이의 정규화된 볼륨을 데시벨(-80 ~ 20)로 변환합니다.
        float bgmDB = NowOptionData.BGMVolume <= 0.001f ? -80f : Mathf.Log10(NowOptionData.BGMVolume) * 20f;
        float sfxDB = NowOptionData.SFXVolume <= 0.001f ? -80f : Mathf.Log10(NowOptionData.SFXVolume) * 20f;

        audioMixer.SetFloat(Mixer.BGM, bgmDB);
        audioMixer.SetFloat(Mixer.SFX, sfxDB);
        
        Debug.Log($"[SoundManager] Applied volumes: BGM {bgmDB}dB, SFX {sfxDB}dB");
    }

    public void Initialize()
    {
        if (bgmEntries == null || bgmEntries.Length == 0)
        {
            //Debug.LogWarning("[SoundManager] bgmEntries is empty. Check if this is the correct instance in the scene.");
        }

        _bgmDict.Clear();
        if (bgmEntries != null)
        {
            foreach (var entry in bgmEntries)
            {
                if (!_bgmDict.ContainsKey(entry.Key))
                    _bgmDict.Add(entry.Key, entry.Clips);
            }
        }

        _sfxDict.Clear();
        if (sfxEntries != null)
        {
            foreach (var entry in sfxEntries)
            {
                if (!_sfxDict.ContainsKey(entry.Key))
                    _sfxDict.Add(entry.Key, entry.Clips);
            }
        }
        
       // Debug.Log($"[SoundManager] Initialized with {_bgmDict.Count} BGM and {_sfxDict.Count} SFX entries.");
    }

    public void PlayBGM(BGM target)
    {
        if (_bgmDict.Count == 0) Initialize();

        if (!_bgmDict.TryGetValue(target, out var clips) || clips.Count == 0)
        {
           // Debug.LogWarning($"[SoundManager] BGM clip for {target} not found.");
            return;
        }

        if (bgmSource == null) return;

        int index = Random.Range(0, clips.Count);
        bgmSource.loop = true;
        bgmSource.clip = clips[index];
        bgmSource.Play();
    }

    public void PlaySFX(SFX target)
    {
        if (_sfxDict.Count == 0) Initialize();

        if (!_sfxDict.TryGetValue(target, out var clips) || clips.Count == 0)
        {
            //Debug.LogWarning($"[SoundManager] SFX clip for {target} not found.");
            return;
        }

        if (sfxSource == null) return;

        int index = Random.Range(0, clips.Count);
        sfxSource.pitch = 1f + Random.Range(-soundEffectPitchVariance, soundEffectPitchVariance);
        sfxSource.PlayOneShot(clips[index]);
    }

    #region Option Data Management
    public void SaveOptionData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveData(NowOptionData);
        }
    }

    public bool LoadOptionData()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.TryLoadData(out OptionData data))
        {
            NowOptionData = data;
            return true;
        }
        
        // 기본값: 0.75f(약 -2.5dB)는 적절한 시작 지점입니다.
        NowOptionData = new OptionData { BGMVolume = 0.75f, SFXVolume = 0.75f };
        return false;
    }
    #endregion
}
