using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : Enemy
{
    [Header("Custom")]
    public float m_NormalDamage = 10f;
    public float m_HeavyDamage = 25f;
    private Animator animator;
    private Animator _Animator { get { if (animator == null) animator = GetComponentInChildren<Animator>(); return animator; } }
    public EnemyCheckCollision m_CollisionCheck;




    protected override void EnterIdleCustom()
    {
        _Animator?.SetTrigger("PatrolIdle");
    }

    protected override void EnterPatrolCustom()
    {
        _Animator?.SetTrigger("PatrolWalk");
    }

    

    protected override void EnterDeadCustom()
    {
        _Animator.SetFloat("DeathType", Random.value > 0.5f ? 1 : 0);
        _Animator?.SetTrigger("Death");
    }
    protected override void EnterAoeCombatCustom()
    {

        _Animator.SetTrigger("AttackIdle");
    }
    protected override void EnterRangedCombatCustom()
    {
        _Animator.SetTrigger("AttackIdle");
    }

    protected override void RangedAttackCustom()
    {
        _Animator.SetFloat("AttackType", Random.value > 0.5f ? 1 : 0);
        _Animator?.SetTrigger("NormalAttack");
        isRangedAttack = true;
    }

    protected override void AoeAttackCustom()
    {
        _Animator?.SetTrigger("HeavyAttack");
        isRangedAttack = false;
    }

    protected override void AddDamageCustom()
    {
        _Animator.SetTrigger("Hit");
    }

    protected override void ReachedDesiredRangeCustom()
    {
        _Animator.SetTrigger("AttackIdle");
    }

    protected override void SeekingToBeAtRangeCustom()
    {
        _Animator.SetTrigger("AttackRun");
    }

    public void CheckAttack(int state)
    {
        m_CollisionCheck.EnableCollider(state == 1 ? true : false);
    }

    private bool isRangedAttack = false;
    public void DealDamage()
    {
        GameManager.DealPlayerDamage(isRangedAttack ? m_NormalDamage : m_HeavyDamage);
    }

}
