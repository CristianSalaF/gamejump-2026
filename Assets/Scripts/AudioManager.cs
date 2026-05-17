using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton AudioManager.
/// - Exposes SetMasterVolume / SetMusicVolume / SetSFXVolume
///   (called by the UI sliders – values come in as 0-1 from the slider).
/// - Plays background music through two dedicated AudioSources so you can
///   cross-fade or hard-switch between tracks.
/// - Persists across scenes (DontDestroyOnLoad).
///
/// SETUP IN INSPECTOR
/// ------------------
/// 1. Assign the GameMixer AudioMixer.
/// 2. In the mixer, expose the Volume parameters and name them EXACTLY:
///      "MasterVolume", "MusicVolume", "SFXVolume"
///    (right-click the volume knob -> Expose -> rename in the Exposed Parameters list).
/// 3. Assign musicSourceA and musicSourceB (two AudioSource components on child
///    GameObjects; set their Output to the Music mixer group).
/// 4. Assign mainMenuMusic and gameplayMusic clips.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Mixer Parameter Names")]
    public string masterParam = "MasterVolume";
    public string musicParam = "MusicVolume";
    public string sfxParam = "SFXVolume";

    [Header("Music Sources (assign two child AudioSources)")]
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float defaultMasterVolume = 1f;
    [Range(0f, 1f)] public float defaultMusicVolume = 1f;
    [Range(0f, 1f)] public float defaultSFXVolume = 1f;

    private AudioSource _activeSource;
    private AudioSource _inactiveSource;

    // PlayerPrefs keys  update the sliders on any scene that has the UI
    private const string PREF_MASTER = "Vol_Master";
    private const string PREF_MUSIC = "Vol_Music";
    private const string PREF_SFX = "Vol_SFX";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _activeSource = musicSourceA;
        _inactiveSource = musicSourceB;

        LoadAndApplyVolumes();
    }

    
    /// <summary>Call from the Master slider's OnValueChanged (value 0-1).</summary>
    public void SetMasterVolume(float value)
    {
        SetMixerVolume(masterParam, value);
        PlayerPrefs.SetFloat(PREF_MASTER, value);
    }

    /// <summary>Call from the Music slider's OnValueChanged (value 0-1).</summary>
    public void SetMusicVolume(float value)
    {
        SetMixerVolume(musicParam, value);
        PlayerPrefs.SetFloat(PREF_MUSIC, value);
    }

    /// <summary>Call from the SFX slider's OnValueChanged (value 0-1).</summary>
    public void SetSFXVolume(float value)
    {
        SetMixerVolume(sfxParam, value);
        PlayerPrefs.SetFloat(PREF_SFX, value);
    }


    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);

    /// <summary>
    /// Hard-switches to the given clip on the active source.
    /// Add cross-fade here later if needed.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (_activeSource.clip == clip && _activeSource.isPlaying) return;

        _activeSource.clip = clip;
        _activeSource.loop = true;
        _activeSource.Play();
    }

    public void StopMusic() => _activeSource.Stop();


    /// <summary>
    /// Converts a 0-1 linear slider value to decibels and pushes it to the mixer.
    /// Treats values below 0.001 as silence (-80 dB).
    /// </summary>
    private void SetMixerVolume(string paramName, float linearValue)
    {
        float dB = linearValue > 0.001f
            ? Mathf.Log10(linearValue) * 20f
            : -80f;

        audioMixer.SetFloat(paramName, dB);
    }

    private void LoadAndApplyVolumes()
    {
        float master = PlayerPrefs.GetFloat(PREF_MASTER, defaultMasterVolume);
        float music = PlayerPrefs.GetFloat(PREF_MUSIC, defaultMusicVolume);
        float sfx = PlayerPrefs.GetFloat(PREF_SFX, defaultSFXVolume);

        SetMixerVolume(masterParam, master);
        SetMixerVolume(musicParam, music);
        SetMixerVolume(sfxParam, sfx);
    }

    /// <summary>
    /// Returns the saved linear volume (0-1) for a given pref key.
    /// Used by UI scripts to initialise sliders without creating a circular call.
    /// </summary>
    public float GetSavedMasterVolume() => PlayerPrefs.GetFloat(PREF_MASTER, defaultMasterVolume);
    public float GetSavedMusicVolume() => PlayerPrefs.GetFloat(PREF_MUSIC, defaultMusicVolume);
    public float GetSavedSFXVolume() => PlayerPrefs.GetFloat(PREF_SFX, defaultSFXVolume);
}