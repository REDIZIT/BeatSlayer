using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointScript : MonoBehaviour
{
    public float cooldown, pitch;
    public int road;

    public void Spawn(BeatCubeClass.Type type)
    {
        cooldown += 0.3f;
        if(type == BeatCubeClass.Type.Dir)
        {
            cooldown *= 2;
        }

        GetComponent<Animator>().Play("Effect");
    }
    public void Setup(int road, float pitch)
    {
        this.road = road;
        this.pitch = pitch;
    }
    public void Update()
    {
        if(cooldown > 0)
        {
            cooldown -= Time.deltaTime * pitch;
        }
    }
}
