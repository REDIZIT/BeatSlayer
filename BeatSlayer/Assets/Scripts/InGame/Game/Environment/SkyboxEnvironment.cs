using System.Diagnostics;
using UnityEngine;

namespace InGame.Game.Environment
{
    public class SkyboxEnvironment : MonoBehaviour
    {
        [SerializeField] private Transform skyboxTransform;

        public float delayTime;

        public float scaleMultiplier;
        public bool isScaling;

        private Camera cam;


        void Start()
        {
            cam = Camera.main;
            ScaleToCamRect();
        }

        private void ScaleToCamRect()
        {
            if (!isScaling) return;

            Stopwatch w = new Stopwatch();
            w.Start();

            Transform scaledImage = skyboxTransform;

            SpriteRenderer rend = scaledImage.GetComponent<SpriteRenderer>();
            float texWidth = rend.sprite.rect.width;
            float texHeight = rend.sprite.rect.width;

            float dst = scaledImage.position.z;



            float fov = 2f * Mathf.Atan(Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f)
                * cam.aspect) * Mathf.Rad2Deg; // Horizonal Field of View
            float w1 = Mathf.Tan(0.5f * fov * Mathf.Deg2Rad) * dst / 5f;
            float h1 = w1 * texHeight
                / texWidth;
            float h2 = Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * dst / 5f;
            float w2 = h2 * texWidth
                / texHeight;
            float width = Mathf.Max(w1, w2);
            float height = Mathf.Max(h1, h2);


            scaledImage.localScale = new Vector3(width * scaleMultiplier, height * scaleMultiplier, scaledImage.localScale.z);

            w.Stop();
            delayTime = w.ElapsedMilliseconds;
        }
    }
}
