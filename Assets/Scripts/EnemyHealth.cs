using UnityEngine;

/// <summary>
/// Single source of truth for enemy hit points.
/// Attach this to every enemy. MeleeEnemy / RangedEnemy reference it for death.
/// Implements IDamageable so the player raycast can hit any enemy type uniformly.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 3;

    [Header("Optional")]
    public GameObject deathEffectPrefab;

    int currentHealth;

    // Optional reference – filled automatically if a health-bar component exists.
    EnemyHealthBar healthBar;

    void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponent<EnemyHealthBar>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        healthBar?.UpdateBar(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (deathEffectPrefab != null)
        {
            var fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }
}