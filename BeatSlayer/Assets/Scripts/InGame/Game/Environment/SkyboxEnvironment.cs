using System.Diagnostics;
using UnityEngine;

namespace InGame.Game.Environment
{
    public class SkyboxEnvironment : MonoBehaviour
    {
        [SerializeField] private Transform skyboxTransform;
        [SerializeField] private Camera playerCamera;

        public float delayTime;

        public float scaleMultiplier;
        public bool isScaling;


        void Start()
        {
            //if (!isScaling) return;

            ScaleToCamRect();
        }


        /// <summary>
        /// Stretch object with screen size
        /// </summary>
        //private void ScaleToCamRect()
        //{
        //    float distance = skyboxTransform.position.z;
        //    float height = Mathf.Tan(playerCamera.fieldOfView) * distance;
        //    float width = height * Screen.width / Screen.height / 2f;

        //    skyboxTransform.localScale = new Vector3(width * scaleMultiplier, height * scaleMultiplier, skyboxTransform.localScale.z);
        //}

        private void ScaleToCamRect()
        {
            if (!isScaling) return;

            Stopwatch w = new Stopwatch();
            w.Start();

            Transform scaledImage = skyboxTransform;
            Camera cam = playerCamera;

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

        //private void ScaleToCamRect()
        //{
        //    float distance = skyboxTransform.position.z;
        //    float height = Mathf.Tan(0.5f * playerCamera.fieldOfView * Mathf.Deg2Rad) * distance;
        //    float width = height * Screen.width / Screen.height;

        //    width /= 10f;
        //    height /= 10f;


        //    skyboxTransform.localScale = new Vector3(width, height, 1f);
        //}
    }
}
