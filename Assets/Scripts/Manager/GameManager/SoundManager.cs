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

    private GameManager GM => GameManager.Instance;

    public OptionData NowOptionData;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {
        PlayBGM(BGM.BGM);
    }

    public void Initialize()
    {
        _bgmDict.Clear();
        foreach (var entry in bgmEntries)
        {
            if (!_bgmDict.ContainsKey(entry.Key))
                _bgmDict.Add(entry.Key, entry.Clips);
        }

        _sfxDict.Clear();
        foreach (var entry in sfxEntries)
        {
            if (!_sfxDict.ContainsKey(entry.Key))
                _sfxDict.Add(entry.Key, entry.Clips);
        }
    }

    public void PlayBGM(BGM target)
    {
        if (!_bgmDict.TryGetValue(target, out var clips) || clips.Count == 0) return;

        int index = Random.Range(0, clips.Count);
        bgmSource.loop = true;
        bgmSource.clip = clips[index];
        bgmSource.Play();
    }

    public void PlaySFX(SFX target)
    {
        if (!_sfxDict.TryGetValue(target, out var clips) || clips.Count == 0) return;

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
        
        NowOptionData = new OptionData { BGMVolume = 0, SFXVolume = 0 };
        return false;
    }
    #endregion
}
