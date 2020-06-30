using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.SimpleLocalization
{
	/// <summary>
	/// Localization manager.
	/// </summary>
    public static class LocalizationManager
    {
		/// <summary>
		/// Fired when localization changed.
		/// </summary>
        public static event Action LocalizationChanged = () => { }; 

        private static readonly Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();
        public static string _language = "Unknown";
        public static string folderPath = "GameLocalization/Master";

		/// <summary>
		/// Get or set language.
		/// </summary>
        public static string Language
        {
            get { return _language; }
            set { _language = value; LocalizationChanged(); }
        }

		/// <summary>
		/// Read localization spreadsheets.
		/// </summary>
		public static void Read()
        {
            Debug.Log("Read with " + Language);

            if (Dictionary.Count > 0) return;

            var textAssets = Resources.LoadAll<TextAsset>(folderPath);

            foreach (var textAsset in textAssets)
            {
                var text = ReplaceMarkers(textAsset.text);
                var matches = Regex.Matches(text, "\"[\\s\\S]+?\"");
                


                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var languages = lines[0].Split('|').Select(i => i.Trim()).ToList();


                for (var i = 1; i < languages.Count; i++)
                {
                    if (!Dictionary.ContainsKey(languages[i]))
                    {
                        Dictionary.Add(languages[i], new Dictionary<string, string>());
                    }
                }

                for (var i = 1; i < lines.Length; i++)
                {
					var columns = lines[i].Split('|').Select(j => j.Trim()).Select(j => j.Replace("[N]","\n")).ToList();
					var key = columns[0];

                    for (var j = 1; j < languages.Count; j++)
                    {
                        if(key.Contains("//") || key == "" || key == "\n" || key == @"
" || key == " ") continue;
                        try
                        {
                            Dictionary[languages[j]].Add(key, columns[j]);
                        }
                        catch (Exception err)
                        {
                            Debug.LogWarning("Line " + key + " => " + err.Message);
                            Dictionary[languages[j]][key] = columns[j];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey)
        {
            if (Dictionary.Count == 0)
            {
                Read();
            }

            if (!Dictionary.ContainsKey(Language)) { Debug.LogError("Language not found: " + Language); return "[ERR Lang]"; }
            if (!Dictionary[Language].ContainsKey(localizationKey)) { Debug.LogError("Translation not found: " + localizationKey); return "[ERR Tkey]"; }

            return Dictionary[Language][localizationKey];
        }

	    /// <summary>
	    /// Get localized value by localization key.
	    /// </summary>
		public static string Localize(string localizationKey, params object[] args)
        {
            var pattern = Localize(localizationKey);

            return string.Format(pattern, args);
        }

        private static string ReplaceMarkers(string text)
        {
            return text.Replace("[Newline]", "\n");
        }
    }
}