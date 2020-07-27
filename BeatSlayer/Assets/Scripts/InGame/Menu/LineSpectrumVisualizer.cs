using UnityEngine;
using UnityEngine.UI.Extensions;

namespace InGame.Menu
{
    public class LineSpectrumVisualizer : MonoBehaviour, ISpectrumVisualizer
    {
        public UILineRenderer borderLineRenderer;
        public UILineRenderer fillingLineRenderer;

        public AudioSource asource;

        [SerializeField] private float multiplier = 800;
        [SerializeField] private float normalizePower = 300;
        [SerializeField] private float decreaseStart = 1, decreaseMultiplier = 1.5f;

        private readonly float[] samples;
        private readonly float[] heights;

        public LineSpectrumVisualizer()
        {
            samples = new float[64];
            heights = new float[samples.Length];
        }

        public void SetEnabled(bool isOn)
        {
            borderLineRenderer.enabled = isOn;
        }

        public void UpdateData()
        {
            if (!borderLineRenderer.enabled) return;
            //Stopwatch w = Stopwatch.StartNew();

            asource.GetSpectrumData(samples, 0, FFTWindow.Blackman);


            borderLineRenderer.Points = new Vector2[samples.Length * 2];
            //fillingLineRenderer.Points = new Vector2[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                float prevHeight = heights[i];
                float height = samples[i] * (i + 1) * multiplier;

                float newHeight = (height * 1f + prevHeight * 1f) / 2f;

                heights[i] = newHeight;
                Vector2 v = new Vector2(i / (float)(samples.Length * 2), newHeight);

                borderLineRenderer.Points[i] = v;
                //fillingLineRenderer.Points[i] = v;
            }
            for (int i = 0; i < samples.Length; i++)
            {
                float height = heights[heights.Length - 1 - i];
                Vector2 v = new Vector2((samples.Length + i + 1) / (float)(samples.Length * 2), height);

                borderLineRenderer.Points[samples.Length + i] = v;
                //fillingLineRenderer.Points[samples.Length + i] = v;
            }

            //w.Stop();
        }
    }
}
