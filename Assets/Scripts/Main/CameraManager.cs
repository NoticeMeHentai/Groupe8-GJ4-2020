using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [Range(2f, 15f)] public float m_Distance = 15f;
    [Range(2f, 270f)] public float m_RotationSpeed = 45f;
    [Range(10f,30f)]public float m_LerpSpeed = 15f;
    [SerializeField] private bool m_FollowPlayers = true;
    public Vector3 m_FocusOffset = new Vector3();
    public Vector3 m_PosOffset = new Vector3();
    public Vector3 m_IdealAngle = new Vector3(135, 130, 180);
    [Tooltip("Transform used when resetting the camera pos/rot")][InstanceButton(typeof(CameraManager),nameof(ResetLocalCamera))]
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
        //As the reference point turns around the player, we too should tourn by the same degree
        //Store the Y angle like reference
        //A different point turns around the player, which will be our position reference, which then we will add
        //an offset, given the forward direction given by the stored Y angle reference (which varies while turning too)
        //Then just translate to the ref point, then apply the offset, then look at the focus point


        Vector3 defaultPos = PlayerMovement.Position
            + PlayerMovement.Forward * m_PosOffset.z
            + Vector3.up * m_PosOffset.y
            + PlayerMovement.Right * m_PosOffset.x;

        Vector3 focusPos = PlayerMovement.Position
            + PlayerMovement.Forward * m_FocusOffset.z
            + Vector3.up * m_FocusOffset.y
            + PlayerMovement.Right * m_FocusOffset.x;

        transform.position = defaultPos;
        Vector3 dir = focusPos - defaultPos;
        transform.rotation = Quaternion.LookRotation(dir.normalized);

        //float rotValue = m_RotationSpeed * Time.deltaTime * Input.GetAxis("HorizontalRotation");
        //if (rotValue != 0)
        //{
        //    transform.RotateAround(PlayerMovement.Position, Vector3.up, rotValue);
        //    mForward = transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
        //    mRight = transform.right;
        //}
    }

    public static void ResetCamera()
    {
        sInstance.transform.position = sInstance.mTarget.position;
        sInstance.transform.rotation = Quaternion.Euler(sInstance.m_IdealAngle);
        sInstance.transform.position -= sInstance.transform.forward * sInstance.m_Distance;
    }

    private void ResetLocalCamera()
    {
        transform.rotation = Quaternion.Euler(m_IdealAngle);
        Vector3 defaultPos = mTarget.position - transform.forward * m_Distance;
        Vector3 camPos = defaultPos + transform.right * m_PosOffset.x + Vector3.up * m_PosOffset.y + transform.forward * m_PosOffset.z;
        transform.position = camPos;
        Vector3 focusPos = defaultPos
            + mTarget.forward * m_FocusOffset.z
            + mTarget.right * m_FocusOffset.x
            + Vector3.up * m_FocusOffset.y;
        Vector3 dir = focusPos - transform.position;
        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }
    #endregion
}

