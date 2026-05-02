using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int SpeedHash    = Animator.StringToHash("Speed");
    private static readonly int JumpHash     = Animator.StringToHash("Jump");
    private static readonly int DeathHash    = Animator.StringToHash("Death");
    private static readonly int AttackHash   = Animator.StringToHash("Attack");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    public void SetMoving(bool isMoving, float speed)
    {
        _animator.SetBool(IsMovingHash, isMoving);
        _animator.SetFloat(SpeedHash, speed);
    }

    public void TriggerJump()
    {
        _animator.SetTrigger(JumpHash);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(AttackHash);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        _animator.SetTrigger(DeathHash);
    }
}
