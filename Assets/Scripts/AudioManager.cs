using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton que gestiona el volumen global.
/// Requiere un AudioMixer con grupos expuestos:
///   "MasterVolume", "MusicVolume", "SFXVolume"
/// Los valores se persisten con PlayerPrefs.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    // Claves para el mixer (deben coincidir con los parámetros expuestos)
    private const string MasterParam = "MasterVolume";
    private const string MusicParam = "MusicVolume";
    private const string SFXParam = "SFXVolume";

    // Claves para PlayerPrefs
    private const string MasterKey = "Vol_Master";
    private const string MusicKey = "Vol_Music";
    private const string SFXKey = "Vol_SFX";


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Cargar volúmenes guardados (o usar 1 por defecto)
        SetMasterVolume(PlayerPrefs.GetFloat(MasterKey, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicKey, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXKey, 1f));
    }


    /// <param name="value">0 – 1</param>
    public void SetMasterVolume(float value)
    {
        ApplyVolume(MasterParam, value);
        PlayerPrefs.SetFloat(MasterKey, value);
    }

    /// <param name="value">0 – 1</param>
    public void SetMusicVolume(float value)
    {
        ApplyVolume(MusicParam, value);
        PlayerPrefs.SetFloat(MusicKey, value);
    }

    /// <param name="value">0 – 1</param>
    public void SetSFXVolume(float value)
    {
        ApplyVolume(SFXParam, value);
        PlayerPrefs.SetFloat(SFXKey, value);
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat(MasterKey, 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat(MusicKey, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFXKey, 1f);


    // Convierte rango lineal [0,1] a decibelios [-80, 0]
    private void ApplyVolume(string param, float linearValue)
    {
        if (audioMixer == null) return;

        float db = linearValue > 0.0001f
            ? Mathf.Log10(linearValue) * 20f
            : -80f;

        audioMixer.SetFloat(param, db);
    }
}