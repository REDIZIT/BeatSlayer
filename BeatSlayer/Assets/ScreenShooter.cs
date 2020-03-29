using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenShooter : MonoBehaviour
{
    bool shot = false;
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Screen Shot!");
                string fileName = Application.persistentDataPath + "/Screen.png";

                for (int i = 0; i < 25; i++)
                {
                    if(!File.Exists(Application.persistentDataPath + "/Screen" + i + ".png"))
                    {
                        fileName = Application.persistentDataPath + "/Screen" + i + ".png";
                        break;
                    }
                }

                ScreenCapture.CaptureScreenshot(fileName);
            }
        }
        else
        {
            if(Input.touches.Length > 2)
            {
                if (!shot)
                {
                    Debug.Log("Screen Shot!");
                    shot = true;
                    string fileName ="Screen.png";
                    for (int i = 0; i < 25; i++)
                    {
                        if (!File.Exists(Application.persistentDataPath + "/Screen" + i + ".png"))
                        {
                            fileName = "Screen" + i + ".png";
                            break;
                        }
                    }
                    ScreenCapture.CaptureScreenshot(fileName);
                }
                else
                {
                    Debug.Log("No screen shot");
                }
            }
            else
            {
                shot = false;
            }
        }
    }
}