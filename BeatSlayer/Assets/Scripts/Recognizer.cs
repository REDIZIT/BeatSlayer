using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recognizer : MonoBehaviour
{
    public List<RecognizerSample> samples = new List<RecognizerSample>();

    public int displayIndex;

    public LineRenderer line;

    private void Start()
    {
        Debug.DrawLine(Vector3.zero, Vector3.forward);
    }
    private void Update()
    {
            
    }
    public void OnCubeSpawn(float[] spectrumData, BeatCubeClass.Type type)
    {
        samples.Add(new RecognizerSample(spectrumData, type));
        displayIndex = samples.Count - 1;
    }
    public bool b = true;
    public Vector3 offset;
    private void OnDrawGizmosSelected()
    {
        if (displayIndex < 0 || displayIndex >= samples.Count) return;

        RecognizerSample sample = samples[displayIndex];
        //line.positionCount = sample.spectrum.Length;
        for (int i = 0; i < sample.spectrum.Length - 1; i++)
        {
            //line.SetPosition(i, new Vector3(i, sample.spectrum[i]));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(offset + new Vector3(i / 50f, sample.spectrum[i], 0), offset + new Vector3((i + 1) / 50f, sample.spectrum[i + 1], 0));
        }

        float[] average = new float[64];
        float[] averageDir = new float[64];
        for (int i = 0; i < samples.Count; i++)
        {
            if(sample.type == BeatCubeClass.Type.Point)
            {
                for (int s = 0; s < samples[i].spectrum.Length; s++)
                {
                    average[s] += samples[i].spectrum[s];
                }
            }
            else if(sample.type == BeatCubeClass.Type.Dir)
            {
                for (int s = 0; s < samples[i].spectrum.Length; s++)
                {
                    averageDir[s] += samples[i].spectrum[s];
                }
            }
        }
        for (int i = 0; i < average.Length; i++)
        {
            average[i] /= samples.Count;
        }
        for (int i = 0; i < averageDir.Length; i++)
        {
            averageDir[i] /= samples.Count;
        }

        for (int i = 0; i < average.Length - 1; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(offset + new Vector3(i / 50f, average[i], 0), offset + new Vector3((i + 1) / 50f, average[i + 1], 0));
        }
        for (int i = 0; i < averageDir.Length - 1; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(offset + new Vector3(i / 50f, averageDir[i], 0), offset + new Vector3((i + 1) / 50f, averageDir[i + 1], 0));
        }
    }
}

public class RecognizerSample
{
    public float[] spectrum;
    public BeatCubeClass.Type type;

    public RecognizerSample(float[] spectrum, BeatCubeClass.Type type)
    {
        this.spectrum = spectrum;
        this.type = type;
    }
}