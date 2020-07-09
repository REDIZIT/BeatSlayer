using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleLiquidator : MonoBehaviour
{
    ParticleSystem[] systems;
    bool started;
    public bool isLeanObject;
    private void Start()
    {
        systems = GetComponentsInChildren<ParticleSystem>();
    }
    private void Update()
    {
        if (systems.All(c => !c.isPlaying))
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