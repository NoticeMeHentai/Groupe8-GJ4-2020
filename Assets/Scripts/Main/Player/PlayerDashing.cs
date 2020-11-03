using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager : MonoBehaviour
{
    [Header("Dash")]
    [Min(0f)] public float m_DashTime = 0.5f;
    [Min(0f)] public float m_DashCooldown = 0.5f;
    [Min(0f)] public float m_DashDistance = 0.5f;
    [Range(0.5f, 1f)] public float m_DashCanBeStoppedAt = 0.8f;
    public AnimationCurve m_DashPositionCurve;


    private bool isDodging = false;
    private float timeWhenDashed;

    private void DashCheck()
    {
        bool willDash = Input.GetButtonDown("Dash");


        if (willDash)
        {
            Vector3 dashInput = Input.GetAxis("HorizontalMovement") * CameraManager.RightDirection + Input.GetAxis("VerticalMovement") * CameraManager.ForwardDirection;
            if (dashInput.magnitude > 0.2f)
                StartCoroutine(DashCoroutine(dashInput.normalized));
        }
    }

    private IEnumerator DashCoroutine(Vector3 direction)
    {
        isDodging = true;
        timeWhenDashed = Time.time;
        ChangeState(PlayerState.Dashing);
        Vector3 initialPos = transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 targetPos = transform.position + direction * m_DashDistance;
        float currentTime = 0;
        while (currentTime < m_DashTime)
        {
            float progress = m_DashPositionCurve.Evaluate(currentTime / m_DashTime);
            if (progress > m_DashCanBeStoppedAt)
            {
                if (Input.GetAxis("HorizontalMovement") != 0
                || Input.GetAxis("VerticalMovement") != 0
                || Input.GetButtonDown("Dash")
                || Input.GetButtonDown("NormalAttack")
                || Input.GetButtonDown("HeavyAttack"))
                {
                    isDodging = false;
                    ChangeState(PlayerState.Idle);
                    Update();
                    yield break;
                }
            }
            transform.position = Vector3.Lerp(initialPos, targetPos, progress);
            currentTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        isDodging = false;
        ChangeState(PlayerState.Idle);

    }
}
