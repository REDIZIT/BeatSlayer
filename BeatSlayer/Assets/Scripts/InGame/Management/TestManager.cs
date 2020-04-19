using InGame.SceneManagement;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Testing
{
    public static class TestManager
    {
        public static string EditorFolderPath
        {
            get
            {
                if (Application.isEditor) return new DirectoryInfo(Application.persistentDataPath).Parent + "/Beat Slayer Editor";
                return new DirectoryInfo(Application.persistentDataPath).Parent.Parent + "/com.REDIZIT.BeatSlayerEditor/files";
            }
        }


        public static void CheckUpdates()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/data/moderation")) return;
            if (!File.Exists(Application.persistentDataPath + "/data/moderation/request.json")) return;

            TestRequest request = LoadRequest();
            File.Delete(Application.persistentDataPath + "/data/moderation/request.json");

            string filepath = "";
            if(request.type == TestType.ModerationMap) filepath = Application.persistentDataPath + "/data/moderation/" + request.trackname + ".bsz";
            else
            {
                filepath = EditorFolderPath + "/Maps/" + request.trackname + "/" + request.trackname + ".bsu";
            }

            SceneloadParameters parameters;
            if (request.type == TestType.ModerationMap) parameters = SceneloadParameters.ModerationPreset(filepath);
            else parameters  = SceneloadParameters.EditorTestPreset(filepath);

            SceneController.instance.LoadScene(parameters);
        }
        public static TestRequest LoadRequest()
        {
            string filepath = Application.persistentDataPath + "/data/moderation/request.json";
            TestRequest request = JsonConvert.DeserializeObject<TestRequest>(File.ReadAllText(filepath));
            return request;
        }

        public static void OpenEditor()
        {
            if (Application.isEditor) { Debug.Log("Open BS"); return; }

            bool fail = false;
            string bundleId = "com.REDIZIT.BeatSlayerEditor"; // your target bundle id
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = null;
            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
            }
            catch (System.Exception e)
            {
                fail = true;
            }

            if (fail)
            { //open app in store
                Debug.LogError("OpenEditor() failed");
                //Application.OpenURL("https://google.com");
            }
            else //open the app
                ca.Call("startActivity", launchIntent);

            up.Dispose();
            ca.Dispose();
            packageManager.Dispose();
            launchIntent.Dispose();
        }
    }


    public class TestRequest
    {
        public TestType type;
        public string trackname;

        public TestRequest(TestType type)
        {
            this.type = type;
        }
        public TestRequest(TestType type, string trackname)
        {
            this.type = type;
            this.trackname = trackname;
        }

        public TestRequest() { }
    }

    public enum TestType
    {
        OwnMap, ModerationMap
    }
}