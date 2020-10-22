using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    [Min(0f)] public float m_Height = 1.5f;
    [Min(0f)]public float m_Distance = 5f;

    [Range(0f,60f)]public float m_LerpSpeed = 45f;

    private float targetAngle = 0;
    private Quaternion targetRot;

    public static Vector3 Position { get => sInstance.transform.position; 
        set { sInstance.transform.position = value; } }
    public static float Distance => sInstance.m_Distance;
    private float _RotationSpeed => CameraManager.RotationSpeed;
    public static float CurrentRotation => sInstance.targetAngle;
    private static CameraFocus sInstance;
    private void Awake()
    {
        sInstance = this;

        transform.position = PlayerMovement.Position - PlayerMovement.Forward * m_Distance + Vector3.up*m_Height;

    }

    private void Start()
    {
        targetAngle = 0;
    }
    private void Update()
    {
        float rotValue = _RotationSpeed * Time.deltaTime * Input.GetAxis("HorizontalRotation");
            targetAngle += rotValue;

        Vector3 targetPos = PlayerMovement.Position + Vector3.up * m_Height +Vector3.forward * m_Distance;
        targetPos = RotatePointAroundPivot(targetPos, PlayerMovement.Position, targetAngle * Vector3.up);
        transform.position = Vector3.Lerp(transform.position, targetPos, m_LerpSpeed * Time.deltaTime);
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(PlayerMovement.Position, Vector3.up, m_Distance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
