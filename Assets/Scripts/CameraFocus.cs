using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public Transform m_PointReference;
    [Min(0f)] public float m_Height = 1.5f;
    [Min(0f)]public float m_Distance = 5f;
    [Range(0f,180f)] float m_RotationSpeed = 60;
    [Range(0f,60f)]public float m_LerpSpeed = 45f;

    private float targetAngle;
    private Quaternion targetRot;

    public static Vector3 Position = sInstance.transform.position;

    private static CameraFocus sInstance;
    private void Awake()
    {
        sInstance = this;
    }

    private void Start()
    {
        targetAngle = transform.eulerAngles.y;
    }
    private void Update()
    {
        float rotValue = m_RotationSpeed * Time.deltaTime * Input.GetAxis("HorizontalRotation");
        if (rotValue != 0)
        {
            targetAngle += rotValue;
            targetRot = Quaternion.Euler(0,targetAngle,  0);


        }
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, m_LerpSpeed * Time.deltaTime);
        Vector3 targetPos = PlayerMovement.Position + Vector3.up * m_Height - transform.forward * m_Distance;
        transform.position = Vector3.Lerp(transform.position, targetPos, m_LerpSpeed * Time.deltaTime);
    }
}
