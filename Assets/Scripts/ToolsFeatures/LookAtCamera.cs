using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    //public float m_LerpSpeed = 45f;
    public bool m_Reverse = false;

    private void Update()
    {
        Vector3 dir = CameraManager.CameraTransform.forward*(-1);
        
        transform.rotation = Quaternion.LookRotation(dir * (m_Reverse ? 1 : -1));
    }
}
