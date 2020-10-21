using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [Range(2f, 50f)] public float m_Distance = 15f;
    [Range(2f, 270f)] public float m_RotationSpeed = 45f;
    [Range(10f,30f)]public float m_LerpSpeed = 15f;
    [SerializeField] private bool m_FollowPlayers = true;
    public Vector3 m_IdealAngle = new Vector3(135, 130, 180);
    [Tooltip("Transform used when resetting the camera pos/rot")][InstanceButton(typeof(CameraManager),nameof(ResetCamera))]
    public Transform mTarget;



    public bool mLockView = false;
    [NewLabel("Reset distance"), Tooltip("Distance at which the camera will be placed from the selected transform when resetting")]
    [Min(5f)] public float mResetViewDistance = 15f;




    private Vector3 mPreviousPos;
    private static CameraManager sInstance;
    private Vector3 mRight = new Vector3();
    private Vector3 mForward = new Vector3();

    #region Properties
    //FollowPlayer
    public static bool FollowPlayers { get { if(sInstance!=null)return sInstance.m_FollowPlayers; return false; } set { sInstance.m_FollowPlayers = value; } }
    public static Transform CameraTransform => sInstance.transform;



    private Camera mCameraComponent;
    public static Camera CameraComponent { get { if (sInstance.mCameraComponent == null) sInstance.mCameraComponent = sInstance.GetComponentInChildren<Camera>(); return sInstance.mCameraComponent; } }


    public static Vector3 RightDirection { get { if (sInstance == null) return Camera.main.transform.right; return sInstance.mRight; } }
    public static Vector3 ForwardDirection { 
        get {
            if (sInstance == null)
                return Camera.main.transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
            return sInstance.mForward; } 
    }


    #endregion

    #region Monobehaviour

    private void Awake()
    {
        sInstance = this;
        if (m_FollowPlayers)
        {
            transform.rotation = Quaternion.Euler(m_IdealAngle);
            LateUpdate();
            //transform.LookAt(PlayerMovement.Position);
        }
        mForward = transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
        mRight = transform.right;
    }

    void LateUpdate()
    {
        Vector3 newPos = PlayerMovement.Position - transform.forward * m_Distance;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * m_LerpSpeed);

        float rotValue = m_RotationSpeed * Time.deltaTime * Input.GetAxis("HorizontalRotation");
        if (rotValue != 0)
        {
            transform.RotateAround(PlayerMovement.Position, Vector3.up, rotValue);
            mForward = transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
            mRight = transform.right;
        }
    }

    public static void ResetCamera()
    {
        sInstance.transform.position = sInstance.mTarget.position;
        sInstance.transform.rotation = Quaternion.Euler(sInstance.m_IdealAngle);
        sInstance.transform.position -= sInstance.transform.forward * sInstance.m_Distance;
    }
    #endregion
}

