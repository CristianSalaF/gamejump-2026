using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single Responsibility: manages the shield block state and raises/lowers the shield.
///
/// FIX (DIP): now implements IBlocker so PlayerHealth depends on the interface,
/// not this concrete class.
///
/// FIX (Hierarchy): the shield is a child of CameraHolder/Main Camera, so all
/// localPosition / localRotation work is already in camera space — no changes
/// needed to the math, but shieldTransform must be assigned to the Shield child
/// of Main Camera in the Inspector.
///
/// The rest pose is captured once in Awake from wherever the shield sits in the
/// scene, so it works correctly regardless of parenting depth.
/// </summary>
public class BlockHandler : MonoBehaviour, IBlocker, InputSystem_Actions.IPlayerActions
{
    [Header("Shield Reference")]
    [Tooltip("Drag the Shield child of Main Camera here.")]
    [SerializeField] private Transform shieldTransform;

    [Header("Block Settings")]
    [SerializeField] private float staminaCostPerSecond = 15f;
    [SerializeField] private float damageReductionPercent = 0.75f;

    [Header("Raised Pose Offset (from rest)")]
    [Tooltip("How much to MOVE the shield from its rest local position when raised. " +
             "Positive Y lifts it, negative X brings it more central.")]
    [SerializeField] private Vector3 raisedPositionOffset = new Vector3(0.2f, 0.35f, 0.1f);

    [Tooltip("Additional local Euler rotation applied on top of the rest rotation when raised. " +
             "Tilts the face of the shield toward the camera's forward.")]
    [SerializeField] private Vector3 raisedRotationOffset = new Vector3(20f, 15f, 0f);

    [SerializeField] private float transitionSpeed = 14f;

    // Injected
    private IStaminaSystem _stamina;
    private InputSystem_Actions _input;

    // Rest pose — captured once in Awake, never mutated
    private Vector3 _restLocalPosition;
    private Quaternion _restLocalRotation;

    // Pre-computed raised pose targets
    private Vector3 _raisedLocalPosition;
    private Quaternion _raisedLocalRotation;

    // State
    private bool _blockInputHeld;
    private bool _isBlocking;

    // ── IBlocker ──────────────────────────────────────────────────────────────
    public bool IsBlocking => _isBlocking;
    public float DamageReductionPercent => damageReductionPercent;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (shieldTransform != null)
        {
            // Capture the exact rest pose the artist placed the shield at
            _restLocalPosition = shieldTransform.localPosition;
            _restLocalRotation = shieldTransform.localRotation;

            // Raised pose = rest + offset (works for any starting transform)
            _raisedLocalPosition = _restLocalPosition + raisedPositionOffset;
            _raisedLocalRotation = _restLocalRotation * Quaternion.Euler(raisedRotationOffset);
        }
    }

    /// <summary>Called by the bootstrapper after construction.</summary>
    public void Initialize(InputSystem_Actions input, IStaminaSystem stamina)
    {
        _input = input;
        _stamina = stamina;
        _input.Player.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        _input?.Player.RemoveCallbacks(this);
    }

    private void Update()
    {
        UpdateBlockState();
        AnimateShield();
    }

    // ── IPlayerActions ────────────────────────────────────────────────────────

    public void OnBlock(InputAction.CallbackContext context)
    {
        // performed = button held, cancelled = button released
        _blockInputHeld = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context) { }
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }

    // ── Block logic ───────────────────────────────────────────────────────────

    private void UpdateBlockState()
    {
        if (_blockInputHeld && _stamina.HasStamina)
        {
            bool consumed = _stamina.TryConsume(staminaCostPerSecond * Time.deltaTime);
            _isBlocking = consumed;
        }
        else
        {
            _isBlocking = false;
        }
    }

    private void AnimateShield()
    {
        if (shieldTransform == null) return;

        Vector3 targetPos = _isBlocking ? _raisedLocalPosition : _restLocalPosition;
        Quaternion targetRot = _isBlocking ? _raisedLocalRotation : _restLocalRotation;

        shieldTransform.localPosition = Vector3.Lerp(
            shieldTransform.localPosition, targetPos, Time.deltaTime * transitionSpeed);

        shieldTransform.localRotation = Quaternion.Lerp(
            shieldTransform.localRotation, targetRot, Time.deltaTime * transitionSpeed);
    }
}
