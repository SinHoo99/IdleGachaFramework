using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    
    // Cached animator hashes
    private readonly int _isIdleHash = Animator.StringToHash("isIdle");
    private readonly int _isAttackingHash = Animator.StringToHash("isAttacking");
    private readonly int _isHitHash = Animator.StringToHash("isHit");
    private readonly int _isDieHash = Animator.StringToHash("isDie");
    private readonly int _isParryingHash = Animator.StringToHash("isParrying");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    public void UpdateAnimationState(PlayerState state, bool isParrying)
    {
        if (_animator == null) return;

        SafeSetBool(_isIdleHash, state == PlayerState.Idle);
        SafeSetBool(_isAttackingHash, state == PlayerState.Attack);
        SafeSetBool(_isHitHash, state == PlayerState.Hit);
        SafeSetBool(_isDieHash, state == PlayerState.Die);
        SafeSetBool(_isParryingHash, isParrying);
    }

    private void SafeSetBool(int hash, bool value)
    {
        if (_animator == null) return;
        
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.nameHash == hash)
            {
                _animator.SetBool(hash, value);
                return;
            }
        }
    }
}
