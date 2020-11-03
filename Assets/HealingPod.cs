using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPod : MonoBehaviour
{
    public float m_HealingPerSecond = 5f;

    private bool isBeingTriggered = false;

    public bool HealEnabled { get; set; } = true;
    private ParticleSystem ps;

    private float currentTime = 0;
    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }
    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > 1)
        {
            currentTime -= 1;
            if (isBeingTriggered && HealEnabled)
            {
                ps.Play();
                PlayerManager.HealPlayer(m_HealingPerSecond);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentTime = 0;
            if (HealEnabled)
            {
                ps.Play();
                PlayerManager.HealPlayer(m_HealingPerSecond);
            }
            isBeingTriggered = true;
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isBeingTriggered = false;
    }
}
