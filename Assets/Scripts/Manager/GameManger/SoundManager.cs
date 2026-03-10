using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioClipClass<T>
{
    public T MyEnum;
    public List<AudioClip> MyClip;
}


public class SoundManager : MonoBehaviour
{
    public SettingPopup SettingPopup;
    private GameManager GM => GameManager.Instance;
    private void Start()
    {
        PlayBGM(BGM.BGM);
    }
    public AudioMixer AudioMixer;

    [SerializeField][Range(0f, 1f)] private float SoundEffectPitchVariance;

    public void Initialize()
    {
     
        BGMDicts = new Dictionary<BGM, List<AudioClip>>();
        foreach (var sound in BGMs)
        {
            BGMDicts.Add(sound.MyEnum, sound.MyClip);
        }

        SFXDicts = new Dictionary<SFX, List<AudioClip>>();
        foreach (var sound in SFXs)
        {
            SFXDicts.Add(sound.MyEnum, sound.MyClip);
        }
    }


    [SerializeField] private AudioSource BGMSource;
    private Dictionary<BGM, List<AudioClip>> BGMDicts;
    [SerializeField] private AudioClipClass<BGM>[] BGMs;
    public void PlayBGM(BGM target)
    {
        int index = Random.Range(0, BGMDicts[target].Count);

        BGMSource.loop = true;
        BGMSource.clip = BGMDicts[target][index];
        BGMSource.Play();
    }

    [SerializeField] private AudioSource SFXSource;
    private Dictionary<SFX, List<AudioClip>> SFXDicts;
    [SerializeField] private AudioClipClass<SFX>[] SFXs;
    public void PlaySFX(SFX target)
    {
        int index = Random.Range(0, SFXDicts[target].Count);

        SFXSource.pitch = 1f + Random.Range(-SoundEffectPitchVariance, SoundEffectPitchVariance);
        SFXSource.PlayOneShot(SFXDicts[target][index]);
    }

    #region 家府单捞磐 包访
    public OptionData NowOptionData;

    public void SaveOptionData()
    {
        GM.SaveManager.SaveData(NowOptionData);
    }

    public bool LoadOptionData()
    {
        if (GM.SaveManager.TryLoadData(out OptionData data))
        {
            NowOptionData = data;
            return true;
        }
        else
        {
            NowOptionData.BGMVolume = 0;
            NowOptionData.SFXVolume = 0;
            return false;
        }

    }
    #endregion
}

