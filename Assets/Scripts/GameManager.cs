using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que gestiona el estado global del juego: escenas, pausa y tiempo.
/// Colócalo en un GameObject persistente en la escena del Menú Principal.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [Tooltip("Nombre exacto de la escena del menú principal en Build Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Nombre exacto de la escena del juego en Build Settings")]
    public string gameSceneName = "Main";

    public bool IsPaused { get; private set; }


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


    public void LoadGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }
}