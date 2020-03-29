using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquaredCircle : MonoBehaviour
{
    AudioSource asrc;
    float prevVolume = 0;

    [Header("Rolling")]
    public Color orange, blue;
    float emissionVal;
    public Transform rollingParent;
    public int rollingCount;
    float targetAngle;
    float yTargetAngle, yAngleCooldown;
    float[] rollingSquaresAngles;
    float rollingEmissionValue;
    float changeDirVolume = 0.6f;
    int rotDir = 1;
    float rotDirCooldown = 0;
    float firstRotOffset = 0;


    // ===================================================================================
    //  Light beams
    [Header("Light Beams")]
    public Transform[] lightBeams;
    public Color lightBeamColor;
    float lightBeamValue, lightBeamDecrease;


    [Header("SpectrumLines")]
    public LineRenderer leftLine, rightLine;


    private void Start()
    {
        asrc = Camera.main.GetComponent<GameManager>().audioManager.asource;

        for (int i = 1; i < rollingCount; i++)
        {
            GameObject newRolling = Instantiate(rollingParent.GetChild(0).gameObject, rollingParent);
            newRolling.transform.eulerAngles = new Vector3(0, 0, i * 5);
            newRolling.transform.localPosition = new Vector3(0, 0, i * 5);
        }
        rollingSquaresAngles = new float[rollingParent.childCount];



        leftLine.SetPosition(63, new Vector3(0, 0, 500));
        leftLine.SetPosition(62, new Vector3(0, 0, 62 * 2));
        rightLine.SetPosition(63, new Vector3(0, 0, 500));
        rightLine.SetPosition(62, new Vector3(0, 0, 62 * 2));
    }

    private void Update()
    {
        Animate();
    }


    public void Animate()
    {
        float volume = 0;
        float[] samples = new float[64];
        asrc.GetSpectrumData(samples, 0, FFTWindow.Triangle);
        volume = samples.Sum(c => volume + c);
        float diff = volume - prevVolume;


        if(diff >= 0.6f)
        {
            firstRotOffset += Random.Range(-35, 35);
        }

        if(rotDirCooldown <= 0)
        {
            if (diff >= 1.1f)
            {
                rotDir = -rotDir;
                rotDirCooldown = 3;
            }
        }
        else
        {
            rotDirCooldown -= Time.deltaTime;
        }


        leftLine.material.SetColor("_EmissionColor", Color.white * volume * 3f);
        rightLine.material.SetColor("_EmissionColor", Color.white * volume * 3f);
        // Ignore last positions
        for (int i = 0; i < 64 - 2; i++)
        {
            float prevH = leftLine.GetPosition(i).y;
            float vol = samples[i] * (i + 1) * 10f;
            float h = (prevH + vol) / 2f;

            Vector3 v3 = new Vector3(0, h, i * 2);
            leftLine.SetPosition(i, v3);
            rightLine.SetPosition(i, v3);
        }




        for (int i = 0; i < rollingParent.transform.childCount; i++)
        {
            Transform square = rollingParent.transform.GetChild(i);
            rollingEmissionValue += (volume - rollingEmissionValue) / 100f;

            float targetAngle = rollingEmissionValue * i * 20f * rotDir + firstRotOffset;

            rollingSquaresAngles[i] += (targetAngle - rollingSquaresAngles[i]) / 100f;

            square.localEulerAngles = new Vector3(0, 0, rollingSquaresAngles[i]);

            square.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", orange * rollingEmissionValue * 10f);
            square.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", orange * rollingEmissionValue * 10f);

            square.GetChild(2).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", blue * rollingEmissionValue * 10f);
            square.GetChild(3).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", blue * rollingEmissionValue * 10f);

            //float plannedPos = Mathf.Abs(rollingSquaresAngles[i]);
            //square.localPosition = new Vector3(0, 0, plannedPos + 10 * i);
            //square.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(rollingEmissionValue * 30f, rollingEmissionValue * 30f, rollingEmissionValue * 30f));
        }


        // ========================================================================================================
        //  Light Beams
        if (diff > 0.8f)
        {
            lightBeamDecrease = 0.05f;
            lightBeamValue = diff * 1.5f;
        }
        else
        {
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





        prevVolume = volume;
    }
}