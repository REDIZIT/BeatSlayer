using Localization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.SimpleLocalization
{
    public static class CustomLocalizationManager
    {
        public static Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
        private static string folderPath = "GameLocalization/Global";

        static CustomLocalizationManager()
        {
            
        }

        public static void ReadFile()
        {
            var assets = Resources.LoadAll<TextAsset>(folderPath);

            Stopwatch w = Stopwatch.StartNew();
            //w.Stop();

            dictionary = new Dictionary<string, Dictionary<string, string>>();
            foreach (TextAsset asset in assets)
            {
                //w.Start();
                LocalizationSheet sheet = JsonConvert.DeserializeObject<LocalizationSheet>(asset.text);
                //w.Stop();

                foreach (LocalizationItem item in sheet.dictionary)
                {
                    if (dictionary.ContainsKey(item.key))
                    {
                        Debug.LogWarning($"Localization item with key {item.key} already exists");
                        continue;
                    }
                    dictionary.Add(item.key, item.translations);
                }
            }


            Debug.Log($"ReadFile end in {w.ElapsedMilliseconds}ms");
        }
    }
}
