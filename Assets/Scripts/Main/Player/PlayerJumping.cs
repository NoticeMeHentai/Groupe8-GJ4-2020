﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager : MonoBehaviour
{
    [Header("Jump")]
    public float m_JumpForce = 15f;
    public float m_Gravity = 10f;
    public float m_VerticalInfluence = 5f;
    [Min(0f)] public float m_AirSpeed = 10f;
    [Min(0f)] public float m_JumpCooldown = 0.5f;
    [Min(0f)] public float m_MinAirTime = 0.1f;

    bool isHurtWhileJumping = false;
    bool isJumpCooldownOver = true;
    bool isMinAirOver = false;
    float verticalVelocity = 0;

    bool isIdlingOnTheAir = false;

    private bool _CanJump => _IsIdle && GameManager.sGameHasStarted && !StopActions && isJumpCooldownOver;
    private void JumpCheck()
    {

        bool wantsToJump = Input.GetButtonDown("Jump");
        bool keepsJumping = Input.GetButton("Jump");
        if (_CanJump && wantsToJump)
        {
            Debug.ClearDeveloperConsole();
            ChangeState(PlayerState.Jumping);
            verticalVelocity = m_JumpForce;
            Debug.Log("Wants to jump!");
            _Animator?.SetTrigger("Jump");
            _Animator.SetBool("JumpDown", false);
            StartCoroutine(MinAirCooldown());


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
            if (verticalVelocity < 0 && !isIdlingOnTheAir)
            {
                AnimatorStateInfo info = _Animator.GetCurrentAnimatorStateInfo(0);
                isIdlingOnTheAir = true;
                //When jumping, the animation goes directly to the idle looping mid air state
                //If it is already in there, then we don't need to tell the animator to go in there, as that'd send the animator to fall jump
                //if (!info.IsName(m_JumpAirStateName)) _Animator.SetTrigger("Jump");
                _Animator.SetBool("JumpDown",true);

            }
            //Debug.Log("In the middle of the jump! Current velocity: "+verticalVelocity);

            Debug.DrawLine(transform.position + mDirection, transform.position + mDirection + Vector3.up * 0.5f);

            Vector3 newPos = Vector3.Lerp(transform.position, transform.position + mDirection, 25f * Time.fixedDeltaTime);
            Vector3 smoothDelta = newPos - transform.position;
            _CharacterController.Move(smoothDelta);

            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
            if (isMinAirOver && Physics.Raycast(transform.position+Vector3.up*0.02f, Vector3.up, out mDownHitInfo, 5f, MathHelper.GroundLayerMask))
            {
                Debug.DrawLine(transform.position, mDownHitInfo.point, Color.red);
                Debug.Log("Hitting something");
                if (mDownHitInfo.collider.CompareTag("Ground"))
                {
                    transform.position = mDownHitInfo.point;
                    isIdlingOnTheAir = false;
                    _Animator.SetBool("JumpDown", true);
                    _Animator.SetTrigger("JumpSnapDown");
                    Debug.Log("HitGround!");
                    ChangeState(PlayerState.Idle);
                    StartCoroutine(JumpCooldown());
                }
            }
            else
                Debug.DrawLine(transform.position, transform.position + Vector3.up * 5f, Color.white);
        }
    }

    private void Fall()
    {
        Debug.Log("Started falling from Idle state!");
        verticalVelocity = 0;
        ChangeState(PlayerState.Jumping);
    }

    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if(_IsJumping && hit.collider.CompareTag("Ground"))
    //    {
    //        Debug.Log("HitGround!");
    //        ChangeState(PlayerState.Idle);
    //    }
    //}

    IEnumerator JumpCooldown()
    {
        isJumpCooldownOver = false;
        yield return new WaitForSeconds(m_JumpCooldown);
        isJumpCooldownOver = true;
    }

    IEnumerator MinAirCooldown()
    {
        isMinAirOver = false;
        yield return new WaitForSeconds(m_MinAirTime);
        isMinAirOver = true;

    }



}
