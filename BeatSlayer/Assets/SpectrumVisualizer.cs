using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SpectrumVisualizer : MonoBehaviour
{
    public AudioSource aSource;
    public RectTransform[] bars;

    public float bpm;
    float timeFromBeat;

    public Text musicSourceText;

    public bool isInited;

    float[] heights = new float[64];
    float[] decrease = new float[64];

    [Header("Settings")]
    public float multiplier;
    public float normalizePower;

    public float decreaseStart, decreaseMultiplier;

    public float width;


    public void Start()
    {
        if (SSytem.instance.GetBool("MenuMusic"))
        {
            if(!isInited)
            {
                Init();
            }
            else
            {
                aSource.Play();
            }
        }
        else
        {
            Stop();
        }
    }

    public void Init()
    {
        StartCoroutine(IEInit());
    }
    public IEnumerator IEInit()
    {
        //string[] maps = Directory.GetDirectories(Application.persistentDataPath + "/maps");

        //if (maps.Length == 0) yield break;

        //string mapPath = maps[UnityEngine.Random.Range(0, maps.Length - 1)];
        //string nickPath = Directory.GetDirectories(mapPath)[0];
        //string audioPath = nickPath + "/" + new DirectoryInfo(mapPath).Name + ".mp3";
        //if (!File.Exists(audioPath)) audioPath = nickPath + "/" + new DirectoryInfo(mapPath).Name + ".ogg";

        //musicSourceText.text = $"♪   {new DirectoryInfo(mapPath).Name}   ♪";

        //DateTime t1 = DateTime.Now;
        //Debug.Log("Loading " + audioPath);
        //using (WWW w = new WWW("file:///" + audioPath))
        //{
        //    yield return w;
        //    DateTime t2 = DateTime.Now;
        //    aSource.clip = w.GetAudioClip();
        //    DateTime t3 = DateTime.Now;
        //    Debug.Log("Clip load time: " + (t3 - t2).TotalMilliseconds);
        //}


        //Debug.Log("File load time: " + (DateTime.Now - t1).TotalMilliseconds);


        float rectWidth = GetComponent<RectTransform>().sizeDelta.x;
        float spacing = rectWidth / 64f;

        bars = new RectTransform[64];
        transform.GetChild(0).gameObject.SetActive(true);

        for (int i = 0; i < 64; i++)
        {
            GameObject bar = Instantiate(transform.GetChild(0).gameObject, transform);
            bars[i] = bar.GetComponent<RectTransform>();
            bars[i].anchoredPosition = new Vector2(spacing * i, 0);
        }

        transform.GetChild(0).gameObject.SetActive(false);

        isInited = true;

        aSource.Play();

        yield return new WaitForEndOfFrame();
    }

    
    public void Stop()
    {
        foreach (Transform child in transform) if (child.name != "Bar") Destroy(child.gameObject);
        transform.GetChild(0).gameObject.SetActive(false);

        isInited = false;

        aSource.Stop();

        musicSourceText.text = "";
    }


    private void Update()
    {
        if (!isInited) return;

        float[] data = new float[64];
        aSource.GetSpectrumData(data, 0, FFTWindow.Hamming);

        for (int i = 0; i < bars.Length; i++)
        {
            float height = data[i] * multiplier + (data[i] * i * normalizePower);


            if(height > heights[i])
            {
                decrease[i] = decreaseStart;
                heights[i] = height;
            }
            else
            {
                decrease[i] *= decreaseMultiplier;
                heights[i] -= decrease[i];
                height = heights[i];
            }

            if (height < 0) height = 0;

            bars[i].sizeDelta = new Vector2(width, height + width);
        }


        //float beatsPerSecond = bpm / 60f;
        //float minScale = 0.8f;
        //float maxScale = 1;

        //timeFromBeat += Time.deltaTime;

        //if(timeFromBeat >= 60f / bpm)
        //{
        //    transform.localScale = Vector3.one;
        //    timeFromBeat = 0;
        //}
        //else
        //{
        //    transform.localScale -= Vector3.one * Time.deltaTime * 0.2f;
        //}
        
    }
}
