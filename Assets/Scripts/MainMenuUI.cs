using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador del Menú Principal.
///
/// Jerarquía UI sugerida:
///   Canvas
///     MainPanel
///         Button_Play
///         Button_Settings
///         Button_Quit
///     SettingsPanel
///         Slider_Master
///         Slider_Music
///         Slider_SFX
///         Button_Back
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Settings – Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;


    void Start()
    {
        ShowMainPanel();
        InitSliders();
    }


    public void OnPlayButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame();
        else
            Debug.LogWarning("GameManager no encontrado en la escena.");
    }

    public void OnSettingsButton()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnQuitButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }


    public void OnBackButton()
    {
        ShowMainPanel();
    }


    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }


    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    /// <summary>Sincroniza sliders con los valores guardados en AudioManager.</summary>
    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        if (masterSlider != null)
        {
            masterSlider.value = AudioManager.Instance.GetMasterVolume();
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }
}