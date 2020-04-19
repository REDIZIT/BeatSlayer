using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// https://youtu.be/pi0UXxCQg3w?t=286
//

public class CheatEngine : MonoBehaviour
{
    public AudioSource asource;
    List<Bit> cubes = new List<Bit>();

    [Header("Cheats")]
    [Range(0, 10)] public float pitch;
    public bool keyboardControl;

    private void Update()
    {
        if (!Application.isEditor) return;

        if(asource.pitch != pitch)
        {
            asource.pitch = pitch;
        }

        if(cubes.Count > 0)
        {
            List<Bit> cubesToSlice = cubes.Where(c => c != null && Mathf.Abs(c.transform.position.z - transform.position.z) < 30).ToList();
            if (Input.GetKeyDown(KeyCode.E))
            {
                foreach (Bit cube in cubesToSlice.Where(c => c.transform.position.x == 1.25f))
                {
                    cube.SendBitSliced(Vector2.zero);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                foreach (Bit cube in cubesToSlice.Where(c => c.transform.position.x == -1.25f))
                {
                    cube.SendBitSliced(Vector2.zero);
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                foreach (Bit cube in cubesToSlice.Where(c => c.transform.position.x == -3.5f))
                {
                    cube.SendBitSliced(Vector2.zero);
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                foreach (Bit cube in cubesToSlice.Where(c => c.transform.position.x == 3.5f))
                {
                    cube.SendBitSliced(Vector2.zero);
                }
            }
        }
    }

    public void AddCube(Bit cube)
    {
        if (!Application.isEditor) return;

        cubes.Add(cube);
    }
}
