using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida flotante en world-space que se coloca encima de un enemigo.
///
/// SETUP:
/// 1. Crea un Canvas (World Space) hijo del enemigo.
///    - Sort Order: 10 (para que aparezca sobre geometría)
///    - Scale: 0.01 en X, Y, Z
/// 2. Dentro del Canvas añade un Slider (modo Image Fill):
///    - Background: color rojo/gris
///    - Fill: color verde -> rojo según porcentaje
///    - Handle: desactivado
/// 3. Arrastra este script al Canvas (o al enemigo) y asigna los campos.
///
/// Este componente se puede inicializar desde MeleeEnemy / RangedEnemy.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Slider que representa los puntos de vida")]
    public Slider healthSlider;

    [Tooltip("Imagen de relleno cuyo color cambia según el %")]
    public Image fillImage;

    [Header("Colores")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Billboard")]
    [Tooltip("Si es true, la barra siempre mira a la cámara principal")]
    public bool faceCamera = true;

    [Header("Ocultación")]
    [Tooltip("Ocultar la barra cuando el enemigo tiene vida máxima")]
    public bool hideAtFullHealth = true;

    private Camera _mainCamera;
    private Canvas _canvas;

    void Awake()
    {
        _mainCamera = Camera.main;
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
            _canvas = GetComponentInChildren<Canvas>();
    }

    void LateUpdate()
    {
        if (faceCamera && _mainCamera != null)
            transform.forward = _mainCamera.transform.forward;
    }


    /// <summary>
    /// Inicializa la barra con los valores máximos.
    /// Llama esto desde Awake/Start del enemigo.
    /// </summary>
    public void Init(int maxHealth)
    {
        if (healthSlider == null) return;
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        UpdateVisuals(maxHealth, maxHealth);
    }

    /// <summary>
    /// Actualiza la barra de vida. Llama esto cada vez que el enemigo recibe daño.
    /// </summary>
    public void SetHealth(int current, int max)
    {
        if (healthSlider == null) return;
        healthSlider.value = Mathf.Clamp(current, 0, max);
        UpdateVisuals(current, max);
    }


    private void UpdateVisuals(int current, int max)
    {
        float pct = max > 0 ? (float)current / max : 0f;

        // Cambiar color según porcentaje
        if (fillImage != null)
        {
            if (pct > 0.5f)
                fillImage.color = Color.Lerp(halfHealthColor, fullHealthColor, (pct - 0.5f) * 2f);
            else
                fillImage.color = Color.Lerp(lowHealthColor, halfHealthColor, pct * 2f);
        }

        // Ocultar si vida completa
        if (hideAtFullHealth && _canvas != null)
            _canvas.enabled = pct < 1f;
    }
}