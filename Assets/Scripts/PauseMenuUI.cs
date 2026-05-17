using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controls the in-game pause menu.
/// Sliders delegate to AudioManager – add more sliders the same way
/// without touching any other class.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsSubPanel;

    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private PlayerInput _input;
    private bool _isPaused = false;

    void Awake()
    {
        _input = new PlayerInput();
    }

    void Start()
    {
        // Initialise sliders from saved prefs (no circular callbacks)
        InitSliders();

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    void OnEnable()
    {
        _input.Main.Pause.performed += OnPausePressed;
        _input.Main.Enable();
    }

    void OnDisable()
    {
        _input.Main.Pause.performed -= OnPausePressed;
        _input.Main.Disable();
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

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (_isPaused) OnResumeButton();
        else OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        _isPaused = true;
        pausePanel.SetActive(true);
        settingsSubPanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnResumeButton()
    {
        _isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnSettingsButton() => settingsSubPanel.SetActive(true);
    public void OnBackSettingsButton() => settingsSubPanel.SetActive(false);

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        masterSlider.value = AudioManager.Instance.GetSavedMasterVolume();
        musicSlider.value = AudioManager.Instance.GetSavedMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSavedSFXVolume();
    }
}