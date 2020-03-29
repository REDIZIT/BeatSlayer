using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatLineSphereAnim : MonoBehaviour
{
    public Vector3 rotation, targetRotation;
    public float threshold, targetThreshold;

    private void Start()
    {
        rotation = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        targetRotation = rotation;

        threshold = Random.Range(0.2f, 0.7f);
        targetThreshold = threshold;
    }

    private void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
        rotation += (targetRotation - rotation) / 100f;

        threshold += (targetThreshold - threshold) / 100f;
        GetComponent<MeshRenderer>().material.SetFloat("_Threshold", threshold);
        float offset = GetComponent<MeshRenderer>().material.GetFloat("_Offset");
        GetComponent<MeshRenderer>().material.SetFloat("_Offset", offset + Time.deltaTime);

        if (Random.value < 0.02f)
        {
            targetRotation = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
            targetThreshold = Random.Range(0.2f, 0.7f);
        }
    }
}