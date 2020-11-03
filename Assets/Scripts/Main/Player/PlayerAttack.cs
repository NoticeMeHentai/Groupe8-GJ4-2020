using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager
{
    [Header("Attacks")]
    public bool m_ShowAttackDebugs = false;
    public AttackSlot m_StartingLightAttack;
    public AttackSlot m_StartingHeavyAttack;


    private bool nextComboIsLight = false;

    /// <summary>
    /// After a combo attack, there's a small windows in the animation where the player can either just wait for the animation to end and go to idle, or actually continue the combo or dash or something else
    /// </summary>
    private bool canDoSomethingElseAfterAttacking = false;
    private AttackSlot _CurrentAttackCombo;
    /// <summary>
    /// If when 'checkIfPlayerWantsToKeepCombo' is set to true, the player actually keeps attacking, it sets to true, so at the given time, the animation automatically switches to the other combo
    /// </summary>
    private bool playerWantsToKeepCombo = false;
    /// <summary>
    /// Time windows in the animation where we check if the player wants to keep the combo or not
    /// </summary>
    private bool checkIfPlayerWantsToKeepCombo = false;
    private int comboCounter = 0;
    private bool transitioningToAnotherCombo = false;
    private bool _TriesToDoSomethingElse => Input.GetAxis("HorizontalMovement") != 0
                || Input.GetAxis("VerticalMovement") != 0
                || Input.GetButtonDown("Dash");

    private string _NextComboLabel => comboCounter % 2 == 1 ? _CurrentAnimLabelA : _CurrentAnimLabelB;
    private string _CurrentAnimLabelA = "AttackAPlaceHolder";
    private string _CurrentAnimLabelB = "AttackBPlaceHolder";
    private AnimationClip _NextComboClip => comboCounter % 2 == 1 ? _CurrentAnimClipA : _CurrentAnimClipB;
    private AnimationClip _CurrentAnimClipA;
    private AnimationClip _CurrentAnimClipB;



    private void CheckAttack()
    {
        bool wantsToLightAttack = Input.GetButton("NormalAttack");
        bool wantsToHeavyAttack = Input.GetButton("HeavyAttack");
        bool wantsToAttack = wantsToLightAttack|| wantsToHeavyAttack;
        if (checkIfPlayerWantsToKeepCombo && wantsToAttack)
        {
            nextComboIsLight = wantsToLightAttack;
            playerWantsToKeepCombo = true;
            checkIfPlayerWantsToKeepCombo = false;
        } 
        else if (canDoSomethingElseAfterAttacking)
        {
            bool canComboLight = wantsToLightAttack && _CurrentAttackCombo.HasLightCombo;
            if (canComboLight
                || (wantsToHeavyAttack && _CurrentAttackCombo.HasHeavyCombo))
            {
                transitioningToAnotherCombo = true;
                if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] While he could choose something else, the player decided to attack!");
                canDoSomethingElseAfterAttacking = false;
                checkIfPlayerWantsToKeepCombo = false;
                playerWantsToKeepCombo = false;
                SwitchToNextAttack(canComboLight?AttackSlot.AttackType.Light:AttackSlot.AttackType.Heavy);
            }
            else if (_TriesToDoSomethingElse) 
            {
                if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] While he could choose something else, the player decided to choose something else!");
                ChangeToIdle(); //Since the attack method occurs before any other method, the player will be able to actually dash
            } 
        }
        else if(_CanStartAttacking && wantsToAttack)
        {
            if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] While idling, the player decided to attack!");
            comboCounter = 1;
            _CurrentAttackCombo = wantsToLightAttack ? m_StartingLightAttack : m_StartingHeavyAttack;
            _Animator.SetBool("AttackFine", false);
            if (_CurrentAnimClipA == null) _CurrentAnimClipA = _AnimatorOverride["AttackAPlaceHolder"];
            if (_CurrentAnimClipB == null) _CurrentAnimClipB = _AnimatorOverride["AttackBPlaceHolder"];
            animatorOverride[_NextComboClip] = _CurrentAttackCombo.AnimationToPlay;
            _CurrentAnimClipA = _CurrentAttackCombo.AnimationToPlay;
            _Animator.SetTrigger("Attack");
            ChangeState(PlayerState.Attacking);
            //Animation takes care of collision activation, what moment to deal damage and
        }

    }

    /// <summary>
    /// Actually switches the animation to the next attack
    /// </summary>
    /// <param name="attackType"></param>
    private void SwitchToNextAttack(AttackSlot.AttackType attackType)
    {
        _Animator.SetBool("AttackFine", false);
        _CurrentAttackCombo = attackType == AttackSlot.AttackType.Light? _CurrentAttackCombo.LightComboContinuation: _CurrentAttackCombo.HeavyComboContinuation;
        comboCounter++;

        animatorOverride[_NextComboClip] = _CurrentAttackCombo.AnimationToPlay;
        if (comboCounter % 2 == 1)
            _CurrentAnimClipA = _CurrentAttackCombo.AnimationToPlay;
        else
            _CurrentAnimClipB = _CurrentAttackCombo.AnimationToPlay;

        _Animator.SetTrigger("Attack");
        canDoSomethingElseAfterAttacking = false;
        checkIfPlayerWantsToKeepCombo = false;
        transitioningToAnotherCombo = true;
        playerWantsToKeepCombo = false;
    }

    #region Bin
    //private void AttackCheck()
    //{
    //    if (canDoSomethingElseAfterAttacking) //Meaning he's comboing
    //    {
    //        if (_TriesToDoSomethingElse)
    //        {
    //            ChangeState(PlayerState.Idle);
    //            _AnimatorOverride?.SetTrigger("Fine");
    //            isInCombo = false;
    //            canDoSomethingElseAfterAttacking = false;
    //            isChainingCombo = false;
    //            return;
    //        }
    //    }
    //    bool hasPressedNormalAttack = Input.GetButtonDown("NormalAttack");
    //    bool hasPressedHeavyAttack = Input.GetButtonDown("HeavyAttack");

    //    bool hasPressedAnyAttack = hasPressedHeavyAttack || hasPressedNormalAttack;
    //    if (_CanAttack && hasPressedAnyAttack && !isInCombo)
    //    {
    //        isNormalAttack = hasPressedNormalAttack;
    //        isChainingCombo = false;
    //        currentNormalComboCount = 0;
    //        _AnimatorOverride?.SetInteger("CurrentCombo", 0);
    //        _AnimatorOverride?.SetTrigger(hasPressedNormalAttack ? "NormalAttack" : "HeavyAttack");
    //        ChangeState(PlayerState.Attacking);
    //        isInCombo = true;

    //    }
    //    else if (isInCombo)
    //    {
    //        if (isNormalAttack && _CurrentNormalComboAttack._CanKeepCombo)
    //        {
    //            canDoSomethingElseAfterAttacking = true;
    //            if (hasPressedNormalAttack)
    //            {
    //                isChainingCombo = true;
    //                currentNormalComboCount = (currentNormalComboCount + 1) % m_NormalAttackCombo.Length;
    //                _Animator?.SetInteger("CurrentCombo", currentNormalComboCount);
    //                _AnimatorOverride?.SetTrigger("NormalAttack");
    //                canDoSomethingElseAfterAttacking = false;
    //            }

    //        }
    //        else if (isNormalAttack && _CurrentNormalComboAttack._ComboIsOver) isChainingCombo = false;

    //    }
    //    else if (!isNormalAttack && !isInCombo && _IsAttacking)
    //    {
    //        if (Input.GetAxis("HorizontalMovement") != 0
    //            || Input.GetAxis("VerticalMovement") != 0
    //            || Input.GetButtonDown("Dash"))
    //        {
    //            ChangeState(PlayerState.Idle);
    //            _AnimatorOverride?.SetTrigger("Fine");
    //            isInCombo = false;
    //            canDoSomethingElseAfterAttacking = false;
    //            isChainingCombo = false;
    //            return;
    //        }
    //    }


    //} 
    #endregion
    #region Called by animations

    /// <summary>
    /// If there are any enemies to be hit, hit them
    /// </summary>
    public void HitEnemies()
    {
        transitioningToAnotherCombo = false;
        Enemy[] enemies = _CurrentAttackCombo.GetEnemiesHit();
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].AddDamage(_CurrentAttackCombo.Damage);
        }
    }

    /// <summary>
    /// Each attack can have different custom methods like activating colliders, calling vfx etc
    /// </summary>
    /// <param name="index"></param>
    public void ComboCalling(int index)
    {
        _CurrentAttackCombo.CustomComboCalling(index);
    }

    /// <summary>
    /// Opens the possibility of the player pressing attack again to keep combo
    /// </summary>
    public void StartCheckingIfWeWantToKeepCombo()
    {
        if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] From now on, the player could possibly chain with yet another combo!");
        checkIfPlayerWantsToKeepCombo = true;
    }
    /// <summary>
    /// Actually checks if the player wants to continue the combo and does so if true
    /// Also opens the possibility of the player doing something else like dashing 
    /// </summary>
    public void CheckIfWeKeepComboOrNot()
    {
        if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] Before being able to do anything else, does the player want to continue the combo?");
        if (playerWantsToKeepCombo) 
        {
            SwitchToNextAttack(nextComboIsLight?AttackSlot.AttackType.Light: AttackSlot.AttackType.Heavy);
            if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] The player wanted to chain the animation and so he did!");
        }
        else
        {
            canDoSomethingElseAfterAttacking = true;
            if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] The player can now do something aside from comboing!");
        }
    }

    /// <summary>
    /// Called by the animation when the attack animation has been played and there haven't been any follow-play for another attack, so it just transitions toward the Idle status
    /// </summary>
    public void ChangeToIdle()
    {
        if (!transitioningToAnotherCombo)
        {
            _AnimatorOverride[_CurrentAnimLabelA] = m_StartingLightAttack.AnimationToPlay;
            _AnimatorOverride[_CurrentAnimLabelB] = m_StartingLightAttack.AnimationToPlay;
            if (m_ShowAttackDebugs) Debug.Log("[PlayerAttack] No more comboing, returning to idle or something else!");
            comboCounter = 0;
            _Animator?.SetBool("AttackFine", true);
            Debug.Log("r");
            ChangeState(PlayerState.Idle);
            canDoSomethingElseAfterAttacking = false;
            checkIfPlayerWantsToKeepCombo = false;
            playerWantsToKeepCombo = false; 
        }
    } 
    #endregion
}
