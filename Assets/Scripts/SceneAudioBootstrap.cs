using UnityEngine;

/// <summary>
/// Drop one of these on a GameObject in each scene.
/// It calls AudioManager to switch to the correct music track
/// when the scene loads, without you having to wire anything else.
/// </summary>
public class SceneAudioBootstrap : MonoBehaviour
{
    public enum SceneTrack { MainMenu, Gameplay, None }

    [Tooltip("Which music track should play in this scene?")]
    public SceneTrack track = SceneTrack.Gameplay;

    void Start()
    {
        if (AudioManager.Instance == null) return;

        switch (track)
        {
            case SceneTrack.MainMenu:
                AudioManager.Instance.PlayMainMenuMusic();
                break;
            case SceneTrack.Gameplay:
                AudioManager.Instance.PlayGameplayMusic();
                break;
            case SceneTrack.None:
                AudioManager.Instance.StopMusic();
                break;
        }
    }
}