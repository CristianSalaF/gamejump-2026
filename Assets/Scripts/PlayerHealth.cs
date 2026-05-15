using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Single Responsibility: tracks player health and reacts to damage.
///
/// FIX (DIP): now depends on the IBlocker interface rather than the concrete
/// BlockHandler class. PlayerHealth has no reason to know how blocking is
/// implemented — it only needs to ask "is the player blocking, and by how much?"
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;   // passes current health
    public UnityEvent OnDeath;

    // Injected by bootstrapper — depends on interface, not concrete type (DIP)
    private IBlocker _blocker;

    private float _currentHealth;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    /// <summary>Inject the blocker abstraction so health can query reduction.</summary>
    public void Initialize(IBlocker blocker)
    {
        _blocker = blocker;
    }

    // ── IDamageable ───────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        float finalDamage = CalculateDamage(amount);

        _currentHealth -= finalDamage;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0f)
            OnDeath?.Invoke();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private float CalculateDamage(float rawDamage)
    {
        if (_blocker != null && _blocker.IsBlocking)
            return rawDamage * (1f - _blocker.DamageReductionPercent);

        return rawDamage;
    }

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);
    }
}
