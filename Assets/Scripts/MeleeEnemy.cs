using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class MeleeEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;

    [Header("Movement")]
    public float detectionRange = 15f;

    NavMeshAgent agent;
    Transform player;
    float lastDamageTime = -Mathf.Infinity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRange)
            agent.SetDestination(player.position);
        else
            agent.ResetPath();
    }

    void OnTriggerEnter(Collider other) => TryDamagePlayer(other.gameObject);
    void OnTriggerStay(Collider other) => TryDamagePlayer(other.gameObject);

    void TryDamagePlayer(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;

        if (other.TryGetComponent<PlayerHealth>(out var ph))
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