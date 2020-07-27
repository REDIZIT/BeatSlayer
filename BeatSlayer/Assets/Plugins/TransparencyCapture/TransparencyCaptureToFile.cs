using System.Collections;
using UnityEngine;

/// <summary>
/// Can save bloom effect but saber becomes semi-transparent
/// </summary>
public class TransparencyCaptureToFile:MonoBehaviour
{
    public IEnumerator capture(string filepath)
    {

        yield return new WaitForEndOfFrame();
        //After Unity4,you have to do this function after WaitForEndOfFrame in Coroutine
        //Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"
        zzTransparencyCapture.captureScreenshot(filepath);
    }
}