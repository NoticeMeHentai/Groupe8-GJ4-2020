using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float mMaxWalkSpeed = 10f;
    [Min(0f)] public float m_AirSpeed = 10f;
    [Min(0f)] public float mRunSpeedRatio = 2.5f;
    [Range(0.2f, 1f)] public float m_AccelerationTime = 0.5f;
    [Range(10f, 30f)] public float m_RotationSpeed = 30f;
    [Min(0f)] public float m_MaxHeightSnap = 0.15f;


    private float mCurrentWalkRatio = 0;
    private float mCurrentRunRatio = 0;
    private RaycastHit mDownHitInfo;
    private Vector3 mDirection;
    private float mCurrentSlowRatio = 0;


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
            walkLerpSpeed = MathHelper.HardIn(Mathf.Clamp(walkLerpSpeed + movementInput.sqrMagnitude * Time.deltaTime / m_AccelerationTime, 0f, movementInput.sqrMagnitude), FunctionsCurves.Xexp2);
            if (_IsRunning) runLerpSpeed = MathHelper.HardIn(Mathf.Clamp01(runLerpSpeed + Time.deltaTime / m_AccelerationTime), FunctionsCurves.Xexp3);
        }
        //If not giving any input and it's still slowing down, slow down
        else if (walkLerpSpeed > 0.05f)
        {
            walkLerpSpeed = MathHelper.EasyIn(Mathf.Clamp01(walkLerpSpeed - Time.deltaTime / m_AccelerationTime), FunctionsCurves.Xexp3);
            runLerpSpeed = 0;
            if (walkLerpSpeed <= 0.1f) walkLerpSpeed = 0f;
        }
        currentSpeed = 0;
        if (!_IsRunning)
            currentSpeed = walkLerpSpeed * (1 - mCurrentSlowRatio) * mMaxWalkSpeed;
        else if (_IsRunning)
        {
            float difference = (mRunSpeedRatio - 1f) * mMaxWalkSpeed; //To optimize
            currentSpeed = mMaxWalkSpeed + difference * mRunSpeedRatio;
        }

        float speedAnimatorParameter = (_IsRunning ? 0.5f + runLerpSpeed * 0.5f : walkLerpSpeed * 0.5f);
        _Animator?.SetFloat("Speed", speedAnimatorParameter);


        //Delta movement calculus
        mDirection = CameraManager.RightDirection * movementInput.x +
            CameraManager.ForwardDirection * movementInput.y;



        //Rotation
        if (mDirection.magnitude > 0.2f)
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(mDirection), m_RotationSpeed * Time.deltaTime * (1 - mCurrentSlowRatio));
        mDirection *= currentSpeed * Time.deltaTime;




        //If there's a "Object reference" error leading here, then it means something went wrong when instantiating the characters
        Vector3 smoothDelta = new Vector3();
        if (currentSpeed > 0)
        {
            smoothDelta = (Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime)) - transform.position;
        }

        float customSafeDistance = 0.1f;
        Vector3 startingPoint = transform.position + Vector3.up * customSafeDistance;
        if (Physics.Raycast(startingPoint, Vector3.down, out mDownHitInfo, 10f, MathHelper.BlockersAndGroundLayerMask) && Vector3.Distance(mDownHitInfo.point, startingPoint) > 0.05f)
        {
            smoothDelta -= Vector3.up * mDownHitInfo.distance; //Gravity is negative, hence the adding
            mDirection += Vector3.up * 0.02f; //Small offset so that quads or effects placed aroung the player won't be hidden beneath the floor
            Debug.DrawLine(startingPoint, mDownHitInfo.point, Color.red);
            if(mDownHitInfo.distance > m_MaxHeightSnap + customSafeDistance)
            {
                _CharacterController.Move(smoothDelta);
                Fall();
            }

        }



        _CharacterController.Move(smoothDelta);
        mCurrentWalkRatio = walkLerpSpeed;
        mCurrentRunRatio = runLerpSpeed;
    }

    private void Slow(float amount)
    {
        mCurrentSlowRatio = amount;
        if (mHasAnimator) _Animator.SetBool("Slow", amount > 0);
    }
}
