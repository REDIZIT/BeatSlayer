using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Analyzer : MonoBehaviour
{
    //public GameScript gs;
    public GameManager gs;

    public float[] data;

    public float volumelineOffset = 0;
    public List<AnalyzerItem> volumeline = new List<AnalyzerItem>();

    public float cubeCooldown = 0.2f;
    float m_cubeCooldown;

    public float highTier;

    public bool display;
    public bool selfUpdate;

    public bool useDoubleAudioSource;

    int useAlg = 3;

    public float timeLeftAfterSpawn = 0;

    private void Update()
    {
        if (selfUpdate)
        {
            AnalyzerUpdate();
        }
    }

    float fpslimiter;
    float asourceDelay;
    public void AnalyzerUpdate()
    {
        timeLeftAfterSpawn += Time.deltaTime;
        if (fpslimiter <= 0)
        {
            fpslimiter = 0.01666666666666666666666666666667f;
        }else
        {
            fpslimiter -= Time.deltaTime;
            return;
        }

        if (useAlg == -1)
        {
            if (File.Exists(Application.persistentDataPath + "/alg1.txt")) useAlg = 1;
            else if (File.Exists(Application.persistentDataPath + "/alg2.txt")) useAlg = 2;
            else
            {
                useAlg = 1;
            }
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

        asourceDelay = Mathf.Abs(gs.audioManager.asource.time - gs.audioManager.spectrumAsource.time);
        if (useAlg == 1)
        {
            FirstAlg();
        }
        else if (useAlg == 2)
        {
            SecondAlg();
        }
        else if (useAlg == 3)
        {
            ThirdAlg();
        }

        volumelineOffset += 1f / 8f;
    }


    // First successful algorithm
    public void FirstAlg()
    {

        float volume = 0;
        float origVolume = 0;
        for (int i = 0; i < data.Length; i++)
        {
            //if(i < 32 && i > 4)
            //{
            //    volume += data[i] * 2f;
            //}
            volume += data[i] * 2f;
            if(i < 2)
            {
                if (highTier > 0) volume *= 4;
            }
        }
        origVolume = volume;
        //if (highTier > 0) volume *= 3f;


        AnalyzerItem item = new AnalyzerItem(volume, data);
        volumeline.Add(item);

        if (highTier > 0) highTier -= Time.deltaTime;
        if (volumeline.Count > 1)
        {
            Color def = Color.gray;

            int len = volumeline.Count;

            //if (volumeline.Count > 1 && volumeline[len - 1].volume > 0.4f && volumeline[len - 2].volume > 0.4f && volumeline[len - 3].volume > 0.4f) item.isHighTier = true;

            float diff = volumeline[volumeline.Count - 1].volume - volumeline[volumeline.Count - 2].volume;
            //diff > 0.2f ? Color.red : def;
            item.diff = diff;

            float roundedVol = Mathf.RoundToInt(volume * 100) / 100f;

            float startPointVal = 0.15f; // 75% of 0.2f
            float addPointVal = volume * 0.1f; // 25% of 0.2f
            float highTierStartVal = 0.7f; // 75% of 1.3f = 0.975f
            float highTierAddVal = volume * 0.35f; // 25% of 1.3f = 0.325f
            float highTierVal = highTierStartVal + highTierAddVal;
            // 0.0 -> 0.15f
            // 0.1 -> 0.2f
            // 0.2 -> 0.25f

            //float pointVal = 0.2f;
            float pointVal = startPointVal + addPointVal;


            if (volume > highTierVal) { highTier = 0.1f; }
            else if (volume < highTierVal * 0.5f) { highTier = 0; }


            if (highTier > 0) pointVal *= 2f / volume * (highTier > 0 ? 1.25f : 0.75f );
            if (m_cubeCooldown < 0)
            {
                if (diff > pointVal)
                {
                    item.isBeatCube = true;
                    m_cubeCooldown = highTier > 0 ? cubeCooldown * 0.2f : cubeCooldown;
                    //Color c = Color.white;
                    //if (highTier > 0) c = Color.magenta;

                    BeatCubeClass.Type t = BeatCubeClass.Type.Point;
                    if (timeLeftAfterSpawn > 1f) { t = BeatCubeClass.Type.Bomb; }
                    BeatCubeClass cl = new BeatCubeClass(-1, -1, t);
                    if(t == BeatCubeClass.Type.Dir)
                    {
                        int random = Random.Range(0, 8);
                        
                        cl.subType = (BeatCubeClass.SubType)random;
                    }

                    gs.beatManager.SpawnBeatCube(cl);
                    timeLeftAfterSpawn = 0;
                }
            }
            else
            {
                m_cubeCooldown -= Time.deltaTime;
            }
        }
    }


    public void SecondAlg()
    {
        float volume = 0;
        float origVolume = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if(i < 4)
            {
                volume += data[i] * (i + 1);
            }
            volume += data[i] * (i + 1);
        }
        volume *= 0.5f;
        origVolume = volume;
        //if (highTier > 0) volume *= 3f;


        AnalyzerItem item = new AnalyzerItem(volume, data);
        volumeline.Add(item);

        if (highTier > 0) highTier -= Time.deltaTime;
        if (volumeline.Count > 1)
        {
            Color def = Color.gray;

            int len = volumeline.Count;


            float diff = volumeline[volumeline.Count - 1].volume - volumeline[volumeline.Count - 2].volume;
            item.diff = diff;

            float roundedVol = Mathf.RoundToInt(volume * 100) / 100f;

            float startPointVal = 0.15f; // 75% of 0.2f
            float addPointVal = volume * 0.06f; // 25% of 0.2f
            float highTierStartVal = 0.73f; // 75% of 1.3f = 0.975f
            float highTierAddVal = volume * 0.28f; // 25% of 1.3f = 0.325f
            float highTierVal = highTierStartVal + highTierAddVal;
            // 0.0 -> 0.15f
            // 0.1 -> 0.2f
            // 0.2 -> 0.25f

            //float pointVal = 0.2f;
            float pointVal = startPointVal + addPointVal;


            if (volume > highTierVal) { highTier = 0.3f; }

            if (highTier > 0) pointVal *= 3f / (highTierVal + 1);
            if (m_cubeCooldown < 0)
            {
                if (diff > pointVal)
                {
                    item.isBeatCube = true;
                    m_cubeCooldown = highTier > 0 ? cubeCooldown * 0.4f : cubeCooldown;
                    //Color c = Color.white;
                    //if (highTier > 0) c = Color.magenta;
                    gs.beatManager.SpawnBeatCube(new BeatCubeClass(0, -1, BeatCubeClass.Type.Point)/*, c*/);
                }
            }
            else
            {
                m_cubeCooldown -= Time.deltaTime;
            }
        }
    }


    float ta_prevVolume = 0;
    float[] ta_prevSamples;
    float ta_timeFromLastSpawn = 0;
    public void ThirdAlg()
    {
        //Debug.Log(asource.isPlaying + " / " + spectrumAudioSource.isPlaying);

        if (ta_prevSamples == null) ta_prevSamples = new float[data.Length];
        float disturbance = 0;

        float volume = 0;
        for (int i = 0; i < data.Length; i++)
        {
            //volume += data[i] * 2;
            disturbance += Mathf.Abs(data[i] - ta_prevSamples[i]);
            ta_prevSamples[i] = data[i] * ((i + 1) / 1.8f);
            
        }
        volume = disturbance;

        AnalyzerItem item = new AnalyzerItem(volume, data);
        volumeline.Add(item);


        ta_timeFromLastSpawn += Time.deltaTime;
        if (m_cubeCooldown <= 0)
        {
            if (ta_prevVolume + 0.25f < volume)
            {
                item.isBeatCube = true;
                m_cubeCooldown = 0.05f;
                //Color c = Color.white;

                BeatCubeClass.Type t = ta_timeFromLastSpawn <= 0.14f ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir;

                if (ta_timeFromLastSpawn >= 0.14f && Random.value >= 0.92f) t = BeatCubeClass.Type.Bomb;

                int road = Random.Range(0, 4);
                int saberType = road <= 1 ? -1 : 1;

                BeatCubeClass cl = new BeatCubeClass(-1, road, t);
                cl.saberType = saberType;
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


    private void OnDrawGizmos()
    {
        //if (!display) return;

        for (int i = 1; i < data.Length; i++)
        {
            Gizmos.DrawLine(new Vector3((i - 1) / 4f, data[i - 1] * 10, 0), new Vector3(i / 4f, data[i] * 10));
        }

        for (int i = 1; i < volumeline.Count; i++)
        {
            Vector3 startPos = new Vector3((i - 1) / 8f, volumeline[i - 1].volume);
            Vector3 pos = new Vector3(i / 8f, volumeline[i].volume);

            startPos += new Vector3(-volumelineOffset, -5, 0);
            pos += new Vector3(-volumelineOffset, -5, 0);

            Gizmos.color = Color.gray;
            if (volumeline[i].isHighTier) Gizmos.color = Color.cyan;

            if (volumeline[i].diff > 0.2f) Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, pos);

            if(volumeline[i].isBeatCube)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos - Vector3.down * 5, pos + Vector3.down * 5);
            }
        }
    }
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