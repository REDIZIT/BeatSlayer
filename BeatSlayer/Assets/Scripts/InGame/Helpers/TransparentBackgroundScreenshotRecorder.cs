using UnityEngine;
using System.Collections;
using System.IO;

/*

Usage:
1. Attach this script to your chosen camera's game object.
2. Set that camera's Clear Flags field to Solid Color.
3. Use the inspector to set frameRate and framesToCapture
4. Choose your desired resolution in Unity's Game window (must be less than or equal to your screen resolution)
5. Turn on "Maximise on Play"
6. Play your scene. Screenshots will be saved to YourUnityProject/Screenshots by default.

*/

public class TransparentBackgroundScreenshotRecorder : MonoBehaviour
{

    #region private fields
    private string folderName = "";
    private GameObject whiteCamGameObject;
    private Camera whiteCam;
    private GameObject blackCamGameObject;
    private Camera blackCam;
    private Camera mainCam;
    private int videoFrame = 0; // how many frames we've rendered
    private float originalTimescaleTime;
    private bool done = false;
    private int screenWidth;
    private int screenHeight;
    private Texture2D textureBlack;
    private Texture2D textureWhite;
    private Texture2D textureTransparentBackground;
    #endregion

    void Awake()
    {
        mainCam = gameObject.GetComponent<Camera>();
        CreateBlackAndWhiteCameras();
        CacheAndInitialiseFields();
    }

    public IEnumerator CaptureFrame(string path)
    {
        yield return new WaitForEndOfFrame();

        RenderCamToTexture(blackCam, textureBlack);
        RenderCamToTexture(whiteCam, textureWhite);
        CalculateOutputTexture();
        SavePng(path);

        done = true;
        StopCoroutine("CaptureFrame");
    }

    void RenderCamToTexture(Camera cam, Texture2D tex)
    {
        cam.enabled = true;
        cam.Render();
        WriteScreenImageToTexture(tex);
        cam.enabled = false;
    }

    void CreateBlackAndWhiteCameras()
    {
        whiteCamGameObject = (GameObject)new GameObject();
        whiteCamGameObject.name = "White Background Camera";
        whiteCam = whiteCamGameObject.AddComponent<Camera>();
        whiteCam.CopyFrom(mainCam);
        whiteCam.backgroundColor = Color.white;
        whiteCamGameObject.transform.SetParent(gameObject.transform, true);

        blackCamGameObject = (GameObject)new GameObject();
        blackCamGameObject.name = "Black Background Camera";
        blackCam = blackCamGameObject.AddComponent<Camera>();
        blackCam.CopyFrom(mainCam);
        blackCam.backgroundColor = Color.black;
        blackCamGameObject.transform.SetParent(gameObject.transform, true);
    }

    void WriteScreenImageToTexture(Texture2D tex)
    {
        tex.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
        tex.Apply();
    }

    void CalculateOutputTexture()
    {
        Color color;
        for (int y = 0; y < textureTransparentBackground.height; ++y)
        {
            // each row
            for (int x = 0; x < textureTransparentBackground.width; ++x)
            {
                // each column
                float alpha = textureWhite.GetPixel(x, y).r - textureBlack.GetPixel(x, y).r;
                alpha = 1.0f - alpha;
                if (alpha == 0)
                {
                    color = Color.clear;
                }
                else
                {
                    color = textureBlack.GetPixel(x, y) / alpha;
                }
                color.a = alpha;
                textureTransparentBackground.SetPixel(x, y, color);
            }
        }
    }

    void SavePng(string path)
    {
        var pngShot = textureTransparentBackground.EncodeToPNG();
        File.WriteAllBytes(path, pngShot);
    }

    void CacheAndInitialiseFields()
    {
        originalTimescaleTime = Time.timeScale;
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        textureBlack = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        textureWhite = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        textureTransparentBackground = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
    }
}