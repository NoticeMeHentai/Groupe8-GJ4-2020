using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    private void Awake()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        Destroy(gameObject, main.duration);
    }
}
