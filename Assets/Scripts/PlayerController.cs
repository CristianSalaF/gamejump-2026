using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shared interface – implemented by EnemyHealth so the player raycast
/// works on any enemy type without knowing the concrete class.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}

public class PlayerController : MonoBehaviour
{
    // ── Input ──────────────────────────────────────────────────────────────
    PlayerInput playerInput;
    PlayerInput.MainActions input;

    // ── Components ─────────────────────────────────────────────────────────
    CharacterController controller;
    Animator animator;

    // ── Audio ──────────────────────────────────────────────────────────────
    // Both sources should be children of the Player and their Output set to
    // the GameMixer → SFX group.
    [Header("Audio")]
    [Tooltip("Plays sword swing and hit sounds. Output → SFX mixer group.")]
    public AudioSource sfxSource;
    [Tooltip("Separate source so hit and swing can overlap.")]
    public AudioSource hitSource;

    [Header("Attack Sounds")]
    public AudioClip swordSwing;
    public AudioClip hitSound;

    // ── Movement ───────────────────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;

    Vector3 playerVelocity;
    bool isGrounded;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity = 15f;

    float xRotation;

    [Header("Movement Lock")]
    public bool movementLocked;

    // ── Attack ─────────────────────────────────────────────────────────────
    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    bool attacking;
    bool readyToAttack = true;
    int attackCount;

    // ── Animation state names ──────────────────────────────────────────────
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string ATTACK1 = "Attack 1";
    const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        playerInput = new PlayerInput();
        input = playerInput.Main;

        input.Jump.performed += _ => Jump();
        input.Attack.started += _ => Attack();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        isGrounded = controller.isGrounded;
        SetAnimations();
    }

    void FixedUpdate()
    {
        if (!movementLocked)
            Move(input.Movement.ReadValue<Vector2>());
        else
            ApplyGravityOnly();
    }

    void LateUpdate()
    {
        Look(input.Look.ReadValue<Vector2>());
    }

    // ── Movement ───────────────────────────────────────────────────────────
    void Move(Vector2 rawInput)
    {
        Vector3 dir = new Vector3(rawInput.x, 0f, rawInput.y);
        controller.Move(transform.TransformDirection(dir) * moveSpeed * Time.deltaTime);
        ApplyGravityOnly();
    }

    void ApplyGravityOnly()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Jump()
    {
        if (movementLocked || !isGrounded) return;
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
    }

    void Look(Vector2 rawInput)
    {
        xRotation -= rawInput.y * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * rawInput.x * sensitivity * Time.deltaTime);
    }

    public void LockMovement() => movementLocked = true;
    public void UnlockMovement() => movementLocked = false;

    // ── Animations ─────────────────────────────────────────────────────────
    void SetAnimations()
    {
        if (attacking) return;

        // Use horizontal velocity only (y is gravity, not locomotion)
        Vector2 flat = new Vector2(playerVelocity.x, playerVelocity.z);
        ChangeAnimationState(flat.sqrMagnitude > 0.01f ? WALK : IDLE);
    }

    void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(newState, 0.2f);
    }

    // ── Attack ─────────────────────────────────────────────────────────────
    void Attack()
    {
        if (!readyToAttack || attacking) return;

        movementLocked = true;
        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        PlayOneShot(sfxSource, swordSwing, Random.Range(0.9f, 1.1f));

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        movementLocked = false;
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (!Physics.Raycast(cam.transform.position,
                             cam.transform.forward,
                             out RaycastHit hit,
                             attackDistance,
                             attackLayer))
            return;

        PlayOneShot(hitSource, hitSound, 1f);

        if (hit.transform.TryGetComponent<IDamageable>(out var target))
            target.TakeDamage(attackDamage);
    }

    // ── Audio helpers ──────────────────────────────────────────────────────
    static void PlayOneShot(AudioSource source, AudioClip clip, float pitch)
    {
        if (source == null || clip == null) return;
        source.pitch = pitch;
        source.PlayOneShot(clip);
    }
}