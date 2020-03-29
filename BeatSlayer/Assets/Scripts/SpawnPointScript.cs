using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointScript : MonoBehaviour
{
    public float cooldown;
    public float pitch;

    public void Spawn(BeatCubeClass.Type type)
    {
        cooldown += 0.3f;
        if(type == BeatCubeClass.Type.Dir)
        {
            cooldown *= 2;
        }

        GetComponent<Animator>().Play("Effect");
    }
    private void Start()
    {
        //pitch = Camera.main.GetComponent<GameScript>().asource.pitch;
        pitch = 1;
    }
    public void Update()
    {
        if(cooldown > 0)
        {
            cooldown -= Time.deltaTime * pitch;
        }
    }
}
