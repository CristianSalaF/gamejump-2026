using UnityEngine;

/// <summary>
/// Single Responsibility: handles only first-person movement and camera look.
///
/// FIX (Shared Input): no longer creates its own InputSystem_Actions. The
/// bootstrapper injects the single shared instance via Initialize(). This
/// eliminates the dual-input-instance conflict that caused missed or doubled inputs.
///
/// FIX (Hierarchy): vertical look rotates cameraHolder (the empty GameObject
/// that is the direct parent of Main Camera, Sword, and Shield). This keeps
/// the weapon and shield locked to the camera view correctly, because they are
/// children of cameraHolder and therefore move with it.
///
/// Ground detection uses a bottom-sphere overlap rather than
/// CharacterController.isGrounded, which is unreliable on flat surfaces.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Ground Detection")]
    [Tooltip("Assign the GroundCheck empty GameObject at the capsule bottom, or leave null to auto-create.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Match to roughly half the CharacterController radius (~0.25).")]
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    [Header("Look")]
    [Tooltip("Assign the CameraHolder empty GameObject (parent of Main Camera, Sword, Shield).")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float verticalClampDeg = 85f;

    private CharacterController _controller;

    // Injected by bootstrapper — shared with all other handlers
    private InputSystem_Actions _input;

    private Vector3 _velocity;
    private float _verticalRotation;
    private bool _isGrounded;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // Auto-create a ground-check point if one wasn't assigned in the Inspector
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            float bottom = _controller.center.y - (_controller.height / 2f);
            gc.transform.localPosition = new Vector3(0f, bottom, 0f);
            groundCheck = gc.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Called by PlayerCombatBootstrapper with the single shared input instance.
    /// Must be called before the first Update tick — Awake order is guaranteed
    /// because the bootstrapper also runs in Awake and calls this immediately.
    /// </summary>
    public void Initialize(InputSystem_Actions input)
    {
        _input = input;
        // The bootstrapper already called _input.Player.Enable(); no need to repeat it.
    }

    private void Update()
    {
        if (_input == null) return;   // safety guard before Initialize is called

        CheckGround();
        HandleGravityAndJump();
        HandleMovement();
        HandleLook();
    }

    // ── Ground detection ──────────────────────────────────────────────────────

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        _controller.Move(move * (moveSpeed * Time.deltaTime));
    }

    private void HandleGravityAndJump()
    {
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;  // small negative keeps sphere touching ground next frame

        if (_input.Player.Jump.WasPressedThisFrame() && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    // ── Camera look ───────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();

        // Horizontal: rotate the Player body — this keeps movement direction in sync
        transform.Rotate(Vector3.up, lookInput.x * mouseSensitivity);

        // Vertical: rotate the CameraHolder (which parents Camera + Sword + Shield)
        // so weapons stay locked to the camera view at all vertical angles
        _verticalRotation -= lookInput.y * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalClampDeg, verticalClampDeg);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
