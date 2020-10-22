using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPod : MonoBehaviour
{
    public float m_HealingPerSecond = 5f;

    private bool isBeingTriggered = true;

    public bool HealEnabled { get; set; } = true;

    private float currentTime = 0;
    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > 1)
        {
            currentTime -= 1;
            if (isBeingTriggered && HealEnabled) GameManager.HealPlayer(m_HealingPerSecond);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isBeingTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isBeingTriggered = false;
    }
}
