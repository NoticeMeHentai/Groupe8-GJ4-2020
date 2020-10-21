using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolingTestsDELETE : MonoBehaviour
{
    public GameObject gameObjectPls;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            PoolingSystem.Preload(gameObjectPls, 10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            PoolingSystem.GetFromPool(gameObjectPls, transform.position, transform.rotation);
        }
        if (Input.GetKey(KeyCode.Z))
            transform.Translate(0.1f, 0, 0);
        if (Input.GetKey(KeyCode.Q))
            transform.Translate(0, 0, 0.1f);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(0, 0, -0.1f);
        if (Input.GetKey(KeyCode.S))
            transform.Translate(-0.1f, 0, 0);
    }
}
