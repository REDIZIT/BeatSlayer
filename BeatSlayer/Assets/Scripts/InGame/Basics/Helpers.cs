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
    public class HelperUI : MonoBehaviour
    {
        /// <summary>
        /// Fill content with list and make some implementation
        /// </summary>
        /// <param name="content">Content transform</param>
        /// <param name="list">List of classes</param>
        /// <param name="implementation"></param>
        /// <typeparam name="TItem">Content child UI class</typeparam>
        /// <typeparam name="TData">Info class which needs to implement into UI</typeparam>
        public static void FillContent<TItem, TData>(Transform content, IEnumerable<TData> list, Action<TItem, TData> implementation)
        {
            GameObject prefab = ClearContent(content);
            prefab.SetActive(true);

            foreach (TData infoClass in list)
            {
                TItem itemUI = Instantiate(prefab, content).GetComponent<TItem>();
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