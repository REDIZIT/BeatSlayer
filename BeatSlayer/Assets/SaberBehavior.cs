using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberBehavior : MonoBehaviour
{
    public MeshRenderer[] coloredRenders;

    public void Init(Material coloredMaterial)
    {
        foreach(MeshRenderer renderer in coloredRenders)
        {
            renderer.material = coloredMaterial;
        }
    }

    public void SetEnabled(bool enabled)
    {
        GetComponent<MeshRenderer>().enabled = enabled;
    }
}
