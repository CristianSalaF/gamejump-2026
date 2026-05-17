using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu canvas.
/// Sliders call AudioManager directly so you never have to touch this class
/// when adding new mixer groups – just add a new slider and wire its
/// OnValueChanged to the matching AudioManager.SetXxxVolume method.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Start main-menu music (safe if AudioManager already playing it)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuMusic();

        // Initialise sliders from saved prefs without triggering OnValueChanged
        InitSliders();

        // Wire slider callbacks
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // Make sure only the main panel is visible
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }

    private void OnMasterChanged(float v) => AudioManager.Instance?.SetMasterVolume(v);
    private void OnMusicChanged(float v) => AudioManager.Instance?.SetMusicVolume(v);
    private void OnSFXChanged(float v) => AudioManager.Instance?.SetSFXVolume(v);

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnSettingsButton()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        // Temporarily remove listeners so setting .value doesn't fire callbacks
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        masterSlider.value = AudioManager.Instance.GetSavedMasterVolume();
        musicSlider.value = AudioManager.Instance.GetSavedMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSavedSFXVolume();
    }
}