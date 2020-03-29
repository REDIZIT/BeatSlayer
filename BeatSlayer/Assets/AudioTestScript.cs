using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTestScript : MonoBehaviour
{
    public AudioSource first, second;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            first.Play();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            second.Play();
        }
    }
}
