using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public partial class PlayerManager : MonoBehaviour
{
    public enum PlayerState { Idle, Attacking, Stun, Jumping, Dashing, GettingHit, Dead }
    #region Exposed variables


    [Header("Main")]
    public float m_MaxPlayerHP = 100f;
    [Min(0f)] public float mStunHitTime = 0.5f;
    [Range(0f, 1f)] public float mSlowIntensity = 0.25f;
    public AnimationCurve mMudSlownessCurve = new AnimationCurve();

    #endregion

    #region Private/Local variables
    private PlayerState currentState;
    private float currentHP;


    /// The pack of information I'll get when raycasting down 
    /// The direction the player is currently looking at

    private float mMaxRunSpeed = 0;
    private bool _IsStun => currentState == PlayerState.Stun;
    private bool _IsAttacking => currentState == PlayerState.Attacking;
    private bool _IsJumping => currentState == PlayerState.Jumping;
    private bool _IsDashing => currentState == PlayerState.Dashing;
    private bool _IsIdle => currentState == PlayerState.Idle;
    private bool _IsGettingHit => currentState == PlayerState.GettingHit;





    #endregion

    #region Properties



    private CharacterController mCharacterController;
    private CharacterController _CharacterController { get { if (mCharacterController == null) mCharacterController = GetComponentInChildren<CharacterController>(); return mCharacterController; } }



    private Rigidbody mRigidbody;
    private Rigidbody _Rigidbody { get { if (mRigidbody == null) mRigidbody = GetComponentInChildren<Rigidbody>(); return mRigidbody; } }


    private Animator animator;
    private Animator _Animator { get { if (animator == null) animator = GetComponentInChildren<Animator>(); return animator; } }
    private AnimatorOverrideController animatorOverride;
    private AnimatorOverrideController _AnimatorOverride { get { if (animatorOverride == null)
            {
                animatorOverride = new AnimatorOverrideController(_Animator.runtimeAnimatorController);
                _Animator.runtimeAnimatorController = animatorOverride;
            }
            return animatorOverride; } }

    

    private static PlayerManager sInstance;

    public static Vector3 Position => sInstance.transform.position;
    public static Vector3 Forward => sInstance.transform.forward;
    public static Vector3 Right => sInstance.transform.right;
    public static bool IsDodging => sInstance.isDodging;
    public static float sCurrentHP => sInstance.currentHP;
    public static float sCurrentRatioHP => sCurrentHP / sInstance.m_MaxPlayerHP;
    public static bool StopActions { get; set; }

    private bool _CanMove => !StopActions && _IsIdle && GameManager.sGameHasStarted;
    private bool _IsSlowed => mCurrentSlowRatio > 0;
    private bool _IsRunning => _CanMove && !_IsSlowed && Input.GetAxis("Run") > 0.1f;
    private bool _CanDash => _IsIdle && GameManager.sGameHasStarted && (Time.time > (timeWhenDashed + m_DashCooldown + m_DashTime)) && !StopActions;
    /// <summary>
    /// Is on idle and the game manager has started and isn't stopping actions
    /// </summary>

    private bool _CanStartAttacking => (_IsIdle /*|| (_IsAttacking && canAttackAgain)*/) && GameManager.sGameHasStarted && !StopActions;




    #endregion

    #region Events
    public static Action OnHit;
    public static Action OnHeal;
    public static Action OnStun;
    public static Action OnUnfreeze;
    public static Action OnDeath;
    #endregion


    #region MonoBehaviour
    private void Awake()
    {
        currentHP = m_MaxPlayerHP;
        animator = GetComponentInChildren<Animator>();
        animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverride;
        transform.rotation = Quaternion.Euler(Vector3.up*180);
        mCurrentSlowRatio = 1f;
        timeWhenDashed = (m_DashCooldown + m_DashTime) * (-1);
        GameManager.OnGameReady += delegate { mCurrentSlowRatio = 0f; };

        if (sInstance != null)
        {
            Debug.Log("[Player] There was already an instance, wtf");
            Destroy(sInstance);
        }
        sInstance = this;

        //LayerAndTag
        this.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Player");


        mMaxRunSpeed = mMaxWalkSpeed * mRunSpeedRatio;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    private void Start()
    {
        Vector3 startingPoint = transform.position + Vector3.up;
        if (Physics.Raycast(startingPoint, Vector3.down, out mDownHitInfo, 10f, MathHelper.BlockersAndGroundLayerMask))
            transform.position = mDownHitInfo.point;
    }

    private void Update()
    {

        Shader.SetGlobalVector("PlayerPosition", transform.position);
        CheckAttack();
        if (_CanMove) MoveAndRotate();
        if (_CanDash) DashCheck();
        JumpCheck();

    }


    #endregion

    #region PlayerState
    private void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        Debug.Log("Switched from " + currentState + " to " + newState);
        OnExitState(newState);
        currentState = newState;
        OnEnterState();
    }

    private void OnExitState(PlayerState newState)
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Attacking:
                break;
            case PlayerState.Stun:
                break;
            case PlayerState.Jumping:
                isHurtWhileJumping = false;
                verticalVelocity = 0;
                break;
            case PlayerState.Dashing:
                _Animator?.SetTrigger("Fine");
                break;
            case PlayerState.GettingHit:
                break;
            default:
                break;
        }
    }

    private void OnEnterState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Attacking:
                break;
            case PlayerState.Stun:
                break;
            case PlayerState.Jumping:
                break;
            case PlayerState.Dashing:
                _Animator?.SetTrigger("Dash");
                break;
            case PlayerState.GettingHit:
                break;
            case PlayerState.Dead:
                OnDeath?.Invoke();
                _Animator?.SetBool("Dead", true);
                break;
            default:
                break;
        }
    }


    #endregion





    #region Interaction
    

    private IEnumerator StunCoroutine(float stunTime = 0)
    {
        OnStun?.Invoke();
        _Animator?.SetTrigger("Fall");
        yield return new WaitForSeconds(stunTime == 0 ? 0 : mStunHitTime);
       _Animator?.SetTrigger("GetUp");
        OnUnfreeze?.Invoke();

    }


    public static void DealPlayerDamage(float amount)
    {
        if (!PlayerManager.IsDodging)
        {
            sInstance.currentHP -= amount;
            if (sInstance.currentHP < 0)
            {
                sInstance.ChangeState(PlayerState.Dead);
            }
            else
            {
                Debug.Log("PlayerHit!");
                OnHit?.Invoke();
            }
        }
    }

    public static void HealPlayer(float amount)
    {
        sInstance.currentHP = Mathf.Max(sInstance.currentHP + amount, sInstance.m_MaxPlayerHP);
        OnHeal?.Invoke();
    }
    #endregion


}
