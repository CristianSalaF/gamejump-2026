using UnityEngine;

/// <summary>
/// Componente de salud genérico para cualquier enemigo.
/// Conecta automáticamente con EnemyHealthBar si está presente en el mismo GameObject
/// o en alguno de sus hijos.
///
/// INTEGRACIÓN con MeleeEnemy / RangedEnemy:
///   • Añade este componente al prefab del enemigo.
///   • En MeleeEnemy.TakeDamage (si lo tienes), llama: GetComponent<EnemyHealth>().TakeDamage(dmg);
///   • O deja que los proyectiles llamen directamente a este componente:
///       if (other.TryGetComponent<EnemyHealth>(out var eh)) eh.TakeDamage(damage);
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración")]
    public int maxHealth = 3;

    [Header("Efectos opcionales")]
    public GameObject deathEffectPrefab;

    // Estado interno
    private int _currentHealth;
    private EnemyHealthBar _healthBar;


    void Awake()
    {
        _currentHealth = maxHealth;
        _healthBar = GetComponentInChildren<EnemyHealthBar>(includeInactive: true);
    }

    void Start()
    {
        _healthBar?.Init(maxHealth);
    }


    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        _healthBar?.SetHealth(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        _healthBar?.SetHealth(_currentHealth, maxHealth);
    }


    private void Die()
    {
        if (deathEffectPrefab != null)
        {
            var fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }
}