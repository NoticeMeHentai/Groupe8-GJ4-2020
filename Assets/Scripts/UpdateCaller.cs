using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//https://forum.unity.com/threads/how-to-call-update-from-a-class-thats-not-inheriting-from-monobehaviour.451954/
public class UpdateCaller : MonoBehaviour
{
    private static UpdateCaller instance;
    public static void AddUpdateCallback(Action updateMethod)
    {
        if (instance == null)
        {
            
            instance = new GameObject("[Update Caller]").AddComponent<UpdateCaller>();
        }
        instance.updateCallback += updateMethod;
    }

    private Action updateCallback;

    private void Update()
    {
        updateCallback();
    }
}
