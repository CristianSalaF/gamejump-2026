using UnityEngine;

/// <summary>
/// Attach to the projectile prefab.
/// Requires a Trigger Collider. Movement is handled here (no Rigidbody needed).
/// Uses a LayerMask to decide what it can hit – set "Player" layer in the Inspector.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public int damage = 1;
    public float lifetime = 5f;

    [Tooltip("Layers this projectile can damage. Typically just the Player layer.")]
    public LayerMask damageLayers;

    [Header("Optional")]
    public GameObject hitEffectPrefab;

    [HideInInspector] public Vector3 direction;   // Set by RangedEnemy before firing.

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Only react to layers we care about
        if ((damageLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent<PlayerHealth>(out var ph))
            ph.TakeDamage(damage);

        SpawnHitEffect();
        Destroy(gameObject);
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;
        var fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, 2f);
    }
}