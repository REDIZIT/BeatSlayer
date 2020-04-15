using System;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections;

public class ScreenshotBlur : MonoBehaviour
{
    int resWidth, resHeight;
    Camera camera { get { return GetComponent<Camera>(); } }

    IEnumerator Start()
    {
        resWidth = Screen.width;
        resHeight = Screen.height;


        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/original.png");
        while (!File.Exists(Application.persistentDataPath + "/original.png")) 
        {
            yield return new WaitForEndOfFrame();
        }

        
    }

    void DoBlur()
    {
        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/original.png");


        //TimeSpan d1 = Blurer.Blurer.Blur(bytes, Application.persistentDataPath + "/blur0.png", 0);
        //TimeSpan d2 = Blurer.Blurer.Blur(bytes, Application.persistentDataPath + "/blur1.png", 1);
        //TimeSpan d3 = Blurer.Blurer.Blur(bytes, Application.persistentDataPath + "/blur2.png", 2);
        TimeSpan d4 = Blurer.Blurer.Blur(bytes, Application.persistentDataPath + "/blur25.png", 25);

        //Debug.Log("d1 in " + d1.TotalMilliseconds);
        //Debug.Log("d2 in " + d2.TotalMilliseconds);
        //Debug.Log("d3 in " + d3.TotalMilliseconds);
        Debug.Log("d1 in " + d4.TotalMilliseconds);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) DoBlur();
    }

    byte[] TakeScreenshot()
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();

        return bytes;
    }
}