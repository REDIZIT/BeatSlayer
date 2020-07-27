using System;
using System.Collections;
#if UNITY_EDITOR
using System.Drawing;
#endif
using System.IO;
using UnityEngine;

namespace InGame.Helpers
{
#if UNITY_EDITOR

    public class SaberMakerShooter : MonoBehaviour
    {
        public TransparencyCaptureToFile bloomRecorder;
        public TransparentBackgroundScreenshotRecorder objectRecorder;

        public string objectFilePath = "capture-object.png";
        public string bloomFilePath = "capture-bloom.png";
        public string resultFilePath = "capture-result.png";
        public bool deleteTempFiles = true;
        public bool usePersistentDataPath = true;

        private IEnumerator Start()
        {
            if (usePersistentDataPath)
            {
                resultFilePath = Application.persistentDataPath + "/" + resultFilePath;
                objectFilePath = Application.persistentDataPath + "/" + objectFilePath;
                bloomFilePath = Application.persistentDataPath + "/" + bloomFilePath;
            }




            Debug.Log("Capturing..");

            yield return StartCoroutine(bloomRecorder.capture(bloomFilePath));
            yield return objectRecorder.CaptureFrame(objectFilePath);

            Debug.Log("Merging..");

            Bitmap objectMap = new Bitmap(objectFilePath);
            Bitmap bloomMap = new Bitmap(bloomFilePath);

            
            if(objectMap.Width != bloomMap.Width || objectMap.Height != bloomMap.Height)
            {
                Debug.LogError("Object map: " + objectMap.Width + "x" + objectMap.Height);
                Debug.LogError("Bloom map: " + bloomMap.Width + "x" + bloomMap.Height);
                Debug.LogError("Different images sizes");
                yield break;
            }

            // Pausing for debug log working
            yield return null;

            // Go over all pixels and if object map has pixel colored then
            // make pixel on bloom map solid (alpha = 255)
            // Fixes semi-tranparent object on bloomed image
            for (int y = 0; y < bloomMap.Height; y++)
            {
                for (int x = 0; x < bloomMap.Width; x++)
                {
                    var pixel = objectMap.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        var bloomPixel = bloomMap.GetPixel(x, y);
                        bloomMap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, bloomPixel));
                    }
                }
            }

            Debug.Log("Saving..");

            yield return null;

            bloomMap.Save(resultFilePath);
            objectMap.Dispose();
            bloomMap.Dispose();

            if (deleteTempFiles)
            {
                File.Delete(objectFilePath);
                File.Delete(bloomFilePath);
            }

            Debug.Log("Done!");
        }
    }

#endif
}