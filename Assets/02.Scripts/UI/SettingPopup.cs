using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    private SoundManager GM => SoundManager.Instance;
    private AudioMixer AudioMixer => SoundManager.Instance != null ? SoundManager.Instance.AudioMixer : null;

    [Header("Sound")]
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;

    private void Awake()
    {
        BGMSlider.onValueChanged.AddListener(ChangeBGMVolume);
        SFXSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    private void OnEnable()
    {
        if (AudioMixer == null)
        {
            Debug.LogWarning("[SettingPopup] AudioMixer is null. Volume sliders won't be initialized.");
            return;
        }

        if (BGMSlider != null && AudioMixer.GetFloat(Mixer.BGM, out float BGMVolume))
        {
            BGMSlider.value = Mathf.Pow(10, (BGMVolume / 20));
        }

        if (SFXSlider != null && AudioMixer.GetFloat(Mixer.SFX, out float SFXVolume))
        {
            SFXSlider.value = Mathf.Pow(10, (SFXVolume / 20));
        }
    }

    private void OnDisable()
    {
        if (AudioMixer == null || GM == null) return;

        if (AudioMixer.GetFloat(Mixer.BGM, out float BGMVolume))
            GM.NowOptionData.BGMVolume = BGMVolume;

        if (AudioMixer.GetFloat(Mixer.SFX, out float SFXVolume))
            GM.NowOptionData.SFXVolume = SFXVolume;

        GM.SaveOptionData();
    }

    public void Initializer()
    {
        if (GM == null || AudioMixer == null) return;

        GM.LoadOptionData();
        AudioMixer.SetFloat(Mixer.BGM, GM.NowOptionData.BGMVolume);
        AudioMixer.SetFloat(Mixer.SFX, GM.NowOptionData.SFXVolume);       
    }

    public void ChangeBGMVolume(float volume)
    {
        if (AudioMixer == null) return;

        if (volume == 0)
        {
            AudioMixer.SetFloat(Mixer.BGM, -80f);
        }
        else
        {
            AudioMixer.SetFloat(Mixer.BGM, Mathf.Log10(volume) * 20);
        }
    }

    public void ChangeSFXVolume(float volume)
    {
        if (AudioMixer == null) return;

        if (volume == 0)
        {
            AudioMixer.SetFloat(Mixer.SFX, -80f);
        }
        else
        {
            AudioMixer.SetFloat(Mixer.SFX, Mathf.Log10(volume) * 20);
        }
    }

    #region 설정 창 활성화 토글

    public void Toggle()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            SoundManager.Instance?.PlaySFX(SFX.Click);
        }
        else
        {
            gameObject.SetActive(true);
            SoundManager.Instance?.PlaySFX(SFX.Click);
        }
    }

    /// <summary>
    /// 버튼 클릭 시 전체 게임 데이터를 초기화합니다.
    /// </summary>
    public void OnClickResetButton()
    {
        if (PlayerDataManager.Instance != null)
        {
            // 전체 데이터 파괴 및 초기화 (이벤트 버스를 통해 타 시스템에 알림)
            PlayerDataManager.Instance.DestroyData();
            
            // 클릭 효과음 재생
            SoundManager.Instance?.PlaySFX(SFX.Click);

            Debug.Log("[SettingPopup] All game data has been reset via UI button.");
        }
        else
        {
            Debug.LogError("[SettingPopup] PlayerDataManager instance not found!");
        }
    }

    #endregion
}
