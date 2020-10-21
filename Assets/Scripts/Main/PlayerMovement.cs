using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{

    #region Exposed variables
    [Header("Movement")]
    [Min(0f)]public float mMaxWalkSpeed = 10f;
    [Min(0f)] public float mRunSpeedRatio = 2.5f;
    [Range(0.2f,1f)]public float mAccelerationTime = 0.5f;
    [Range(10f,30f)]public float mRotationSpeed = 30f;
    [Header("Obstacles")]
    public float mStunTime = 1.5f;
    [Range(0f, 1f)] public float mSlowIntensity = 0.25f;
    public AnimationCurve mMudSlownessCurve = new AnimationCurve();
    [Header("Components")]
    public bool mHasAnimator = false;

    #endregion

    #region Private/Local variables
    private float mCurrentWalkRatio = 0;
    private float mCurrentRunRatio = 0;
    private bool mIsFrozen = false;
    /// The pack of information I'll get when raycasting down 
    private RaycastHit mDownHitInfo;
    /// The direction the player is currently looking at
    private Vector3 mDirection;
    private float mMaxRunSpeed = 0;
    private float mCurrentSlowRatio = 0;





    #endregion

    #region Properties


    private CharacterController mCharacterController;
    private CharacterController _CharacterController { get { if (mCharacterController == null) mCharacterController = GetComponentInChildren<CharacterController>(); return mCharacterController; } }

    private static PlayerMovement sInstance;
    public static Vector3 Position => sInstance.transform.position;


    private Rigidbody mRigidbody;
    private Rigidbody _Rigidbody { get { if (mRigidbody == null) mRigidbody = GetComponentInChildren<Rigidbody>(); return mRigidbody; } }

    
    private Animator mAnimator;
    private Animator _Animator { get { if (mAnimator == null) mAnimator = GetComponentInChildren<Animator>(); return mAnimator; } }

    public static Action OnStun;
    public static Action OnUnfreeze;

    private bool _CanMove => !mIsFrozen && GameManager.sGameHasStarted;
    private bool _IsSlowed => mCurrentSlowRatio > 0;
    private bool _IsRunning => _CanMove && !_IsSlowed && Input.GetAxis("Run") > 0.1f;

    #endregion


    #region MonoBehaviour
    private void Awake()
    {
        mCurrentSlowRatio = 1f;

        GameManager.OnGameReady += delegate { mCurrentSlowRatio = 0f; };
        GameManager.OnGameOverTimeRanOut += delegate { mIsFrozen = true; };
        GameManager.OnGameOverNoLivesLeft += delegate { mIsFrozen = true; };
        GameManager.OnRestart += delegate { mCurrentSlowRatio = 0f; mIsFrozen = false; };
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

        OnStun += delegate { mIsFrozen = true;  Invoke(nameof(Unfreeze), mStunTime); };
        OnUnfreeze += delegate { mIsFrozen = false; };
    }



    private void Update()
    {

        if (_CanMove)
        {
            MoveAndRotate();
        }

    }

    private void FixedUpdate()
    {
        Shader.SetGlobalVector("PlayerPosition", transform.position);
    }

    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (hit.collider.CompareTag("Rock") && !mIsFrozen && !mIsInMud && (Input.GetAxis("Run") > 0.2f)) Fall();
    //}

    
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

        float speedAnimatorParameter = (_IsRunning? 0.5f + mRunSpeedRatio*0.5f : walkLerpSpeed*0.5f);
        if(mHasAnimator)_Animator.SetFloat("Speed", speedAnimatorParameter);


        //Delta movement calculus
        mDirection = CameraManager.RightDirection * movementInput.x + 
            CameraManager.ForwardDirection * movementInput.y;



        //Rotation
        if (mDirection.magnitude > 0.2f)
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(mDirection), mRotationSpeed * Time.deltaTime*(1 - mCurrentSlowRatio));
        mDirection *= currentSpeed * Time.deltaTime;




        //If there's a "Object reference" error leading here, then it means something went wrong when instantiating the characters
        Vector3 smoothDelta = new Vector3();
        if (currentSpeed > 0)
        {
            smoothDelta = (Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime)) - transform.position;
        }
        Vector3 startingPoint = transform.position - Vector3.up * 0.4f;
        if (Physics.Raycast(startingPoint, Vector3.down, out mDownHitInfo, 50f, MathHelper.BlockersAndGroundLayerMask) && Vector3.Distance(mDownHitInfo.point, startingPoint) > 0.02f)
        {
            
            Debug.DrawLine(startingPoint, mDownHitInfo.point, Color.red);

            smoothDelta -= Vector3.up * mDownHitInfo.distance; //Gravity is negative, hence the adding
            mDirection += Vector3.up * 0.02f; //Small offset so that quads or effects placed aroung the player won't be hidden beneath the floor
        }
        _CharacterController.Move(smoothDelta);
        mCurrentWalkRatio = walkLerpSpeed;
        mCurrentRunRatio = runLerpSpeed;
    }

    
    #endregion

    #region Interaction
    private void Slow(float amount)
    {
        mCurrentSlowRatio = amount;
        if (mHasAnimator) _Animator.SetBool("Slow", amount > 0);
    }
    private void Unfreeze()
    {
        if (mHasAnimator) _Animator.SetTrigger("GetUp");
        mIsFrozen = false;
    }
    public static void Stun(float stunTime = 0)
    {
        if (sInstance.mHasAnimator) sInstance._Animator.SetTrigger("Hit");
        sInstance.StartCoroutine(sInstance.StunCoroutine(stunTime));
        if (OnStun != null) OnStun();
    }

    private IEnumerator StunCoroutine(float stunTime = 0)
    {
        if (OnStun != null) OnStun();
        if (mHasAnimator) _Animator.SetTrigger("Fall");
        yield return new WaitForSeconds(stunTime == 0 ? 0 : mStunTime);
        if (mHasAnimator) _Animator.SetTrigger("GetUp");
        if (OnUnfreeze != null) OnUnfreeze();
    } 
    #endregion
}
