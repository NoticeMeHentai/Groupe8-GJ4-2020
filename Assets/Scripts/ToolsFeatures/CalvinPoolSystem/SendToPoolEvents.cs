using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SendToPoolEvents : MonoBehaviour
{
    #region Public Variables
    public bool isDebug;
    public bool onTimer;
    public bool onDistanceTraveled;
    public bool onCollision;
    public bool enableOtherGoOnDisable;
    public bool disableDelay;

    public float disableDelayInSeconds;
    public GameObject gameObjToEnable;

    //SendToPoolOnTimer variables
    public float timeBeforeDisable = 1;

    //SendToPoolOnDistance variables
    public float maxLengthToTravel;
    public float maxHeightToTravel;
    public float maxTotalDistanceToTravel;

    //SendToPoolOnCollision variables
    public bool onCollisionEnter;
    public bool onCollisionExit;

    //CallFunctionOnSend variables
    public bool callFunctionOnSend;
    public UnityEvent functionOnSend;

    public bool resetTransformPosition;
    #endregion

    #region Private Variables

    private bool isDisabling;

    private float timeSinceEnable;

    private float lengthTravelled;
    private float heightTravelled;
    private float totalDistance;
    private Vector3 startingPosition;

    #endregion

    #region Unity Functions;
    private void OnEnable()
    {
        isDisabling = false;
        startingPosition = transform.position;
        timeSinceEnable = 0;
    }

    private void Update()
    {
        if (!isDisabling) // check to see if a requirement has been met
        {
            if(onTimer)
                SendOnTimer();
            if (onDistanceTraveled)
                SendOnDistance();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (onCollision == true && onCollisionEnter == true)
            Disabling();
        if(isDebug)
            Debug.Log("Collided");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (onCollision && onCollisionEnter)
            Disabling();
    }

    #endregion

    #region SendToPoolFunctions
    /// <summary>
    /// Will wait for x seconds before calling Disabling() event
    /// </summary>
    private void SendOnTimer()
    {
        timeSinceEnable += Time.deltaTime;
        if (timeSinceEnable >= timeBeforeDisable)
        {
            Disabling();
        }
    }

    /// <summary>
    /// Will check how far the gameObject has travelled since enabling on xz, y and globally
    /// </summary>
    private void SendOnDistance()
    {
        lengthTravelled = (Mathf.Abs(transform.position.x) - Mathf.Abs(startingPosition.x)) + (Mathf.Abs(transform.position.z) - Mathf.Abs(startingPosition.z));
        if (lengthTravelled >= maxLengthToTravel && maxLengthToTravel > 0)
            Disabling();

        heightTravelled = Mathf.Abs(transform.position.y) - Mathf.Abs(startingPosition.y);
        if (heightTravelled >= maxHeightToTravel && maxHeightToTravel > 0)
            Disabling();

        totalDistance = lengthTravelled + heightTravelled;
        if (totalDistance >= maxTotalDistanceToTravel && maxTotalDistanceToTravel > 0)
            Disabling();
    }


    /// <summary>
    /// The global function that every other can call to disable the object with the required variables such as :
    /// spawning or not another gameObject, waiting for x seconds before disabling.
    /// </summary>
    private void Disabling()
    {
        isDisabling = true;

        if (callFunctionOnSend)
            functionOnSend.Invoke();

        if (enableOtherGoOnDisable)
        {
            try
            {
                PoolingSystem.GetFromPool(gameObjToEnable, transform.position, transform.rotation);
            }
            catch (UnassignedReferenceException)
            {
                Debug.LogWarning("You tried calling an object on disable with SendToPoolEvents but no GameObject was assigned to the script on the object " + gameObject.name);
            }
        }

        if (disableDelay)
            StartCoroutine(DisableDelay());
        else
        {
            if (resetTransformPosition)
                transform.position = startingPosition;
            PoolingSystem.SendToPool(gameObject);
        }
    }

    IEnumerator DisableDelay()
    {
        yield return new WaitForSeconds(disableDelayInSeconds);
        if (resetTransformPosition)
            transform.position = startingPosition;
        PoolingSystem.SendToPool(gameObject);
    }

    #endregion
}
