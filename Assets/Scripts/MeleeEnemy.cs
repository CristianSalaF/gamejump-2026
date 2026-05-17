using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int contactDamage = 1;
    public float damageCooldown = 1f;   // seconds between damage ticks while in contact

    [Header("Movement")]
    public float detectionRange = 15f;

    // ---- private ----
    NavMeshAgent agent;
    Transform player;
    int currentHealth;
    float lastDamageTime = -Mathf.Infinity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;

        // Player GameObject must be tagged "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRange)
            agent.SetDestination(player.position);
        else
            agent.ResetPath();
    }

    // -------------------------------------------------- //
    // Receive damage (called by player attack logic)      //
    // -------------------------------------------------- //
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(currentHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // -------------------------------------------------- //
    // Deal damage on contact with the player             //
    // CharacterController doesn't fire OnCollision �     //
    // set the enemy collider to "Is Trigger" instead.   //
    // -------------------------------------------------- //
    void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    void TryDamagePlayer(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;

        if (other.TryGetComponent<PlayerHealth>(out PlayerHealth ph))
            ph.TakeDamage(contactDamage);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}