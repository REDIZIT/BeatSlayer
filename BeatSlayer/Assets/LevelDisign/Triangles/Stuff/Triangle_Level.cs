using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle_Level : MonoBehaviour
{
    GameManager gs;
    Camera cam;
    public GameObject[] triangles;

    public Transform waveParent;
    int waveElementsCount = 40;

    public Transform leftLights, rightLights;

    private void Awake()
    {
        gs = Camera.main.GetComponent<GameManager>();
    }

    private void Start()
    {
        // Инициализация волнового эффекта
        GameObject root = waveParent.GetChild(0).gameObject;

        for (int i = 1; i < waveElementsCount; i++)
        {
            GameObject el = Instantiate(root, waveParent);
            el.transform.localPosition = new Vector3(i * (1.08f - 0.5f), 0, 0);
            el.transform.localScale = new Vector3(0.5f, 0.2f, 1);

            GameObject el2 = Instantiate(root, waveParent);
            el2.transform.localPosition = new Vector3(-i * (1.08f - 0.5f), 0, 0);
            el.transform.localScale = new Vector3(0.5f, 0.2f, 1);
        }
        waveSamples = new float[waveElementsCount * 2 - 1];

        GameObject lightsRoot = leftLights.GetChild(0).gameObject;
        for (int i = 1; i < 30; i++)
        {
            GameObject el = Instantiate(lightsRoot, leftLights);
            el.transform.localPosition = new Vector3(0, 0, i * 1.75f * 8);

            GameObject el2 = Instantiate(lightsRoot, rightLights);
            el2.transform.localPosition = new Vector3(0, 0, i * 1.75f * 8);
        }
        lightShotsSamples = new float[30];
    }

    public bool doTest;
    private void Update()
    {
        Animate();
    }

    public GameObject rollingSquaresParent;
    public float[] rollingSquaresAngles;
    float rollingEmissionValue;



    float[] spectrumSamples = new float[64];
    float spectrumAmplitude = 0;
    float prevSpectrumAmplitude = 0;

    public Color redColor, blueColor, waveColor;

    public float[] trianglesAngle;


    public float[] waveSamples;

    public float[] lightShotsSamples;


    public void Animate()
    {
        gs.audioManager.asource.GetSpectrumData(spectrumSamples, 0, FFTWindow.Triangle);
        prevSpectrumAmplitude = spectrumAmplitude;

        spectrumAmplitude = 0;

        for (int i = 0; i < 64; i++) // FFTWindow.Triangle needs 64 samples (i think its minimum)
        {
            spectrumSamples[i] /= 2f; // Нужно делать на 2 т.к. я поднял громкость у AudioSource с 0.25 до 0.5

            spectrumAmplitude += spectrumSamples[i];
        }

        for (int i = 0; i < rollingSquaresParent.transform.childCount; i++)
        {
            Transform square = rollingSquaresParent.transform.GetChild(i);
            rollingEmissionValue += (spectrumAmplitude - rollingEmissionValue) / 100f;

            float targetAngle = rollingEmissionValue * i * 45f * (i % 2 == 0 ? 1 : -1);

            rollingSquaresAngles[i] += (targetAngle - rollingSquaresAngles[i]) / 100f;

            square.localEulerAngles = new Vector3(0, rollingSquaresAngles[i], 0);


            square.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", (i % 2 == 0 ? redColor : blueColor) * rollingEmissionValue * 10);
            square.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white * rollingEmissionValue * 20);

            //float plannedPos = Mathf.Abs(rollingSquaresAngles[i]);
            //square.localPosition = new Vector3(0, 0, plannedPos + 10 * i);
            //square.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(rollingEmissionValue * 30f, rollingEmissionValue * 30f, rollingEmissionValue * 30f));
        }



        for (int i = waveSamples.Length - 1; i > 0; i--)
        {
            waveSamples[i] = waveSamples[i - 1];
        }

        waveSamples[0] = spectrumAmplitude;
        waveParent.GetChild(0).localPosition = new Vector3(0, spectrumAmplitude * 8, 0);
        waveParent.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", waveColor * spectrumAmplitude  * 10f);

        for (int i = 1; i < waveSamples.Length - 1; i++)
        {
            float elColorCoef = waveSamples[i] * 5f;
            if (elColorCoef >= 1) elColorCoef *= 2;
            Color elColor = waveColor * elColorCoef;

            Transform el1 = waveParent.GetChild(i).transform;
            el1.localPosition = new Vector3(
                el1.localPosition.x,
                waveSamples[i] * 8,
                el1.localPosition.z);
            el1.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", elColor);

            Transform el2 = waveParent.GetChild(i + 1).transform;
            el2.localPosition = new Vector3(
                el2.localPosition.x,
                el1.localPosition.y,
                el2.localPosition.z);
            el2.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", elColor);
        }





        float amplitudeDiff = spectrumAmplitude - prevSpectrumAmplitude;

        for (int i = lightShotsSamples.Length - 1; i > 0; i--)
        {
            lightShotsSamples[i] = lightShotsSamples[i - 1] / 1.25f;
        }

        if(amplitudeDiff >= 0.13f)
        {
            lightShotsSamples[0] = amplitudeDiff;
        }
        else
        {
            lightShotsSamples[0] = 0;
        }
        //if (amplitudeDiff >= .13f)
        //{
        //    lightShotsSamples[0] = amplitudeDiff;
        //    leftLights.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", amplitudeDiff * Color.white * 5f);
        //    leftLights.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", amplitudeDiff * Color.white * 5f);
        //    rightLights.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", amplitudeDiff * Color.white * 5f);
        //    rightLights.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", amplitudeDiff * Color.white * 5f);
        //}

        for (int i = 0; i < lightShotsSamples.Length; i++)
        {
            Transform el1 = leftLights.GetChild(i).transform;
            el1.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * lightShotsSamples[i / 2] * 20f);
            el1.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * lightShotsSamples[i / 2] * 20f);

            Transform el2 = rightLights.GetChild(i).transform;
            el2.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * lightShotsSamples[i / 2] * 20f);
            el2.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * lightShotsSamples[i / 2] * 20f);
        }
    }
}
