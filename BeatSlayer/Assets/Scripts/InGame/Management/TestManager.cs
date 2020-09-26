using InGame.SceneManagement;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ProjectManagement;
using UnityEngine;

namespace Testing
{
    public static class TestManager
    {
        public static MenuScript_v2 menu;
        
        public static string EditorFolderPath
        {
            get
            {
#if !UNITY_EDITOR
                return new DirectoryInfo(Application.persistentDataPath).Parent.Parent + "/com.REDIZIT.BeatSlayerEditor/files";
#else
                return new DirectoryInfo(Application.persistentDataPath).Parent + "/Beat Slayer Editor";
#endif
                
            }
        }

        public static void Setup(MenuScript_v2 m)
        {
            menu = m;
        }

        public static void CheckUpdates()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/data/moderation")) return;
            if (!File.Exists(Application.persistentDataPath + "/data/moderation/request.json")) return;

            TestRequest request = LoadRequest();

            string filepath = "";
            if(request.type == TestType.OwnMap)
            {
                File.Delete(Application.persistentDataPath + "/data/moderation/request.json");
                
                filepath = EditorFolderPath + "/Maps/" + request.trackname + "/" + request.trackname + ".bsu";
                request.filepath = filepath;

                SceneloadParameters parameters = SceneloadParameters.EditorTestPreset(request);

                SceneController.instance.LoadScene(parameters);
            }
        }

        public static void CheckModerationUpdates()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/data/moderation")) return;
            if (!Directory.Exists(Application.persistentDataPath + "/data/moderation/map")) return;
            if (!File.Exists(Application.persistentDataPath + "/data/moderation/request.json")) return;

            if (menu.beatmapUI.overlay.activeSelf) return;
            
            TestRequest request = LoadRequest();

            string filepath = "";
            if (request.type == TestType.ModerationMap)
            {
                filepath = Application.persistentDataPath + "/data/moderation/map/" + request.trackname + ".bsu";
                Debug.Log("Moderation Map trackname is " + filepath);

                request.filepath = filepath;

                Project proj = ProjectManager.LoadProject(filepath);

                if (proj.difficulties == null || proj.difficulties.Count == 0)
                {
                    proj = ProjectUpgrader.UpgradeToDifficulty(proj);
                }
                
                menu.beatmapUI.OpenModeration(request, proj);
                DeleteRequest(false);
            }
        }

        public static TestRequest LoadRequest()
        {
            string filepath = Application.persistentDataPath + "/data/moderation/request.json";
            TestRequest request = JsonConvert.DeserializeObject<TestRequest>(File.ReadAllText(filepath));
            return request;
        }

        public static void DeleteRequest(bool withmap)
        {
            string requestPath = Application.persistentDataPath + "/data/moderation/request.json";
            string mapFolder = Application.persistentDataPath + "/data/moderation/map";
            
            File.Delete(requestPath);

            if (withmap)
            {
                foreach (var filepath in Directory.GetFiles(mapFolder))
                {
                    File.Delete(filepath);
                }
            }
        }

        public static void OpenEditor()
        {
            if (Application.isEditor) { Debug.Log("Open BS"); return; }

            bool fail = false;
            string bundleId = "com.REDIZIT.BeatSlayerEditor";
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
            {
                Debug.LogError("OpenEditor() failed");
            }
            else 
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
        public string filepath;
        public int difficultyId;
        
        public TestRequest(TestType type, string trackname, int difficultyId)
        {
            this.type = type;
            this.trackname = trackname;
            this.difficultyId = difficultyId;
        }

        public TestRequest() { }
    }

    public enum TestType
    {
        OwnMap, ModerationMap
    }
}