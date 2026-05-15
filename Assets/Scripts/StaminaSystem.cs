using UnityEngine;

/// <summary>
/// Single Responsibility: manages only stamina state and regeneration.
/// Open/Closed: extend by subclassing or injecting different regen strategies.
/// </summary>
public class StaminaSystem : MonoBehaviour, IStaminaSystem
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRatePerSecond = 10f;
    [SerializeField] private float regenDelayAfterUse = 1.5f;

    private float _currentStamina;
    private float _regenCooldownTimer;

    public float CurrentStamina => _currentStamina;
    public float MaxStamina => maxStamina;
    public bool HasStamina => _currentStamina > 0f;

    private void Awake()
    {
        _currentStamina = maxStamina;
    }

    private void Update()
    {
        HandleRegeneration();
    }

    /// <summary>
    /// Attempts to consume stamina. Returns false and does nothing if insufficient.
    /// </summary>
    public bool TryConsume(float amount)
    {
        if (_currentStamina < amount) return false;

        _currentStamina -= amount;
        _currentStamina = Mathf.Max(_currentStamina, 0f);
        _regenCooldownTimer = regenDelayAfterUse;
        return true;
    }

    public void Regenerate(float amount)
    {
        _currentStamina = Mathf.Min(_currentStamina + amount, maxStamina);
    }

    private void HandleRegeneration()
    {
        if (_regenCooldownTimer > 0f)
        {
            _regenCooldownTimer -= Time.deltaTime;
            return;
        }

        if (_currentStamina < maxStamina)
            Regenerate(regenRatePerSecond * Time.deltaTime);
    }
}
