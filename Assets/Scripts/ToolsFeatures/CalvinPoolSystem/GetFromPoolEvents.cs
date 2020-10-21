using UnityEngine;
using UnityEngine.Events;

public class GetFromPoolEvents : MonoBehaviour
{
    #region Public Variables
    //LerpMovementFromPool variables
    public bool lerpMovementFromPool;
    public float lerpSpeed = 1;
    public float lerpSpeedRandomMargin = 0.1f;
    public Vector3 lerpToTargetWorldPos;
    public float offsetRandomMargin;
    public bool useCurve;
    public AnimationCurve lerpSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);

    //AddForceMovementFromPool
    public bool addForceFromPool;
    public float strength = 100;
    public float strengthRandomMargin = 10;
    public Vector3 directionMultiplier;

    //AddRotationFromPool
    public bool addRotationFromPool;
    public float rotationSpeed = 1;
    public Vector3 axisMultiplier;

    //CallFunctionFromPool variables
    public bool callFunctionFromPool;
    public UnityEvent functionFromPool;

    #endregion

    #region Private Variables
    //LerpMovementFromPool variables
    Vector3 startingWorldPosition = new Vector3();
    float randomizedSpeed;
    float currentLerpPosition;
    float timer;

    //AddForceMovementFromPool
    float strengthRandomized;
    Rigidbody thisRigidBody;
    Quaternion originalOrientation;

    //AddRotationFromPool

    
    #endregion

    #region Unity Functions
    private void Awake()
    {
        thisRigidBody = gameObject.GetComponent<Rigidbody>();

        originalOrientation = transform.rotation;
    }

    private void OnEnable()
    {
        if (callFunctionFromPool)
            functionFromPool.Invoke();

        if (lerpMovementFromPool)
        {
            startingWorldPosition = transform.position;
            randomizedSpeed = lerpSpeed + Random.Range(-lerpSpeedRandomMargin, lerpSpeedRandomMargin);
            currentLerpPosition = 0;
            timer = 0;
        }
        if (addForceFromPool || addRotationFromPool)
            ResetRigidBody();

        if (addForceFromPool)
            AddForceFromPool();
    }
    private void Update()
    {
        if (lerpMovementFromPool && currentLerpPosition <= 0.98)
        {
            LerpMovementFromPool();
        }
        if (addRotationFromPool)
        {
            AddRotationFromPool();
        }
    }

    #endregion

    #region FromPool Functions

    /// <summary>
    /// Will lerp the gameObject from startPosition to endPosition with given speed balanced with given curve
    /// </summary>
    private void LerpMovementFromPool()
    {
        timer += Time.deltaTime * randomizedSpeed;
        if (useCurve == true)
            currentLerpPosition = lerpSpeedCurve.Evaluate(timer);
        else
            currentLerpPosition = timer;
        transform.position = Vector3.Lerp(startingWorldPosition, lerpToTargetWorldPos, currentLerpPosition);
    }

    /// <summary>
    /// Will Add force to the gameObject after resetting it's rigidbody with it's original properties for reuse, with the given strength on the given Axis
    /// </summary>
    private void AddForceFromPool()
    {
        if (strengthRandomMargin > 0)
            strengthRandomized = strength + Random.Range((-strengthRandomMargin), strengthRandomMargin);
        else
            strengthRandomized = strength;

        thisRigidBody.AddForce(transform.right * directionMultiplier.x * strengthRandomized);
        thisRigidBody.AddForce(transform.forward * directionMultiplier.z * strengthRandomized);
        thisRigidBody.AddForce(transform.up * directionMultiplier.y * strengthRandomized);
    }

    /// <summary>
    /// Will Add rotation every frame to the gameObject after resetting it's rigidbody with it's original properties
    /// </summary>
    private void AddRotationFromPool()
    {
        transform.Rotate(axisMultiplier * rotationSpeed);
    }

    /// <summary>
    /// Resets the rigidbody orientation and momentum to it's original state at Start()
    /// </summary>
    private void ResetRigidBody()
    {
        //transform.rotation = originalOrientation;
        thisRigidBody.ResetCenterOfMass();
        thisRigidBody.ResetInertiaTensor();
        thisRigidBody.velocity = Vector3.zero;
        thisRigidBody.angularVelocity = Vector3.zero;
    }
    #endregion
}
