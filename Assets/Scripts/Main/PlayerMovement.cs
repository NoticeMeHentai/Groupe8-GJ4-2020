using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Idle, Attacking, Stun, Jumping, Dashing, GettingHit, Dead }
    #region Exposed variables
    [Header("Movement")]
    [Min(0f)]public float mMaxWalkSpeed = 10f;
    [Min(0f)]public float m_AirSpeed = 10f;
    [Min(0f)] public float mRunSpeedRatio = 2.5f;
    [Range(0.2f,1f)]public float mAccelerationTime = 0.5f;
    [Range(10f,30f)]public float mRotationSpeed = 30f;
    [Header("Dash")]
    [Min(0f)] public float m_DashTime = 0.5f;
    [Min(0f)] public float m_DashCooldown = 0.5f;
    [Min(0f)] public float m_DashDistance = 0.5f;
    public AnimationCurve m_DashPositionCurve;
    [Header("Attacks")]
    public float m_FirstAttackDamage = 25f;
    public float m_FirstAttackDuration = 0.8f;
    public PlayerCheckEnemyCollision m_SwordCollider;
    //[Header("Jump")]
    //[Min(0f)] public float m_JumpForce = 1.5f;
    //[Min(0f)] public float m_JumpTime = .5f;
    //[Min(0f)] public float m_Gravity = 15f;
    //[Min(0f)] public float m_VerticalInfluence = 5f;
    [Header("Obstacles")]
    [Min(0f)] public float mStunHitTime = 0.5f;
    [Range(0f, 1f)] public float mSlowIntensity = 0.25f;
    public AnimationCurve mMudSlownessCurve = new AnimationCurve();
    [Header("Components")]
    public bool mHasAnimator = false;

    #endregion

    #region Private/Local variables
    private PlayerState currentState;
    private bool canAttackAgain = false;
    private float mCurrentWalkRatio = 0;
    private float mCurrentRunRatio = 0;
    private float characterPivotVerticalDistance;
    /// The pack of information I'll get when raycasting down 
    private RaycastHit mDownHitInfo;
    /// The direction the player is currently looking at
    private Vector3 mDirection;
    private float mMaxRunSpeed = 0;
    private float mCurrentSlowRatio = 0;
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

    private static PlayerMovement sInstance;
    public static Action OnStun;
    public static Action OnUnfreeze;
    public static Action OnDeath;
    public static Vector3 Position => sInstance.transform.position;
    public static Vector3 Forward => sInstance.transform.forward;
    public static Vector3 Right => sInstance.transform.right;

    public static bool StopActions { get; set; }

    private bool _CanMove => !StopActions && _IsIdle && GameManager.sGameHasStarted;
    private bool _IsSlowed => mCurrentSlowRatio > 0;
    private bool _IsRunning => _CanMove && !_IsSlowed && Input.GetAxis("Run") > 0.1f;
    private bool _CanDash => _IsIdle && GameManager.sGameHasStarted && (Time.time > (timeWhenDashed + m_DashCooldown + m_DashTime)) && !StopActions;
    private bool _CanJump => _IsIdle && GameManager.sGameHasStarted && !StopActions;
    private bool _CanAttack => (_IsIdle || (_IsAttacking && canAttackAgain)) && GameManager.sGameHasStarted && !StopActions;




    #endregion


    #region MonoBehaviour
    private void Awake()
    {
        transform.rotation = Quaternion.identity;
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

        //OnStun += delegate { mIsFrozen = true;  Invoke(nameof(Unfreeze), mStunTime); };
        //OnUnfreeze += delegate { mIsFrozen = false; };

        characterPivotVerticalDistance = Mathf.Abs(_CharacterController.bounds.min.y - transform.position.y);
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
        if (_CanMove) MoveAndRotate();
        if (_CanDash) DashCheck();
        AttackCheck();
        //JumpCheck();

    }


    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (hit.collider.CompareTag("Rock") && !mIsFrozen && !mIsInMud && (Input.GetAxis("Run") > 0.2f)) Fall();
    //}


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
    float walkLerpSpeed = 0;
    float runLerpSpeed = 0;
    float currentSpeed;
    Vector2 movementInput;
    /// <summary>
    /// Moves the character depending on the axis values
    /// </summary>
    private void MoveAndRotate()
    {
        //Data preparation
        movementInput = new Vector2(Input.GetAxis("HorizontalMovement"), Input.GetAxis("VerticalMovement"));
        movementInput = movementInput.LimitMagnitude(1f);
        //Debug.Log(movementInput);
        walkLerpSpeed = mCurrentWalkRatio;
        runLerpSpeed = mCurrentRunRatio;



        //Smooth lineear gradient movement
        if (movementInput.sqrMagnitude > 0.2) //If acceleration
        {
            walkLerpSpeed = MathHelper.HardIn(Mathf.Clamp(walkLerpSpeed + movementInput.sqrMagnitude * Time.deltaTime / mAccelerationTime, 0f, movementInput.sqrMagnitude), FunctionsCurves.Xexp2);
            if(_IsRunning) runLerpSpeed = MathHelper.HardIn(Mathf.Clamp01(runLerpSpeed +  Time.deltaTime / mAccelerationTime), FunctionsCurves.Xexp3);
        }
        //If not giving any input and it's still slowing down, slow down
        else if (walkLerpSpeed > 0.05f) 
        {
            walkLerpSpeed = MathHelper.EasyIn(Mathf.Clamp01(walkLerpSpeed - Time.deltaTime / mAccelerationTime), FunctionsCurves.Xexp3);
            runLerpSpeed = 0;
            if (walkLerpSpeed <= 0.1f) walkLerpSpeed = 0f;
        }
        currentSpeed = 0;
        if(!_IsRunning)
        currentSpeed = walkLerpSpeed * (1 - mCurrentSlowRatio) * mMaxWalkSpeed;
        else if (_IsRunning)
        {
            float difference = (mRunSpeedRatio - 1f) * mMaxWalkSpeed; //To optimize
            currentSpeed = mMaxWalkSpeed + difference * mRunSpeedRatio;
        }

        float speedAnimatorParameter = (_IsRunning? 0.5f + runLerpSpeed * 0.5f : walkLerpSpeed*0.5f);
        _Animator?.SetFloat("Speed", speedAnimatorParameter);


        //Delta movement calculus
        mDirection = CameraManager.RightDirection * movementInput.x + 
            CameraManager.ForwardDirection * movementInput.y;



        //Rotation
        if (mDirection.magnitude > 0.2f)
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(mDirection), mRotationSpeed * Time.deltaTime * (1 - mCurrentSlowRatio));
        mDirection *= currentSpeed * Time.deltaTime;




        //If there's a "Object reference" error leading here, then it means something went wrong when instantiating the characters
        Vector3 smoothDelta = new Vector3();
        if (currentSpeed > 0)
        {
            smoothDelta = (Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime)) - transform.position;
        }

        Vector3 startingPoint = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(startingPoint, Vector3.down, out mDownHitInfo, 10f, MathHelper.BlockersAndGroundLayerMask) && Vector3.Distance(mDownHitInfo.point, startingPoint) > 0.1f)
        {
            Debug.DrawLine(startingPoint, mDownHitInfo.point, Color.red);
            smoothDelta -= Vector3.up * mDownHitInfo.distance; //Gravity is negative, hence the adding
            mDirection += Vector3.up * 0.02f; //Small offset so that quads or effects placed aroung the player won't be hidden beneath the floor

        }
    
            

        _CharacterController.Move(smoothDelta);
        mCurrentWalkRatio = walkLerpSpeed;
        mCurrentRunRatio = runLerpSpeed;
    }

    bool isHurtWhileJumping = false;
    float verticalVelocity = 0;
    private void JumpCheck()
    {

        //bool wantsToJump = Input.GetButtonDown("Jump");
        //bool keepsJumping = Input.GetButton("Jump");
        //if(_CanJump && wantsToJump)
        //{
        //    ChangeState(PlayerState.Jumping);
        //    _Rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.VelocityChange);
        //    verticalVelocity = m_JumpForce;
        //    Debug.Log("Wants to jump!");
        //}
        //if (_IsJumping && _CharacterController.isGrounded)
        //    ChangeState(PlayerState.Idle);
        //if (/*_IsJumping*/ false)
        //{
        //    Debug.Log("In the middle of the jump!");
        //    //If the player is grounded, back to idle, else fall down
        //    Vector3 startingPoint = transform.position + Vector3.up*0.05f;
        //    if (Physics.Raycast(startingPoint, Vector3.down, out mDownHitInfo, 50f, MathHelper.BlockersAndGroundLayerMask) && mDownHitInfo.distance < 0.1f)
        //    if(_CharacterController.isGrounded)
        //    {
        //        ChangeState(PlayerState.Idle);
        //    }
        //    else
        //    {
        //        //HorizontalMovement
        //        movementInput = new Vector2(Input.GetAxis("HorizontalMovement"), Input.GetAxis("VerticalMovement"));
        //        movementInput = movementInput.LimitMagnitude(1f);
        //        mDirection = CameraManager.RightDirection * movementInput.x +
        //        CameraManager.ForwardDirection * movementInput.y;

        //        mDirection *= movementInput.magnitude * m_AirSpeed* (isHurtWhileJumping ? 0 : 1)* Time.fixedDeltaTime;

        //        if (mDirection.magnitude > 0.2f)
        //            transform.rotation = Quaternion.Lerp(transform.rotation,
        //                Quaternion.LookRotation(mDirection), mRotationSpeed * Time.fixedDeltaTime * (1 - mCurrentSlowRatio));




        //        //VerticalMovement
        //        verticalVelocity += (keepsJumping ? 1 : 0) * m_VerticalInfluence * (isHurtWhileJumping?0:1)*Time.fixedDeltaTime;
        //        verticalVelocity -= m_Gravity*Time.fixedDeltaTime;
        //        mDirection += verticalVelocity * Time.fixedDeltaTime * Vector3.up;
        //        Debug.DrawLine(transform.position + mDirection, transform.position + mDirection + Vector3.up * 0.5f);
        //        if (Physics.Raycast(transform.position + mDirection, Vector3.up, out mDownHitInfo, 10f, MathHelper.GroundLayerMask))
        //        {
        //            Debug.Log("Went too far, so snap");
        //            mDirection.y -= mDownHitInfo.distance;
        //            ChangeState(PlayerState.Idle);
        //        }

        //        Vector3 newPos = (Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime));
        //        Vector3 smoothDelta = newPos - transform.position;
        //        _CharacterController.Move(smoothDelta);
        //    }
        //}



    }



    private bool isDashing;
    private float timeWhenDashed;
    private void DashCheck()
    {
        bool willDash = Input.GetButtonDown("Dash");


        if(willDash)
        {
            Vector3 dashInput = Input.GetAxis("HorizontalMovement") * CameraManager.RightDirection + Input.GetAxis("VerticalMovement")* CameraManager.ForwardDirection;
            if(dashInput.magnitude>0.2f)
            StartCoroutine(DashCoroutine(dashInput.normalized));
        }
    }

    private void AttackCheck()
    {
        if (Input.GetButtonDown("NormalAttack") && _CanAttack)
        {
            canAttackAgain = false;
            _Animator?.SetTrigger("NormalAttack");
            ChangeState(PlayerState.Attacking);
        }
    }

    public void ResetAttackCooldown()
    {
        canAttackAgain = true;
    }
    public void EnableSwordCollider(int value)
    {
        m_SwordCollider.Enabled(value==1?true:false);
    }
    public void DealDamage(Enemy enemy)
    {
        enemy.AddDamage(m_FirstAttackDamage);
        Debug.Log("Hit an enemy!");
    }
    public void ChangeToIdle()
    {
        ChangeState(PlayerState.Idle);
    }
    
    #endregion

    #region Interaction
    private void Slow(float amount)
    {
        mCurrentSlowRatio = amount;
        if (mHasAnimator) _Animator.SetBool("Slow", amount > 0);
    }

    private IEnumerator StunCoroutine(float stunTime = 0)
    {
        if (OnStun != null) OnStun();
        if (mHasAnimator) _Animator.SetTrigger("Fall");
        yield return new WaitForSeconds(stunTime == 0 ? 0 : mStunHitTime);
        if (mHasAnimator) _Animator.SetTrigger("GetUp");
        if (OnUnfreeze != null) OnUnfreeze();
    } 
    #endregion

    private IEnumerator DashCoroutine(Vector3 direction)
    {
        timeWhenDashed = Time.time;
        ChangeState(PlayerState.Dashing);
        Vector3 initialPos = transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 targetPos = transform.position + direction * m_DashDistance;
        float currentTime = 0;
        while (currentTime < m_DashTime)
        {
            float progress = m_DashPositionCurve.Evaluate(currentTime / m_DashTime);
            transform.position = Vector3.Lerp(initialPos, targetPos, progress);
            currentTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        ChangeState(PlayerState.Idle);
        
    }

}
