using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [Range(2f, 15f)] public float m_Distance = 15f;
    [Range(2f, 270f)] public float m_RotationSpeed = 45f;
    [Range(10f,30f)]public float m_LerpSpeed = 15f;
    [SerializeField] private bool m_FollowPlayers = true;
    //public Vector3 m_FocusOffset = new Vector3();
    public Vector3 m_PosOffset = new Vector3();
    //public Vector3 m_IdealAngle = new Vector3(135, 130, 180);
    //[Tooltip("Transform used when resetting the camera pos/rot")][InstanceButton(typeof(CameraManager),nameof(ResetLocalCamera))]
    //public Transform mTarget;



    public bool mLockView = false;
    [NewLabel("Reset distance"), Tooltip("Distance at which the camera will be placed from the selected transform when resetting")]
    //[Min(5f)] public float mResetViewDistance = 15f;




    private Vector3 mPreviousPos;
    private static CameraManager sInstance;
    private Vector3 mRight = new Vector3();
    private Vector3 mForward = new Vector3();

    #region Properties
    //FollowPlayer

    private float _TargetAngle => CameraFocus.CurrentRotation + Mathf.PI*0.5f;
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

    public static float RotationSpeed => sInstance.m_RotationSpeed;


    #endregion

    #region Monobehaviour

    private void Awake()
    {
        sInstance = this;
        if (m_FollowPlayers)
        {
            LateUpdate();
            //transform.LookAt(PlayerMovement.Position);
        }
        mForward = transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
        mRight = transform.right;
    }
    private void Start()
    {
        transform.position = PlayerManager.Position + Vector3.forward* m_Distance + m_PosOffset;
        transform.LookAt(CameraFocus.Position);
        
    }
    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
    void LateUpdate()
    {
        //As the reference point turns around the player, we too should tourn by the same degree
        //Store the Y angle like reference
        //A different point turns around the player, which will be our position reference, which then we will add
        //an offset, given the forward direction given by the stored Y angle reference (which varies while turning too)
        //Then just translate to the ref point, then apply the offset, then look at the focus point

        if (Input.GetButtonDown("SwitchAngleSide")) m_PosOffset.x *= (-1);
        Vector3 defaultPos = PlayerManager.Position - Vector3.forward*m_Distance;
        defaultPos = RotatePointAroundPivot(defaultPos, PlayerManager.Position, _TargetAngle * Vector3.up);

        Vector3 dirTowardsPlayer = (PlayerManager.Position - defaultPos).FlatOneAxis(Vector3Extensions.Axis.y, true);
        Vector3 rightTowardsPlayer = Vector3.Cross(dirTowardsPlayer, Vector3.up).normalized;

        defaultPos+=dirTowardsPlayer * m_PosOffset.z
            + Vector3.up * m_PosOffset.y
            + rightTowardsPlayer * m_PosOffset.x;


        transform.position = Vector3.Lerp(transform.position, defaultPos, m_LerpSpeed*Time.deltaTime);

        transform.LookAt(CameraFocus.Position);

        mForward = transform.forward.FlatOneAxis(Vector3Extensions.Axis.y, true);
        mRight = transform.right.FlatOneAxis(Vector3Extensions.Axis.y, true);
    }


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(PlayerManager.Position, Vector3.up, m_Distance);


            Vector3 defaultPos = PlayerManager.Position - Vector3.forward * m_Distance;
            defaultPos = RotatePointAroundPivot(defaultPos, PlayerManager.Position, _TargetAngle * Vector3.up);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(defaultPos, 0.25f);

            Vector3 dirTowardsPlayer = (PlayerManager.Position - defaultPos).FlatOneAxis(Vector3Extensions.Axis.y, true);
            Vector3 rightTowardsPlayer = Vector3.Cross(dirTowardsPlayer, Vector3.up).normalized;
            Vector3 newPos = defaultPos
            + dirTowardsPlayer * m_PosOffset.z
                + Vector3.up * m_PosOffset.y
                + rightTowardsPlayer * m_PosOffset.x;

            Gizmos.DrawWireSphere(newPos, 0.25f);
            Gizmos.DrawLine(defaultPos, newPos);


        }
    } 
#endif
    #endregion
}

