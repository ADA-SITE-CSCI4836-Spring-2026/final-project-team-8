using System.Collections;
using UnityEngine;

/// <summary>
/// Adds a directional dash to the player with invincibility frames.
///
/// - Input       : Left Shift (configurable)
/// - Direction   : current movement input relative to camera, or forward if idle
/// - Movement    : impulse force on the Rigidbody (works with BasicBehaviour)
/// - I-frames    : player is invincible for the full iFrameDuration
/// - Cooldown    : dash cannot be reused until cooldown expires
/// - Visual cue  : optional trail renderer flashes during i-frames
///
/// Add this to the same root GameObject as BasicBehaviour and PlayerStats.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashForce      = 18f;   // impulse strength
    [SerializeField] private float dashDuration   = 0.18f; // seconds the dash lasts
    [SerializeField] private float cooldown       = 3.0f;  // seconds before next dash
    [SerializeField] private string dashButton    = "Sprint"; // default: Left Shift

    [Header("Invincibility")]
    [SerializeField] private float iFrameDuration = 0.4f;  // seconds of invincibility (can exceed dashDuration)

    [Header("Visual")]
    [SerializeField] private TrailRenderer dashTrail;      // optional — assign in Inspector
    [SerializeField] private float trailDuration  = 0.3f;

    // ── State ─────────────────────────────────────────────────────────────────
    public bool  IsDashing     { get; private set; }
    public bool  IsOnCooldown  => _cooldownTimer > 0f;
    public float CooldownRatio => Mathf.Clamp01(_cooldownTimer / cooldown);

    private float        _cooldownTimer;
    private Rigidbody    _rb;
    private PlayerStats  _playerStats;
    private BasicBehaviour _basicBehaviour;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rb              = GetComponent<Rigidbody>();
        _playerStats     = GetComponent<PlayerStats>();
        _basicBehaviour  = GetComponent<BasicBehaviour>();

        if (_playerStats == null)
            Debug.LogError("[PlayerDash] PlayerStats not found on this GameObject.", this);

        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (Input.GetButtonDown(dashButton) && !IsDashing && !IsOnCooldown)
            StartCoroutine(DashRoutine());
    }

    // ── Dash coroutine ────────────────────────────────────────────────────────

    private IEnumerator DashRoutine()
    {
        IsDashing = true;
        _cooldownTimer = cooldown;

        // ── Direction ──────────────────────────────────────────────────────
        Vector3 dashDir = GetDashDirection();

        // ── Grant i-frames ─────────────────────────────────────────────────
        _playerStats?.SetInvincible(true);

        // ── Enable trail ───────────────────────────────────────────────────
        if (dashTrail != null)
        {
            dashTrail.emitting = true;
            dashTrail.time     = trailDuration;
        }

        // ── Apply impulse ──────────────────────────────────────────────────
        // Clear vertical velocity first so dash stays horizontal
        Vector3 vel = _rb.velocity;
        vel.y = 0f;
        _rb.velocity = vel;
        _rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);

        // ── Wait for dash movement to finish ──────────────────────────────
        yield return new WaitForSeconds(dashDuration);

        IsDashing = false;

        // ── Stop trail ────────────────────────────────────────────────────
        if (dashTrail != null)
            dashTrail.emitting = false;

        // ── Keep i-frames active for remaining duration ────────────────────
        float iFramesRemaining = iFrameDuration - dashDuration;
        if (iFramesRemaining > 0f)
        {
            // Optional: flash the character to signal i-frames visually
            StartCoroutine(FlashRoutine(iFramesRemaining));
            yield return new WaitForSeconds(iFramesRemaining);
        }

        // ── Revoke i-frames ───────────────────────────────────────────────
        _playerStats?.SetInvincible(false);
    }

    // ── Direction calculation ─────────────────────────────────────────────────

    private Vector3 GetDashDirection()
    {
        float h = 0f;
        float v = 0f;

        // Read from BasicBehaviour if available (it already processes input)
        if (_basicBehaviour != null)
        {
            h = _basicBehaviour.GetH;
            v = _basicBehaviour.GetV;
        }
        else
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        if (inputDir.sqrMagnitude < 0.01f)
        {
            // No input — dash forward
            return transform.forward;
        }

        // Transform input direction relative to camera
        if (_basicBehaviour != null && _basicBehaviour.playerCamera != null)
        {
            Transform cam = _basicBehaviour.playerCamera;
            Vector3 camForward = cam.forward;
            Vector3 camRight   = cam.right;
            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();
            return (camForward * v + camRight * h).normalized;
        }

        return transform.TransformDirection(inputDir).normalized;
    }

    // ── Visual flash during i-frames ──────────────────────────────────────────

    private IEnumerator FlashRoutine(float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsed  = 0f;
        float interval = 0.08f;  // flash every 80ms
        bool  visible  = true;

        while (elapsed < duration)
        {
            visible = !visible;
            foreach (Renderer r in renderers)
                r.enabled = visible;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Ensure renderers are visible when done
        foreach (Renderer r in renderers)
            r.enabled = true;
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Show dash reach as a line in Scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * (dashForce * dashDuration * 0.5f));
    }
}
