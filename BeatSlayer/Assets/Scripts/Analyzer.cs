using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Analyzer : MonoBehaviour
{
    //public GameScript gs;
    public GameManager gs;
    public AudioProcessor processor;

    public float[] data;

    public float volumelineOffset = 0;
    public List<AnalyzerItem> volumeline = new List<AnalyzerItem>();

    public float cubeCooldown = 0.2f;
    float m_cubeCooldown;

    public float highTier;

    public bool display;
    public bool selfUpdate;

    public bool useDoubleAudioSource;

    public float timeLeftAfterSpawn = 0;

    private float fpslimiter;

    private float ta_prevVolume = 0;
    private float[] ta_prevSamples;
    private float ta_timeFromLastSpawn = 0;


    private void Start()
    {
        //processor.onBeat.AddListener(() =>
        //{
        //    m_cubeCooldown = 0.05f;


        //    BeatCubeClass.Type t = ta_timeFromLastSpawn <= 0.14f ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir;

        //    if (ta_timeFromLastSpawn >= 0.14f && Random.value >= 0.92f) t = BeatCubeClass.Type.Bomb;

        //    int road = Random.Range(0, 4);
        //    int saberType = road <= 1 ? -1 : 1;

        //    BeatCubeClass cl = new BeatCubeClass(-1, road, t);
        //    cl.saberType = saberType;
        //    if (t == BeatCubeClass.Type.Dir)
        //    {
        //        int random = Random.Range(0, 8);

        //        cl.subType = (BeatCubeClass.SubType)random;
        //    }

        //    gs.beatManager.SpawnBeatCube(cl);
        //    ta_timeFromLastSpawn = 0;
        //});
    }

    private void Update()
    {
        if (selfUpdate)
        {
            AnalyzerUpdate();
        }

        //ta_timeFromLastSpawn += Time.deltaTime;
        //if (fpslimiter <= 0)
        //{
        //    fpslimiter = 0.01666666666666666666666666666667f;
        //}
        //else
        //{
        //    fpslimiter -= Time.deltaTime;
        //    return;
        //}
    }



    public void AnalyzerUpdate()
    {

        //processor.enabled = true;


        //// Old part
        //return;

        timeLeftAfterSpawn += Time.deltaTime;
        if (fpslimiter <= 0)
        {
            fpslimiter = 0.01666666666666666666666666666667f;
        }else
        {
            fpslimiter -= Time.deltaTime;
            return;
        }

        if (useDoubleAudioSource)
        {
            gs.audioManager.spectrumAsource.GetSpectrumData(data, 0, FFTWindow.Triangle);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= 250;
            }
        }
        else
        {
            gs.audioManager.asource.GetSpectrumData(data, 0, FFTWindow.Triangle);
        }


        ThirdAlg();


        volumelineOffset += 1f / 8f;
    }


    
    public void ThirdAlg()
    {
        if (ta_prevSamples == null) ta_prevSamples = new float[data.Length];
        float disturbance = 0;

        for (int i = 0; i < data.Length; i++)
        {
            disturbance += Mathf.Abs(data[i] - ta_prevSamples[i]);
            ta_prevSamples[i] = data[i] * ((i + 1) / 1.8f);
            
        }
        float volume = disturbance;

        AnalyzerItem item = new AnalyzerItem(volume, data);
        volumeline.Add(item);


        ta_timeFromLastSpawn += Time.deltaTime;
        if (m_cubeCooldown <= 0)
        {
            if (ta_prevVolume + 0.25f < volume)
            {
                item.isBeatCube = true;
                m_cubeCooldown = 0.06f; // default was 0.05


                BeatCubeClass.Type t = ta_timeFromLastSpawn <= 0.14f ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir;

                if (ta_timeFromLastSpawn >= 0.14f && Random.value >= 0.92f) t = BeatCubeClass.Type.Bomb;

                int road = Random.Range(0, 4);
                int level = Random.value >= 0.75f ? 1 : 0;
                int saberType = road <= 1 ? -1 : 1;

                BeatCubeClass cl = new BeatCubeClass(-1, road, t);
                cl.saberType = saberType;
                cl.level = level;
                if (t == BeatCubeClass.Type.Dir)
                {
                    int random = Random.Range(0, 8);
                        
                    cl.subType = (BeatCubeClass.SubType)random;
                }

                gs.beatManager.SpawnBeatCube(cl);
                ta_timeFromLastSpawn = 0;
            }
        }
        else
        {
            m_cubeCooldown -= Time.deltaTime;
        }

        ta_prevVolume = volume;
    }
    //private void OnDrawGizmos()
    //{
    //    //if (!display) return;

    //    for (int i = 1; i < data.Length; i++)
    //    {
    //        Gizmos.DrawLine(new Vector3((i - 1) / 4f, data[i - 1] * 10, 0), new Vector3(i / 4f, data[i] * 10));
    //    }

    //    for (int i = 1; i < volumeline.Count; i++)
    //    {
    //        Vector3 startPos = new Vector3((i - 1) / 8f, volumeline[i - 1].volume);
    //        Vector3 pos = new Vector3(i / 8f, volumeline[i].volume);

    //        startPos += new Vector3(-volumelineOffset, -5, 0);
    //        pos += new Vector3(-volumelineOffset, -5, 0);

    //        Gizmos.color = Color.gray;
    //        if (volumeline[i].isHighTier) Gizmos.color = Color.cyan;

    //        if (volumeline[i].diff > 0.2f) Gizmos.color = Color.red;
    //        Gizmos.DrawLine(startPos, pos);

    //        if(volumeline[i].isBeatCube)
    //        {
    //            Gizmos.color = Color.red;
    //            Gizmos.DrawLine(pos - Vector3.down * 5, pos + Vector3.down * 5);
    //        }
    //    }
    //}
}

public class AnalyzerItem
{
    public float volume;
    public float[] data;
    public float diff;

    public bool isBeatCube;
    public bool isHighTier;

    public AnalyzerItem(float volume, float[] data)
    {
        this.volume = volume;
        this.data = data;
    }
}