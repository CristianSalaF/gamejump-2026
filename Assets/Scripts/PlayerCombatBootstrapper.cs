using UnityEngine;

/// <summary>
/// Composition Root / Bootstrapper.
/// Single Responsibility: owns object-graph construction only — no game logic here.
/// Dependency Inversion: all systems talk through interfaces; this is the ONLY place
/// that knows about concrete types.
///
/// ── What changed ────────────────────────────────────────────────────────────
/// 1. FirstPersonMovement.Initialize(input) is called so movement shares the
///    single InputSystem_Actions instance instead of creating a conflicting second one.
/// 2. PlayerHealth.Initialize receives the IBlocker interface (not the concrete
///    BlockHandler), satisfying DIP end-to-end.
///
/// ── Scene setup ─────────────────────────────────────────────────────────────
/// Player GameObject needs these components:
///   • PlayerCombatBootstrapper  (this script)
///   • FirstPersonMovement
///   • SwordSwingHandler
///   • BlockHandler
///   • PlayerHealth
///   • StaminaSystem
///   • CharacterController
///
/// Inspector assignments required:
///   • FirstPersonMovement → cameraHolder = the CameraHolder child transform
///   • SwordSwingHandler   → swordTransform = the Sword child of Main Camera
///   • BlockHandler        → shieldTransform = the Shield child of Main Camera
/// </summary>
public class PlayerCombatBootstrapper : MonoBehaviour
{
    // All sibling components — resolved via GetComponent in Awake
    private InputSystem_Actions _input;
    private StaminaSystem _stamina;
    private FirstPersonMovement _movement;
    private SwordSwingHandler _sword;
    private BlockHandler _block;
    private PlayerHealth _health;

    private void Awake()
    {
        // 1. Create the single shared InputSystem_Actions instance and enable it.
        _input = new InputSystem_Actions();
        _input.Player.Enable();

        // 2. Resolve concrete components on this GameObject.
        _stamina   = GetComponent<StaminaSystem>();
        _movement  = GetComponent<FirstPersonMovement>();
        _sword     = GetComponent<SwordSwingHandler>();
        _block     = GetComponent<BlockHandler>();
        _health    = GetComponent<PlayerHealth>();

        ValidateDependencies();

        // 3. Inject dependencies.
        //    Movement gets the shared input so there is only ever one instance.
        _movement.Initialize(_input);

        //    Sword and block register their callbacks on the shared action map.
        _sword.Initialize(_input);
        _block.Initialize(_input, _stamina);

        //    Health depends on IBlocker — BlockHandler implements it (DIP satisfied).
        _health.Initialize(_block);
    }

    private void OnDestroy()
    {
        _input?.Player.Disable();
        _input?.Dispose();
    }

    private void ValidateDependencies()
    {
        if (_stamina  == null) Debug.LogError("[Bootstrapper] Missing StaminaSystem on Player.", this);
        if (_movement == null) Debug.LogError("[Bootstrapper] Missing FirstPersonMovement on Player.", this);
        if (_sword    == null) Debug.LogError("[Bootstrapper] Missing SwordSwingHandler on Player.", this);
        if (_block    == null) Debug.LogError("[Bootstrapper] Missing BlockHandler on Player.", this);
        if (_health   == null) Debug.LogError("[Bootstrapper] Missing PlayerHealth on Player.", this);
    }
}
