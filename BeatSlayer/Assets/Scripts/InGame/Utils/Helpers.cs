using System;
using System.Collections;
using System.Collections.Generic;
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

        public static void FillContent<TItem, TData>(Transform content, GameObject prefab, IEnumerable<TData> list, Action<TItem, TData> implementation)
        {
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



        public static void RefreshContent<TItem, TData>(Transform content, IEnumerable<TData> list, Action<TItem, TData> implementation, int startIndex = 1)
        {
            int index = startIndex;
            foreach (TData infoClass in list)
            {
                TItem itemUI = content.GetChild(index).GetComponent<TItem>();
                implementation(itemUI, infoClass);

                index++;
            }
        }
        public static void RefreshContentItem<TItem>(Transform content, int index, Action<TItem> implementation)
        {
            Transform child = content.GetChild(index);
            implementation(child.GetComponent<TItem>());
        }

        /// <summary>
        /// Get first child in content and use it as prefab in <see cref="AddContent{T}(Transform, GameObject, Action{T})"/>
        /// </summary>
        /// <typeparam name="TItem">Item MonoBehaviour class</typeparam>
        public static void AddContent<TItem>(Transform content, Action<TItem> implementation)
        {
            GameObject prefab = GetFirst(content).gameObject;
            AddContent(content, prefab, implementation);
        }
        /// <summary>
        /// Add to content prefab and implement data model refreshing in this prefab by implementation
        /// </summary>
        /// <typeparam name="TItem">Item MonoBehaviour class</typeparam>
        public static void AddContent<TItem>(Transform content, GameObject prefab, Action<TItem> implementation)
        {
            prefab.SetActive(true);

            GameObject item = Instantiate(prefab, content);
            item.SetActive(true);
            implementation(item.GetComponent<TItem>());

            prefab.SetActive(false);
        }


        public static void AddOrRefresh<TItem>(Transform content, GameObject prefab, int index, Action<TItem> implementation)
        {
            Transform item = content.GetChild(index);
            if(item == null)
            {
                AddContent<TItem>(content, prefab, implementation);
            }
            else
            {
                RefreshContentItem<TItem>(content, index, implementation);
            }
        }



        public static GameObject ClearContent(Transform content)
        {
            Transform prefabTransform = GetFirst(content);
            foreach(Transform child in content) if (child != prefabTransform) Destroy(child.gameObject);
            return prefabTransform.gameObject;
        }
        public static void ClearContentAll(Transform content)
        {
            foreach (Transform child in content) Destroy(child.gameObject);
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







        private static Transform GetFirst(Transform content)
        {
            return content.GetChild(0);
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