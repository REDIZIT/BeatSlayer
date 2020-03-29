using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossLevel : MonoBehaviour
{
    AudioSource asrc;
    float prevVolume = 0;
    public Animator animator;

    [Header("SpectrumLines")]
    public LineRenderer leftLine, rightLine;


    void Start()
    {
        asrc = Camera.main.GetComponent<AudioSource>();

        leftLine.SetPosition(63, new Vector3(0, 0, 500));
        leftLine.SetPosition(62, new Vector3(0, 0, 62 * 2));
        rightLine.SetPosition(63, new Vector3(0, 0, 500));
        rightLine.SetPosition(62, new Vector3(0, 0, 62 * 2));
    }


    void Update()
    {
        Animate();
        AnimateKeys();
    }

    public int animKey = 0;
    public void AnimateKeys()
    {
        if(animKey == 0)
        {
            if(asrc.time >= 23.8f)
            {
                animKey++;
                animator.Play("FirstCharge");
            }
        }
    }

    public void Animate()
    {
        float volume = 0;
        float[] samples = new float[64];
        asrc.GetSpectrumData(samples, 0, FFTWindow.Triangle);
        volume = samples.Sum(c => volume + c);
        float diff = volume - prevVolume;

        leftLine.material.SetColor("_EmissionColor", new Color(1, 0.25f,0) * volume * 4f);
        rightLine.material.SetColor("_EmissionColor", new Color(1, 0.25f, 0) * volume * 4f);
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
    }
}
