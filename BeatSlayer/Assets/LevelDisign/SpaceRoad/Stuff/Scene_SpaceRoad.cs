using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_SpaceRoad : MonoBehaviour
{
    GameObject cam;

    public GameObject spectrumVisualPrefab;
    public GameObject[] spectrumVisualArrays;
    public GameObject kickVisualParent;
    List<MeshRenderer> kickVisualAll = new List<MeshRenderer>();
    float[] spectrumVisualScale = new float[64];
    float[] spectrumVisualScaleDecrease = new float[64];

    float[] spectrumSamples = new float[64];
    float spectrumAmplitude = 0;

    // ===================================================================================
    //  Light beams
    [Header("Light Beams")]
    public Transform[] lightBeams;
    public Color lightBeamColor;
    float lightBeamValue, lightBeamDecrease;

    // ===================================================================================
    //  Light shots
    [Header("Light shots")]
    float lightShotValue, lightShotDecrease;

    // ===================================================================================
    //  Rolling Stuff
    [Header("Rolling Stuff")]
    public GameObject rollingSquaresParent;
    public float[] rollingSquaresAngles;
    float rollingEmissionValue;

    private void Start()
    {
        cam = Camera.main.gameObject;
        rollingSquaresAngles = new float[rollingSquaresParent.transform.childCount];

        for (int i = 0; i < kickVisualParent.transform.childCount; i++)
        {
            for (int u = 0; u < kickVisualParent.transform.GetChild(i).childCount; u++)
            {
                kickVisualAll.Add(kickVisualParent.transform.GetChild(i).GetChild(u).GetChild(0).GetComponent<MeshRenderer>());
            }
        }

        for (int i = 0; i < 2; i++)
        {
            for (int o = 0; o < 64; o++)
            {
                GameObject c = Instantiate(spectrumVisualPrefab, spectrumVisualArrays[i].transform);
                c.transform.localPosition = new Vector3(0, -5, o * 2);
                spectrumVisualScaleDecrease[i] = 1;
            }
        }
    }

    private void Update()
    {
        Animate();
    }

    float prevSpectrumAmplitude = 0;
    //float averageSpectrumVolume = 0;
    int squareRollingDir = 1;
    float squareRollingDirBlock = 5;
    List<float> averageSpectrumVolumeSamples = new List<float>();
    List<float> smoothSamples = new List<float>();
    public void Animate()
    {
        cam.GetComponent<GameManager>().audioManager.asource.GetSpectrumData(spectrumSamples, 0, FFTWindow.Triangle);
        prevSpectrumAmplitude = spectrumAmplitude;
        spectrumAmplitude = 0;

        for (int i = 0; i < 64; i++) // FFTWindow.Triangle needs 64 samples (i think its minimum)
        {
            spectrumSamples[i] /= 2f; // Нужно делать на 2 т.к. я поднял громкость у AudioSource с 0.25 до 0.5

            spectrumAmplitude += spectrumSamples[i];

            if(spectrumSamples[i] > spectrumVisualScale[i])
            {
                spectrumVisualScale[i] = spectrumSamples[i];
                spectrumVisualScaleDecrease[i] = 1.005f;
            }
            else
            {
                spectrumVisualScale[i] *= spectrumVisualScaleDecrease[i];
                spectrumVisualScaleDecrease[i] *= 0.9f;
            }
            


            spectrumVisualArrays[0].transform.GetChild(i + 1).transform.localScale = new Vector3(1, 10 + spectrumVisualScale[i] * 100, 1);
            spectrumVisualArrays[1].transform.GetChild(i + 1).transform.localScale = new Vector3(1, 10 + spectrumVisualScale[i] * 100, 1);
        }



        averageSpectrumVolumeSamples.Add(spectrumAmplitude);
        int needToAverage = Mathf.CeilToInt(1f / Time.deltaTime);

        if (averageSpectrumVolumeSamples.Count < needToAverage) needToAverage = averageSpectrumVolumeSamples.Count;

        float smooth = 0;
        for (int i = 0; i < needToAverage; i++)
        {
            smooth += averageSpectrumVolumeSamples[averageSpectrumVolumeSamples.Count - i - 1];
        }
        smoothSamples.Add(smooth);


        #region Через средний значени за секунду и 2-4 секунды
        /* 

        oneSecVolume /= needToAverage;
         needToAverage *= 2;
         if (averageSpectrumVolumeSamples.Count < needToAverage) needToAverage = averageSpectrumVolumeSamples.Count;
         float twoSecVolume = 0;
         for (int i = Mathf.RoundToInt(needToAverage / 2f); i < needToAverage; i++)
         {
             twoSecVolume += averageSpectrumVolumeSamples[averageSpectrumVolumeSamples.Count - i - 1];
         }
         twoSecVolume /= needToAverage;

         squareRollingDirBlock -= Time.deltaTime;
         if (oneSecVolume - twoSecVolume >= 0.108f)
         {
             if(squareRollingDirBlock <= 0)
             {
                 Debug.Log("Was " + squareRollingDir + " become " + -squareRollingDir);
                 squareRollingDir = -squareRollingDir;
                 squareRollingDirBlock = 5;
             }
         }*/
        #endregion


        float minH = smooth;
        float curH = smooth;
        int backCount = Mathf.RoundToInt(1f / Time.deltaTime);

        if (backCount > smoothSamples.Count) backCount = smoothSamples.Count;
        for (int i = 0; i < backCount; i++)
        {
            float cur = smoothSamples[smoothSamples.Count - i - 1];
            if (cur < minH)
            {
                minH = cur;
            }
        }
        
        float diff = curH - minH;
        squareRollingDirBlock -= Time.deltaTime;
        if (diff >= 4.75f)
        {
            if(squareRollingDirBlock <= 0)
            {
                squareRollingDir = -squareRollingDir;
                squareRollingDirBlock = 5;
            }
        }


        float spectrumKick = spectrumAmplitude - prevSpectrumAmplitude;
        // ========================================================================================================
        //  Light Beams
        if(spectrumKick > 0.19f) {
            lightBeamDecrease = 0.05f;
            lightBeamValue = spectrumKick * 5f;
        }
        else {
            lightBeamValue -= lightBeamDecrease;
            lightBeamDecrease *= 1.01f;
        }
        if (lightBeamValue < 0) lightBeamValue = 0;
        Color32 color = new Color32(0, 0, 0, (byte)(255 * (lightBeamValue > 1 ? 1 : lightBeamValue)));
        foreach (Transform beamParent in lightBeams)
        {
            beamParent.GetComponent<Animator>().speed = lightBeamValue * 1.5f + 0.25f;
            foreach (Transform beam in beamParent)
            {
                beam.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            }
        }

        // ========================================================================================================================================================
        //  Light Shots
        for (int i = 0; i < kickVisualAll.Count; i++)
        {
            if(spectrumKick > 0.135f)
            {
                lightShotDecrease = 0.002f;
                lightShotValue = 6;
            }
            else
            {
                lightShotValue -= lightShotDecrease;
                lightShotDecrease *= 1.002f;
            }
            if (lightShotValue < 0) lightShotValue = 0;
            kickVisualAll[i].material.SetColor("_EmissionColor", new Color(lightShotValue, lightShotValue, lightShotValue));
        }

        // ========================================================================================================================================================
        //  Rolling Stuff
        for (int i = 0; i < rollingSquaresParent.transform.childCount; i++)
        {
            Transform square = rollingSquaresParent.transform.GetChild(i);
            rollingEmissionValue += (spectrumAmplitude - rollingEmissionValue) / 150f;

            float targetAngle = rollingEmissionValue * i * 25f * squareRollingDir;

            rollingSquaresAngles[i] += (targetAngle - rollingSquaresAngles[i]) / 100f;

            square.localEulerAngles = new Vector3(rollingSquaresAngles[i], 90, 90);
            float plannedPos = Mathf.Abs(rollingSquaresAngles[i]);
            square.localPosition = new Vector3(0, 0, plannedPos + 20 * i);
            square.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(rollingEmissionValue * 30f, rollingEmissionValue * 30f, rollingEmissionValue * 30f));
        }
    }
}
