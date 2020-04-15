using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraBlur : MonoBehaviour
{
    public Camera cam { get { return GetComponent<Camera>(); } }
    public RawImage rawImage;
    public RawImage rawImage2;
    public bool update;
    public RenderTexture renderTexture;
    public Texture2D texture2D;
    int width, height;

    private void Awake()
    {
        width = Screen.width;
        height = Screen.height;

        texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
        
    }
    void Update()
    {
        if (update)
        {
            TakeScreenshot();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Debug.Log("OnRenderImage");
        //cam.enabled = false;
        RenderTexture.active = destination;
        cam.Render();
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        rawImage2.texture = texture2D;
    }

    public void TakeScreenshot()
    {

        //RenderTexture.active = renderTexture;
        
        
        //cam.targetTexture = renderTexture;

        //cam.Render();
        //RenderTexture.active = renderTexture;
        //texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //texture2D.Apply();
        //cam.targetTexture = null;
        //RenderTexture.active = null; // JC: added to avoid errors
        ////Destroy(rt);

        //rawImage.texture = texture2D;
    }
}
