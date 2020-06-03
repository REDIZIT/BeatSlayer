using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

public class SpectrumVisualizer : MonoBehaviour
{
    public AudioSource aSource;
    public RectTransform[] bars;

    public RawImage bgVideo;
    public Color defaultColor;
    public float bgWhiteEffect;

    public float avgTime;

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
        /*if (SSytem.instance.GetBool("MenuMusic"))
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
        }*/
    }

    public void Init()
    {
        StartCoroutine(IEInit());
    }
    public IEnumerator IEInit()
    {
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
        yield return new WaitForEndOfFrame();
    }

    
    public void Stop()
    {
        foreach (Transform child in transform) if (child.name != "Bar") Destroy(child.gameObject);
        transform.GetChild(0).gameObject.SetActive(false);

        isInited = false;
    }


    private void Update()
    {
        if (!isInited) return;

        float[] data = new float[64];

       // Stopwatch w = new Stopwatch();
        //w.Start();

        //aSource.GetSpectrumData(data, 0, FFTWindow.BlackmanHarris);
        aSource.GetSpectrumData(data, 0, FFTWindow.Hamming);

       

        float volume = 0;
        for (int i = 0; i < bars.Length; i++)
        {
            float height = data[i] * multiplier + (data[i] * i * normalizePower);
            volume += height;


            if (height > heights[i])
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


        //w.Stop();
        //Debug.Log(w.ElapsedMilliseconds);
        //avgTime = w.ElapsedMilliseconds;

        AnimateBackground(volume);
    }
    

    void AnimateBackground(float v)
    {
        float volume = v * bgWhiteEffect;
        Color clr = defaultColor + Color.white * volume * volume;
        bgVideo.color = clr;
    }
}
