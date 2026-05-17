using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the world-space health bar image above an enemy.
/// Called by EnemyHealth.TakeDamage – no polling needed.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The fill Image whose fillAmount represents current HP.")]
    public Image fillImage;

    [Header("Colours")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Behaviour")]
    public bool faceCamera = true;
    public bool hideAtFullHealth = true;

    Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;

        // Start hidden if configured
        if (hideAtFullHealth && fillImage != null)
            fillImage.transform.parent.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (faceCamera && mainCam != null)
            transform.forward = mainCam.transform.forward;
    }

    /// <summary>Called by EnemyHealth whenever damage is dealt.</summary>
    public void UpdateBar(int current, int max)
    {
        if (fillImage == null) return;

        float fraction = Mathf.Clamp01((float)current / max);
        fillImage.fillAmount = fraction;
        fillImage.color = Color.Lerp(lowHealthColor,
                                     fraction > 0.5f ? fullHealthColor : halfHealthColor,
                                     fraction > 0.5f ? (fraction - 0.5f) * 2f : fraction * 2f);

        // Show bar the moment damage is taken
        if (hideAtFullHealth)
            fillImage.transform.parent.gameObject.SetActive(fraction < 1f);
    }
}