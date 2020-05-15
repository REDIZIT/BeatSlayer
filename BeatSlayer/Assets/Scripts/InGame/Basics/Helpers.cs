using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using ProjectManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Helpers
{
    public static class Helpers
    {
        private static WebClient c = new WebClient();
        
        public const string url_downloadMap = "http://176.107.160.146/Home/DownloadProject?trackname={0}&nickname={1}";
        
        public static void DownloadMap(string trackname, string nick, Action<DownloadProgressChangedEventArgs> progressCallback, Action<AsyncCompletedEventArgs> completeCallback)
        {
            string url = string.Format(url_downloadMap, trackname.Replace("&", "%amp%"), nick.Replace("&", "%amp%"));
            
            if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");
            string tempPath = Application.persistentDataPath + "/temp/" + trackname + ".bsz";
            
            c = new WebClient();
            c.DownloadProgressChanged += (sender, args) =>
            {
                progressCallback(args);
            };
            c.DownloadFileCompleted += (sender, args) =>
            {
                bool doUnpack = false;
                
                if(args.Cancelled) Debug.Log("Download cancelled");
                else if (args.Error != null) Debug.LogError("Download error\n" + args.Error);
                else
                {
                    doUnpack = true;
                }

                if (doUnpack)
                {
                    ProjectManager.UnpackBspFile(tempPath);
                }
                else
                {
                    File.Delete(tempPath);
                }
                
                completeCallback(args);
                
                // -1 coz of Difficulty has no Downloads field 
                DatabaseScript.SendStatistics(trackname, nick, -1, DatabaseScript.StatisticsKeyType.Download);
            };
            
            c.DownloadFileAsync(new Uri(url), tempPath);
        }

        public static void CancelDownloading()
        {
            c.CancelAsync();
        }
    }

    public class HelperUI : MonoBehaviour
    {
        /// <summary>
        /// Fill content with list and make some implementation
        /// </summary>
        /// <param name="content">Content transform</param>
        /// <param name="list">List of classes</param>
        /// <param name="implementation"></param>
        /// <typeparam name="T">Content child UI class</typeparam>
        /// <typeparam name="T2">Info class which needs to implement into UI</typeparam>
        public static void FillContent<T, T2>(Transform content, IEnumerable<T2> list, Action<T, T2> implementation)
        {
            GameObject prefab = ClearContent(content);
            prefab.SetActive(true);

            foreach (T2 infoClass in list)
            {
                T itemUI = Instantiate(prefab, content).GetComponent<T>();
                implementation(itemUI, infoClass);
            }
        
            prefab.SetActive(false);
        }
    
        public static void FillContent(Transform content, int count, Action<GameObject, int> implementation)
        {
            GameObject prefab = ClearContent(content);
            prefab.SetActive(true);

            for (int i = 0; i < count; i++)
            {
                GameObject item = Instantiate(prefab, content);
                implementation(item, i);
            }

            prefab.SetActive(false);
        }

        public static void AddContent<T>(Transform content, GameObject prefab, Action<T> implementation)
        {
            GameObject item = Instantiate(prefab, content);
            item.SetActive(true);
            implementation(item.GetComponent<T>());
        }

        public static GameObject ClearContent(Transform content)
        {
            Transform prefabTransform = content.GetChild(0);
            foreach(Transform child in content) if (child != prefabTransform) Destroy(child.gameObject);
            return content.GetChild(0).gameObject;
        }
        
        public static void ColorInputField(InputField field, Color clr)
        {
            field.GetComponent<Image>().color = clr;
        }
        public static void ColorInputField(InputField field, bool isCorrect)
        {
            field.GetComponent<Image>().color = isCorrect
                ? new Color32(45,45,45,255) 
                : new Color32(120,0,0,255);
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Converts all imported textures into Sprite type
    /// </summary>
    public class TextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.maxTextureSize = 512;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
 
            int startIndex = assetPath.LastIndexOf("/") + 1;
            string assetName = assetPath.Substring(startIndex, assetPath.Length - startIndex);
         
            textureImporter.textureType = TextureImporterType.Sprite;
        }
    }
    #endif
}