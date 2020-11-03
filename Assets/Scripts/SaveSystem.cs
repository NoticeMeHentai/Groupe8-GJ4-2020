using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private void Awake()
    {
        GameManager.EventRestartCheckpoint += OnRetryCheckpoint;
        GameManager.EventRestartGame += OnRetryGame;
    }

    public static Action OnCheckpointSave;
    public static bool sHasReachedCheckpoint { get; private set; }


    private bool mHasAlreadyCheckpoint = false;
    public static void DoCheckpoint()
    {
        Debug.Log("Checkpoint done");
        OnCheckpointSave?.Invoke();
    }

    private void OnRetryGame()
    {
        mHasAlreadyCheckpoint = false;
        sHasReachedCheckpoint = false;
    }
    private void OnRetryCheckpoint()
    {

    }
}
