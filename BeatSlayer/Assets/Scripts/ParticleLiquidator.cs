using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLiquidator : MonoBehaviour
{
    ParticleSystem system;
    bool started;
    public bool isLeanObject;
    private void Start()
    {
        system = GetComponent<ParticleSystem>();
    }
    private void Update()
    {
        if (!system.isPlaying)
        {
            if (started)
            {
                if (isLeanObject)
                {
                    //Camera.main.GetComponent<GameScript>().lean.HideParticle(gameObject);
                    //started = false;
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            started = true;
        }
    }
}