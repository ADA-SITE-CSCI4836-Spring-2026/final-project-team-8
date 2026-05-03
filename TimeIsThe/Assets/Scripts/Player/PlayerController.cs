using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _characterController;
    private PlayerAnimator _playerAnimator;
    private Camera _cam;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerAnimator = GetComponent<PlayerAnimator>();

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false;
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        // Ignore all input when paused
        if (PauseManager.IsPaused) return;

        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        HandleAttack();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        _isGrounded = _characterController.isGrounded;
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camRight * horizontal + camForward * vertical;
        _characterController.Move(direction * moveSpeed * Time.deltaTime);

        bool isMoving = direction.magnitude > 0.1f;
        _playerAnimator?.SetMoving(isMoving, direction.magnitude);

        if (isMoving)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), 10f * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _playerAnimator?.TriggerJump();
        }
    }

    private void HandleAttack()
    {
        if (Input.GetButtonDown("Fire1"))
            _playerAnimator?.TriggerAttack();
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
