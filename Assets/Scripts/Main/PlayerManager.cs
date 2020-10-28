using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public partial class PlayerManager : MonoBehaviour
{
    public enum PlayerState { Idle, Attacking, Stun, Jumping, Dashing, GettingHit, Dead }
    #region Exposed variables



    [Header("Obstacles")]
    [Min(0f)] public float mStunHitTime = 0.5f;
    [Range(0f, 1f)] public float mSlowIntensity = 0.25f;
    public AnimationCurve mMudSlownessCurve = new AnimationCurve();
    [Header("Components")]
    public bool mHasAnimator = false;

    #endregion

    #region Private/Local variables
    private PlayerState currentState;


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

    
    private Animator mAnimator;
    private Animator _Animator { get { if (mAnimator == null) mAnimator = GetComponentInChildren<Animator>(); return mAnimator; } }

    

    private static PlayerManager sInstance;
    public static Action OnStun;
    public static Action OnUnfreeze;
    public static Action OnDeath;
    public static Vector3 Position => sInstance.transform.position;
    public static Vector3 Forward => sInstance.transform.forward;
    public static Vector3 Right => sInstance.transform.right;
    public static bool IsDodging => sInstance.isDodging;

    public static bool StopActions { get; set; }

    private bool _CanMove => !StopActions && _IsIdle && GameManager.sGameHasStarted;
    private bool _IsSlowed => mCurrentSlowRatio > 0;
    private bool _IsRunning => _CanMove && !_IsSlowed && Input.GetAxis("Run") > 0.1f;
    private bool _CanDash => _IsIdle && GameManager.sGameHasStarted && (Time.time > (timeWhenDashed + m_DashCooldown + m_DashTime)) && !StopActions;
    /// <summary>
    /// Is on idle and the game manager has started and isn't stopping actions
    /// </summary>
    private bool _CanJump => _IsIdle && GameManager.sGameHasStarted && !StopActions;
    private bool _CanAttack => (_IsIdle /*|| (_IsAttacking && canAttackAgain)*/) && GameManager.sGameHasStarted && !StopActions;




    #endregion


    #region MonoBehaviour
    private void Awake()
    {
        transform.rotation = Quaternion.Euler(Vector3.up*180);
        mCurrentSlowRatio = 1f;
        timeWhenDashed = (m_DashCooldown + m_DashTime) * (-1);
        GameManager.OnGameReady += delegate { mCurrentSlowRatio = 0f; };
        GameManager.OnPlayerDeath += delegate { ChangeState(PlayerState.Dead); };
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
        AttackCheck();
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
                _Animator?.SetBool("Dead", true);
                break;
            default:
                break;
        }
    }


    #endregion

    #region Input Functions
   


    #endregion

    #region Attack
    
    

    #endregion

    #region Interaction
    

    private IEnumerator StunCoroutine(float stunTime = 0)
    {
        if (OnStun != null) OnStun();
        if (mHasAnimator) _Animator.SetTrigger("Fall");
        yield return new WaitForSeconds(stunTime == 0 ? 0 : mStunHitTime);
        if (mHasAnimator) _Animator.SetTrigger("GetUp");
        if (OnUnfreeze != null) OnUnfreeze();
    }
    #endregion


}
