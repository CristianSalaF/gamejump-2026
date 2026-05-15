using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single Responsibility: animates the sword swing via Transform tweening and
/// applies damage at the apex of the swing.
///
/// FIX (Hierarchy): the sword is parented to Main Camera (child of CameraHolder).
/// All localRotation math therefore operates in camera space automatically —
/// no coordinate conversion needed.
///
/// FIX (Swing axis): your sword's longest dimension is along its LOCAL Z (scale Z=2).
/// A horizontal sweep across the player's view is a rotation around the sword's
/// LOCAL X axis:
///   • Negative X rotation  → blade sweeps right-to-left  (forward arc)
///   • Positive X rotation  → blade sweeps left-to-right  (forward arc)
/// Alternating direction per click gives the "combo" feel you asked for.
///
/// FIX (No drift): rest pose captured once in Awake; final snap at coroutine end
/// prevents floating-point accumulation across many swings.
/// </summary>
public class SwordSwingHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [Header("Sword Reference")]
    [Tooltip("Drag the Sword child of Main Camera here.")]
    [SerializeField] private Transform swordTransform;

    [Header("Swing Settings")]
    [Tooltip("Total degrees the blade sweeps per attack (around the sword's local X axis).")]
    [SerializeField] private float swingAngle = 70f;
    [SerializeField] private float swingDuration = 0.25f;
    [SerializeField] private float returnDuration = 0.18f;
    [SerializeField] private float cooldownDuration = 0.2f;

    [Header("Damage")]
    [SerializeField] private float damagePerSwing = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask hitLayers;

    // Injected by bootstrapper
    private InputSystem_Actions _input;

    // State
    private bool _isSwinging;
    private bool _swingFromRight = true;   // alternates each click for combo feel

    // Rest pose — captured once, never recalculated
    private Vector3 _restLocalPosition;
    private Quaternion _restLocalRotation;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (swordTransform != null)
        {
            _restLocalPosition = swordTransform.localPosition;
            _restLocalRotation = swordTransform.localRotation;
        }
    }

    /// <summary>Called by the bootstrapper after construction.</summary>
    public void Initialize(InputSystem_Actions input)
    {
        _input = input;
        _input.Player.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        _input?.Player.RemoveCallbacks(this);
    }

    // ── IPlayerActions ────────────────────────────────────────────────────────

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || _isSwinging) return;
        StartCoroutine(PerformSwing());
    }

    public void OnMove(InputAction.CallbackContext context) { }
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnBlock(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }

    // ── Swing coroutine ───────────────────────────────────────────────────────

    private IEnumerator PerformSwing()
    {
        _isSwinging = true;

        // Alternate direction each attack for a natural combo feel:
        //   right-to-left = negative X rotation (blade tip arcs left)
        //   left-to-right = positive X rotation (blade tip arcs right)
        float direction = _swingFromRight ? -1f : 1f;
        _swingFromRight = !_swingFromRight;

        Quaternion startRot = _restLocalRotation;
        Quaternion endRot   = _restLocalRotation * Quaternion.Euler(swingAngle * direction, 0f, 0f);

        // ── Forward arc ───────────────────────────────────────────────────────
        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / swingDuration);
            if (swordTransform != null)
                swordTransform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        // Deal damage at the apex of the swing
        TryDealDamage();

        // ── Return arc ────────────────────────────────────────────────────────
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDuration);
            if (swordTransform != null)
                swordTransform.localRotation = Quaternion.Lerp(endRot, startRot, t);
            yield return null;
        }

        // Hard snap to rest — prevents sub-frame drift accumulating over many swings
        if (swordTransform != null)
            swordTransform.localRotation = startRot;

        yield return new WaitForSeconds(cooldownDuration);
        _isSwinging = false;
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    private void TryDealDamage()
    {
        if (swordTransform == null) return;

        // Cast from the sword tip (offset along the blade's local Z, which is its long axis)
        Vector3 tipPosition = swordTransform.position
                            + swordTransform.forward * (attackRange * 0.5f);

        Collider[] hits = Physics.OverlapSphere(tipPosition, attackRange * 0.5f, hitLayers);
        foreach (Collider hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(damagePerSwing);
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (swordTransform == null) return;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Vector3 tip = swordTransform.position + swordTransform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(tip, attackRange * 0.5f);
    }
}
