using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa en la escena de juego.
/// Se activa/desactiva con la tecla Escape (acción "Pause" en PlayerInput).
///
/// Jerarquía UI sugerida (en la escena Game):
///   Canvas (Screen Space – Overlay)
///     PausePanel (empieza desactivado)
///         Button_Resume
///         Button_Settings  <- abre el subpanel de settings
///         Button_MainMenu
///     SettingsSubPanel (empieza desactivado)
///         Slider_Master
///         Slider_Music
///         Slider_SFX
///         Button_BackSettings
///         Button_Quit
///
/// Requiere que PlayerInput tenga la acción "Pause" con binding <Keyboard>/escape.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsSubPanel;

    [Header("Settings – Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    // Referencia al PlayerInput generado (Assets/Settings/Input/PlayerInput.cs)
    private PlayerInput _playerInput;


    void Awake()
    {
        _playerInput = new PlayerInput();
    }

    void OnEnable()
    {
        _playerInput.Main.Enable();
        _playerInput.Main.Pause.performed += OnPausePerformed;
    }

    void OnDisable()
    {
        _playerInput.Main.Pause.performed -= OnPausePerformed;
        _playerInput.Main.Disable();
    }

    void Start()
    {
        pausePanel.SetActive(false);
        settingsSubPanel.SetActive(false);
        InitSliders();
    }


    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // Si el subpanel de settings está abierto, ciérralo primero
        if (settingsSubPanel.activeSelf)
        {
            CloseSettings();
            return;
        }
        TogglePause();
    }


    public void OnResumeButton() => ClosePauseMenu();
    public void OnMainMenuButton() => GameManager.Instance?.LoadMainMenu();
    public void OnQuitButton() => GameManager.Instance?.QuitGame();

    public void OnSettingsButton()
    {
        settingsSubPanel.SetActive(true);
        SyncSliders();
    }

    public void OnBackSettingsButton() => CloseSettings();


    public void OnMasterVolumeChanged(float value) => AudioManager.Instance?.SetMasterVolume(value);
    public void OnMusicVolumeChanged(float value) => AudioManager.Instance?.SetMusicVolume(value);
    public void OnSFXVolumeChanged(float value) => AudioManager.Instance?.SetSFXVolume(value);


    private void TogglePause()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsPaused)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        GameManager.Instance?.PauseGame();
        pausePanel.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        GameManager.Instance?.ResumeGame();
        pausePanel.SetActive(false);
        settingsSubPanel.SetActive(false);
    }

    private void CloseSettings()
    {
        settingsSubPanel.SetActive(false);
    }

    /// <summary>Registra listeners y carga valores guardados la primera vez.</summary>
    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        SyncSliders();
    }

    /// <summary>Actualiza los sliders con los valores actuales de AudioManager.</summary>
    private void SyncSliders()
    {
        if (AudioManager.Instance == null) return;

        if (masterSlider != null) masterSlider.value = AudioManager.Instance.GetMasterVolume();
        if (musicSlider != null) musicSlider.value = AudioManager.Instance.GetMusicVolume();
        if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.GetSFXVolume();
    }
}