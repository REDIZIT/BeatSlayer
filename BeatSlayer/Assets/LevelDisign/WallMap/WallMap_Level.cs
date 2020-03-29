using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMap_Level : MonoBehaviour
{
    Camera cam;
    //GameScript gs;
    GameManager gs;


    public Transform rightSpectrum, leftSpectrum;
    public float spectrumMultiply, cubeSize;

    public Transform columns;

    private void Awake()
    {
        cam = Camera.main;
        //gs = cam.GetComponent<GameScript>();
        gs = cam.GetComponent<GameManager>();
    }

    private void Start()
    {
        GameObject prefab = rightSpectrum.GetChild(0).gameObject;
        for (int i = 0; i < 64; i++)
        {
            GameObject cube = Instantiate(prefab, rightSpectrum);
            cube.transform.localPosition = new Vector3(0, 0, i * 1.5f);
        }
        Destroy(prefab);

        prefab = leftSpectrum.GetChild(0).gameObject;
        for (int i = 0; i < 64; i++)
        {
            GameObject cube = Instantiate(prefab, leftSpectrum);
            cube.transform.localPosition = new Vector3(0, 0, i * 1.5f);
        }
        Destroy(prefab);



        prefab = columns.GetChild(0).gameObject;
        for (int i = 0; i < 6; i++)
        {
            GameObject cube = Instantiate(prefab, columns);
            cube.transform.localPosition = new Vector3(0, 0, i == 0 ? 0 : Mathf.Clamp(i * i * 8, 10, 999));

            cube.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, -0.6f, 0);
            cube.transform.GetChild(1).GetChild(0).transform.localPosition = new Vector3(0, -0.6f, 0);
            cube.transform.GetChild(2).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
            cube.transform.GetChild(3).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        }
        Destroy(prefab);
    }

    private void Update()
    {
        Animate();
    }

    float[] spectrumSamples = new float[64];
    float spectrumAmplitude = 0;
    float prevSpectrumAmplitude = 0;


    public void Animate()
    {
        gs.audioManager.asource.GetSpectrumData(spectrumSamples, 0, FFTWindow.Triangle);
        prevSpectrumAmplitude = spectrumAmplitude;
        spectrumAmplitude = 0;
        float maxSpectrumSample = 0;

        for (int i = 0; i < 64; i++) // FFTWindow.Triangle needs 64 samples (i think its minimum)
        {
            spectrumSamples[i] /= 2f; // Нужно делать на 2 т.к. я поднял громкость у AudioSource с 0.25 до 0.5
            if (spectrumSamples[i] > maxSpectrumSample) maxSpectrumSample = spectrumSamples[i];

            spectrumAmplitude += spectrumSamples[i];
        }

        AnimateSpectrum(maxSpectrumSample);

        AnimateGates();
    }

    void AnimateSpectrum(float maxSpectrumSample)
    {
        Color spectrumColor = new Color(1, 0.15f, 0);
        for (int i = 0; i < 64; i++)
        {
            Transform cube = rightSpectrum.GetChild(i).transform;
            Transform leftCube = leftSpectrum.GetChild(i).transform;

            float normalized = maxSpectrumSample == 0 ? 0 : spectrumSamples[i] / maxSpectrumSample;
            float prevY = cube.localScale.y;


            cube.localScale = new Vector3(0.7f, (prevY + normalized * cubeSize) / 2f, 0.7f);
            leftCube.localScale = cube.localScale;

            Color prevColor = cube.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
            Color color = (prevColor + spectrumColor * Mathf.Clamp(Mathf.Pow(spectrumSamples[i] * 20, 2), 0, 2)) / 2f;
            cube.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
            leftCube.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
        }
    }

    void AnimateGates()
    {
        for (int i = 0; i < columns.childCount; i++)
        {
            Transform gate = columns.GetChild(i);
            Transform rightHand = gate.GetChild(0).GetChild(0);
            Transform leftHand = gate.GetChild(1).GetChild(0);
            Transform floorLine = gate.GetChild(2);
            Transform roofLine = gate.GetChild(3);

            float startY = -0.6f;
            float prevY = rightHand.localPosition.y;
            rightHand.localPosition = new Vector3(0, (prevY + startY + spectrumAmplitude) / 2f, 0);
            leftHand.localPosition = rightHand.localPosition;

            Color lineColor;
            if(spectrumAmplitude - prevSpectrumAmplitude > 0.15f * (i + 1))
            {
                lineColor = new Color(0, 0.5f, 1) * 1.5f;
                
            }
            else
            {
                lineColor = floorLine.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor") * 0.95f;
            }
            floorLine.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", lineColor);
            roofLine.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", lineColor);
        }
    }
}
