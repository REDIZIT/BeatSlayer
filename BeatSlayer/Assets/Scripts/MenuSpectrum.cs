using UnityEngine;
using UnityEngine.UI;

public class MenuSpectrum : MonoBehaviour
{
    public CanvasScaler canvas;

    public Transform spectrum;
    public GameObject sourceText;

    public Transform video;

    public float kickVal;
    public Color kickColor, notKickColor;
    public float kickCooldown;
    float _kickCooldown;

    public float prevVolume;
    public bool useMenuMusic;
    public bool useKickVideo;

    public bool isPlaying;

    public void Start()
    {
        useMenuMusic = SSytem.GetBool("MenuMusic");

        useKickVideo = SSytem.GetBool("KickVideo");
        //sourceText.SetActive(useMenuMusic);

        if (useMenuMusic && !isPlaying)
        {
            isPlaying = true;
            GetComponent<AudioSource>().Play();

            GameObject prefab = spectrum.GetChild(0).gameObject;
            prefab.SetActive(true);
            for (int i = 1; i < 128 - 2; i++)
            {
                GameObject pike = Instantiate(prefab, spectrum);
            }
        }
        else if(isPlaying)
        {
            isPlaying = false;
            GetComponent<AudioSource>().Stop();
            spectrum.GetChild(0).gameObject.SetActive(false);
            foreach (Transform child in spectrum)
            {
                if (child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    float[] spectrumData = new float[64];
    float[] spectrumDecrease = new float[64];
    private void Update()
    {
        if (!useMenuMusic) return;
        if (spectrum.childCount != 128 - 2) return;

        float[] data = new float[64];
        GetComponent<AudioSource>().GetSpectrumData(data, 0, FFTWindow.Triangle);

        float volume = 0;
        for (int i = 0; i < 63; i++)
        {
            volume += data[i] * 2 + data[i] * i * 0.1f;
            spectrumData[i] -= spectrumDecrease[i];

            if(data[i] >= spectrumData[i])
            {
                spectrumDecrease[i] = 0.001f;
                spectrumData[i] = data[i];
            }
            else
            {
                spectrumDecrease[i] *= 1.2f;
            }

            Vector2 v2 = new Vector2(spectrum.GetChild(i).GetComponent<RectTransform>().sizeDelta.x, spectrumData[i] * 1000 * ((i + 1) / 2f));
            spectrum.GetChild(i).GetComponent<RectTransform>().sizeDelta = v2;
            spectrum.GetChild(63 * 2 - i - 1).GetComponent<RectTransform>().sizeDelta = v2;
        }

        if (useKickVideo)
        {
            float targetScale = 1 + volume * volume * 0.2f;
            float currentScale = video.localScale.x;
            currentScale += (targetScale - currentScale) * 0.4f;
            video.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        else if (video.localScale.x != 1)
        {
            video.localScale = new Vector3(1, 1, 1);
        }
    }
}
