using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager : MonoBehaviour
{
    [Header("Jump")]
    public float m_JumpForce = 15f;
    public float m_Gravity = 10f;
    public float m_VerticalInfluence = 5f;

    bool isHurtWhileJumping = false;
    float verticalVelocity = 0;
    bool hasTouchedTheGround = false;


    private void JumpCheck()
    {
        bool wantsToJump = Input.GetButtonDown("Jump");
        bool keepsJumping = Input.GetButton("Jump");
        if (_CanJump && wantsToJump)
        {
            ChangeState(PlayerState.Jumping);
            verticalVelocity = m_JumpForce;
            Debug.Log("Wants to jump!");
        }
        if (_IsJumping)
        {
            
            //HorizontalMovement
            movementInput = new Vector2(Input.GetAxis("HorizontalMovement"), Input.GetAxis("VerticalMovement"));
            movementInput = movementInput.LimitMagnitude(1f);
            mDirection = CameraManager.RightDirection * movementInput.x +
            CameraManager.ForwardDirection * movementInput.y;

            mDirection *= movementInput.magnitude * m_AirSpeed * (isHurtWhileJumping ? 0 : 1) * Time.deltaTime;

            if (mDirection.magnitude > 0.2f)
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(mDirection), m_RotationSpeed * Time.deltaTime * (1 - mCurrentSlowRatio));




            //VerticalMovement
            verticalVelocity += (keepsJumping ? 1 : 0) * m_VerticalInfluence * (isHurtWhileJumping ? 0 : 1) * Time.deltaTime;
            verticalVelocity -= m_Gravity * Time.deltaTime;
            //Debug.Log("In the middle of the jump! Current velocity: "+verticalVelocity);

            Debug.DrawLine(transform.position + mDirection, transform.position + mDirection + Vector3.up * 0.5f);

            Vector3 newPos = Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime);
            Vector3 smoothDelta = newPos - transform.position;
            _CharacterController.Move(smoothDelta);
            //_Rigidbody.MovePosition(transform.position + Vector3.up * verticalVelocity * Time.fixedDeltaTime);
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
            if (Physics.Raycast(transform.position, Vector3.up, out mDownHitInfo, 5f, MathHelper.GroundLayerMask))
            {
                Debug.DrawLine(transform.position, mDownHitInfo.point, Color.red);
                Debug.Log("Hitting something");
                if (mDownHitInfo.collider.CompareTag("Ground"))
                {
                    transform.position = mDownHitInfo.point;
                    Debug.Log("HitGround!");
                    ChangeState(PlayerState.Idle);
                }
            }
            else
                Debug.DrawLine(transform.position, transform.position + Vector3.up * 5f, Color.white);
        }
    }

    private void Fall()
    {
        verticalVelocity = 0;
        ChangeState(PlayerState.Jumping);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(_IsJumping && hit.collider.CompareTag("Ground"))
        {
            Debug.Log("HitGround!");
            ChangeState(PlayerState.Idle);
        }
    }




}
