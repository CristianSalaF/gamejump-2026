using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Shared interface – both enemy types implement this so the player
// doesn't need to know which kind it is hitting.
public interface IDamageable
{
    void TakeDamage(int amount);
}

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;

    CharacterController controller;
    Animator animator;

    // ── Two dedicated AudioSources ─────────────────────────────────────────
    // Assign both in the Inspector and set their Output to the SFX mixer group.
    [Header("Audio")]
    [Tooltip("AudioSource used for sword swing sounds. Output → SFX mixer group.")]
    public AudioSource sfxSource;       // looping-safe, one-shot SFX
    [Tooltip("AudioSource used for hit sounds. Output → SFX mixer group.")]
    public AudioSource hitSource;       // separate source so swing + hit can overlap

    [Header("Controller")]
    public float moveSpeed = 5;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;

    Vector3 _PlayerVelocity;
    bool isGrounded;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;

    float xRotation = 0f;

    [Header("Movement Lock")]
    public bool movementLocked = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (input.Attack.IsPressed())
            Attack();

        SetAnimations();
    }

    void FixedUpdate()
    {
        if (!movementLocked)
            MoveInput(input.Movement.ReadValue<Vector2>());
        else
            ApplyGravityOnly();
    }

    void LateUpdate()
    {
        LookInput(input.Look.ReadValue<Vector2>());
    }

    void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        controller.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;
        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void ApplyGravityOnly()
    {
        _PlayerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;
        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable() { input.Enable(); }
    void OnDisable() { input.Disable(); }

    void Jump()
    {
        if (movementLocked) return;
        if (isGrounded)
            _PlayerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }

    void AssignInputs()
    {
        input.Jump.performed += ctx => Jump();
        input.Attack.started += ctx => Attack();
    }

    public void LockMovement() => movementLocked = true;
    public void UnlockMovement() => movementLocked = false;

    // ── Animations ─────────────────────────────────────────────────────────

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        if (!attacking)
        {
            if (_PlayerVelocity.x == 0 && _PlayerVelocity.z == 0)
                ChangeAnimationState(IDLE);
            else
                ChangeAnimationState(WALK);
        }
    }

    // ── Attacking ──────────────────────────────────────────────────────────

    [Header("Attacking")]
    public float attackDistance = 2f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    [Header("Attack Sounds")]
    public AudioClip swordSwing;  // played immediately when attack starts
    public AudioClip hitSound;    // played when the raycast connects

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        movementLocked = true;
        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        // Sword swing – slight pitch randomisation for variety
        if (sfxSource != null && swordSwing != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(swordSwing);
        }

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
        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                            out RaycastHit hit, attackDistance, attackLayer))
        {
            // Hit sound through its own source so it can overlap the swing
            if (hitSource != null && hitSound != null)
            {
                hitSource.pitch = 1f;
                hitSource.PlayOneShot(hitSound);
            }

            if (hit.transform.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(attackDamage);
                Debug.Log(target + " attacked");
            }
        }
    }
}