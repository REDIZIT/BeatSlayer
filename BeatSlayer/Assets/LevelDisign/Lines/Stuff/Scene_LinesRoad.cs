using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scene_LinesRoad : MonoBehaviour
{
    public LineRenderer line, leftLine;
    LineRenderer _line, _leftLine;
    public LineRenderer highLine, leftHighLine;
    LineRenderer _highLine, _leftHighLine;
    public LineRenderer lowLine, leftLowLine;
    LineRenderer _lowLine, _leftLowLine;


    // ===================================================================================
    //  Rolling Stuff
    [Header("Rolling Stuff")]
    public Transform rollingStuff;
    public float[] rollingSquaresAngles;
    public int rollDir;
    bool rollDirBlock;
    float rollingEmissionValue;
    Color curColor;


    AudioSource asource;
    float[] data = new float[64];

    public int speed;

    List<float> volumeLs = new List<float>();

    private void Start()
    {
        asource = Camera.main.GetComponent<GameManager>().audioManager.asource;

        line.positionCount = 60;
        leftLine.positionCount = 60;

        _line = Instantiate(line);
        _line.transform.position -= new Vector3(0, 3, 0);
        _leftLine = Instantiate(leftLine);
        _leftLine.transform.position -= new Vector3(0, 3, 0);

        highLine.positionCount = 60;
        leftHighLine.positionCount = 60;

        _highLine = Instantiate(highLine);
        _highLine.transform.position += new Vector3(0, 6, 0);
        _leftHighLine = Instantiate(leftHighLine);
        _leftHighLine.transform.position += new Vector3(0, 6, 0);

        lowLine.positionCount = 60;
        leftLowLine.positionCount = 60;
        _lowLine = Instantiate(lowLine);
        _lowLine.transform.position += new Vector3(0, -1, 0);
        _leftLowLine = Instantiate(leftLowLine);
        _leftLowLine.transform.position += new Vector3(0, -1, 0);

        //for (int i = 0; i < rollingStuff.childCount; i++)
        //{
        //    rollingStuff.GetChild(i).transform.localPosition = new Vector3(0, 0, 10 * i);
        //    rollingStuff.GetChild(i).transform.eulerAngles = new Vector3(-90, 90, -90);
        //}
        rollDir = (int)Random.value;
        rollDir = rollDir == 0 ? -1 : 1;
        for (int i = 0; i < rollingStuff.childCount; i++)
        {
            Transform triangle = rollingStuff.GetChild(i);

            rollingStuff.GetChild(i).transform.localPosition = new Vector3(0, 0, 10 * i);
            triangle.localEulerAngles = new Vector3(0 - 90, 90, -90);
            rollingStuff.GetChild(i).GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.black);
            rollingStuff.GetChild(i).GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.black);
        }
        rollingSquaresAngles = new float[rollingStuff.childCount];
    }
    public void Update()
    {
        if (asource.isPlaying)
        {
            Animate();
        }
    }
    public void Animate()
    {
        asource.GetSpectrumData(data, 0, FFTWindow.Triangle);

        for (int i = 0; i < 64; i++)
        {
            data[i] /= 2f; // Нужно делать на 2 т.к. я поднял громкость у AudioSource с 0.25 до 0.5
        }

        float volume = data.Sum();
        float highVolume = 0;
        float lowVolume = 0;
        for (int i = 0; i < 64; i++)
        {
            if (i > 12) highVolume += data[i];
            if (i < 4) lowVolume += data[i];
        }

        if (volumeLs.Count >= 60)
        {
            volumeLs.RemoveAt(0);
        }
        volumeLs.Add(volume);


        float scale = 2 + volume * 2.5f;
        if(scale < line.transform.localScale.x)
        {
            float diff = line.transform.localScale.x - scale;
            scale = line.transform.localScale.x - diff / 25f;
        }
        Vector3 scalev3 = new Vector3((line.transform.localScale.x + scale) / 2f, 2, 2);
        line.transform.localScale = scalev3;
        leftLine.transform.localScale = scalev3;
        _line.transform.localScale = scalev3;
        _leftLine.transform.localScale = scalev3;
        highLine.transform.localScale = scalev3;
        leftHighLine.transform.localScale = scalev3;
        _highLine.transform.localScale = scalev3;
        _leftHighLine.transform.localScale = scalev3;
        lowLine.transform.localScale = scalev3;
        leftLowLine.transform.localScale = scalev3;
        _lowLine.transform.localScale = scalev3;
        _leftLowLine.transform.localScale = scalev3;


        for (int i = 0; i < 60; i++)
        {
            if (volumeLs.Count - 1 - i < 0) continue;
            float a = volumeLs[volumeLs.Count - 1 - i];
            float b = a, c = a, d = a, e = a, f = a, g = a;
            if (volumeLs.Count - 2 - i >= 0) { b = volumeLs[volumeLs.Count - 2 - i]; }
            if (volumeLs.Count - 3 - i >= 0) { c = volumeLs[volumeLs.Count - 3 - i]; }
            if (volumeLs.Count - 4 - i >= 0) { d = volumeLs[volumeLs.Count - 4 - i]; }
            if (volumeLs.Count - 5 - i >= 0) { e = volumeLs[volumeLs.Count - 5 - i]; }
            if (volumeLs.Count - 6 - i >= 0) { f = volumeLs[volumeLs.Count - 6 - i]; }
            if (volumeLs.Count - 7 - i >= 0) { g = volumeLs[volumeLs.Count - 7 - i]; }
            float lowSmoothVal = (a + b) / 2f;
            float semiSmoothVal = (a + b + c) / 4f;
            float smoothVal = (a + b + c + d + e + f + g) / 7f;

            Vector3 pos = new Vector3(i / 2f, smoothVal * 8, -i * i / 50f);
            Vector3 semiPos = new Vector3(i / 2f, semiSmoothVal * 8, -i * i / 50f);
            Vector3 lowPos = new Vector3(i / 2f, lowSmoothVal * 8, -i * i / 50f);
            line.SetPosition(i, pos);
            leftLine.SetPosition(i, new Vector3(-1 * pos.x, pos.y, pos.z));
            _line.SetPosition(i, new Vector3(semiPos.x, -semiPos.y, semiPos.z));
            _leftLine.SetPosition(i, new Vector3(-1 * semiPos.x, -semiPos.y, semiPos.z));

            highLine.SetPosition(i, semiPos);
            leftHighLine.SetPosition(i, new Vector3(-1 * semiPos.x, semiPos.y, semiPos.z));
            _highLine.SetPosition(i, new Vector3(pos.x, -pos.y, pos.z));
            _leftHighLine.SetPosition(i, new Vector3(-1 * pos.x, -pos.y, pos.z));

            lowLine.SetPosition(i, pos);
            leftLowLine.SetPosition(i, new Vector3(-1 * pos.x, pos.y, pos.z));
            _lowLine.SetPosition(i, new Vector3(lowPos.x, -lowPos.y, lowPos.z));
            _leftLowLine.SetPosition(i, new Vector3(-1 * lowPos.x, -lowPos.y, lowPos.z));
        }


        float clr = line.material.GetColor("_EmissionColor").a;
        float alpha = (volume + clr) / 2f;
        alpha = alpha > 0.2f ? alpha : 0;
        alpha = Mathf.Clamp01(alpha);
        Color color = new Color(alpha * 9, 0.25f * alpha * 5, 0, alpha);
        line.material.SetColor("_EmissionColor", color);
        leftLine.material.SetColor("_EmissionColor", color);
        _line.material.SetColor("_EmissionColor", color * 2);
        _leftLine.material.SetColor("_EmissionColor", color * 2);

        Color highColor = new Color(highVolume, highVolume, highVolume * 20, alpha);
        highLine.material.SetColor("_EmissionColor", highColor);
        leftHighLine.material.SetColor("_EmissionColor", highColor);
        _highLine.material.SetColor("_EmissionColor", highColor * 2f);
        _leftHighLine.material.SetColor("_EmissionColor", highColor * 2f);

        Color lowColor = new Color(lowVolume * 25, 0, 0, alpha);
        lowLine.material.SetColor("_EmissionColor", lowColor);
        leftLowLine.material.SetColor("_EmissionColor", lowColor);
        _lowLine.material.SetColor("_EmissionColor", lowColor * 2f);
        _leftLowLine.material.SetColor("_EmissionColor", lowColor * 2f);


        float width = (line.startWidth + Mathf.Clamp01(volume)) / 2f;
        line.startWidth = width;
        line.endWidth = width;
        leftLine.startWidth = width;
        leftLine.endWidth = width;
        _line.startWidth = width * 2f;
        _line.endWidth = width * 2f;
        _leftLine.startWidth = width * 2f;
        _leftLine.endWidth = width * 2f;

        width = (highLine.startWidth + Mathf.Clamp01(highVolume)) / 2f;
        highLine.startWidth = width;
        highLine.endWidth = width;
        leftHighLine.startWidth = width;
        leftHighLine.endWidth = width;
        _highLine.startWidth = width * 3f;
        _highLine.endWidth = width * 3f;
        _leftHighLine.startWidth = width * 3f;
        _leftHighLine.endWidth = width * 3f;

        width = (lowLine.startWidth + Mathf.Clamp01(lowVolume)) / 2f;
        lowLine.startWidth = width;
        lowLine.endWidth = width;
        leftLowLine.startWidth = width;
        leftLowLine.endWidth = width;
        _lowLine.startWidth = width * 2f;
        _lowLine.endWidth = width * 2f;
        _leftLowLine.startWidth = width * 2f;
        _leftLowLine.endWidth = width * 2f;





        // ========================================================================================================================================================
        //  Rolling Stuff
        Color targetInnerColor = highVolume > 0.2f ? Color.cyan * 0.9f : lowVolume > 0.2f ? new Color(1, 0.05f, 0) : new Color(0.9f, 0.2f, 0) * volume;
        curColor += (targetInnerColor - curColor) / 10f;
        Color reverseColor = new Color(curColor.b, 0, curColor.r);

        if (rollDirBlock)
        {
            if (volume < 0.05f) rollDirBlock = false;
        }
        else
        {
            if (volume > 0.5f) { rollDirBlock = true; rollDir = -rollDir; }
        }

        for (int i = 0; i < rollingStuff.childCount; i++)
        {
            Transform triangle = rollingStuff.GetChild(i);
            rollingEmissionValue += (volume - rollingEmissionValue) / 150f;

            float targetAngle = rollingEmissionValue * i * 25f * rollDir;

            rollingSquaresAngles[i] += (targetAngle - rollingSquaresAngles[i]) / 100f;

            triangle.localEulerAngles = new Vector3(rollingSquaresAngles[i] - 90, 90, -90);
            float whiteVal = Mathf.Clamp01(rollingEmissionValue) * 5;
            rollingStuff.GetChild(i).GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", curColor * 2);
            rollingStuff.GetChild(i).GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", reverseColor * 2);
        }
    }
}