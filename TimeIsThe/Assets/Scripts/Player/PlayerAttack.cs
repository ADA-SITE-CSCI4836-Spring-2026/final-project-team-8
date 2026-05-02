using UnityEngine;

/// <summary>
/// Standalone attack input handler.
/// Add this to the same GameObject as BasicBehaviour (the character root).
/// Does not depend on PlayerController or CharacterController.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    private Animator _animator;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
            Debug.LogError("[PlayerAttack] No Animator found on this GameObject. Attack will not work.", this);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("[PlayerAttack] Fire1 detected — setting Attack trigger.");
            _animator.SetTrigger(AttackHash);
        }
    }
}
