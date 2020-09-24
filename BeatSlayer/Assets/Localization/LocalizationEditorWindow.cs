using Assets.SimpleLocalization;
using GUIExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Localization
{
#if UNITY_EDITOR

    public class LocalizationEditorWindow : EditorWindow
    {
        private string searchString;
        private GUITableState tableState;
        private bool focused;
        private static Action<string> callback;

        private readonly List<List<TableEntry>> rows = new List<List<TableEntry>>();
        private readonly List<TableColumn> columns = new List<TableColumn>();



        [MenuItem("Window/Localiztion")]
        public static void Init()
        {
            var window = GetWindow(typeof(LocalizationEditorWindow));
            window.minSize = new Vector2(900, 700);
            window.titleContent = new GUIContent("Localization Manager");
            window.Show();

            if (LocalizationManager.Dictionary == null) return;
            LocalizationManager.Read();
        }
        public static void SelectKey(Action<string> callback)
        {
            LocalizationEditorWindow.callback = callback;
            Init();
        }

        private void OnGUI()
        {
            DrawHeader();

            DrawTable();
        }

        private void DrawHeader()
        {
            GUI.SetNextControlName("searchBox");
            searchString = GUILayout.TextField(searchString);
            if (!focused)
            {
                EditorGUI.FocusTextInControl("searchBox");
                focused = true;
            }


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Read file", GUILayout.ExpandWidth(false)))
            {
                LocalizationManager.Read(true);
            }
            GUILayout.EndHorizontal();
        }



        private void DrawTable()
        {
            if (LocalizationManager.Dictionary == null) return;


            rows.Clear();
            columns.Clear();
            columns.Add(new TableColumn("Key", 100));

            // Step 1: reformat LocalizationManager dictioanary for grid available format
            // Manager has Dictionary<lang, Dictionary<key, word>> (speed up for in game)
            // Grid should has Dictionary<key, List<word>> (to create rows)
            Dictionary<string, List<string>> keyTranslationsDictionary = new Dictionary<string, List<string>>();

            // Go thro all languages
            foreach (KeyValuePair<string, Dictionary<string, string>> langItemPair in LocalizationManager.Dictionary)
            {
                // Add column for this language
                columns.Add(new TableColumn(langItemPair.Key, 300));

                // Go thro all keys (<key, wordOnThisLanguage>) in this language
                foreach (KeyValuePair<string, string> item in langItemPair.Value)
                {
                    if (!IsPassingSearch(item.Key)) continue;

                    // Simply add to grid format dictionary
                    if (keyTranslationsDictionary.ContainsKey(item.Key))
                    {
                        keyTranslationsDictionary[item.Key].Add(item.Value);
                    }
                    else
                    {
                        keyTranslationsDictionary.Add(item.Key, new List<string> { item.Value });
                    }
                }
            }

            // Step 2: Building a grid
            // Just go thro prepaired dictionary
            foreach (var keyTranslationPair in keyTranslationsDictionary)
            {
                // Row is List of TableEntry inherits
                List<TableEntry> row = new List<TableEntry>
                {
                    // Add key column row value
                    new LabelEntry(keyTranslationPair.Key)
                };

                // Go thro all translated words and add them to row
                foreach (string tranlsation in keyTranslationPair.Value)
                {
                    row.Add(new LabelEntry(tranlsation));
                }

                row.Add(new ActionEntry("Use", () =>
                {
                    OnUseBtnClick(keyTranslationPair.Key);
                }));

                rows.Add(row);
            }

            columns.Add(new TableColumn("Actions", 100));

            // Finally, draw table
            tableState = GUITable.DrawTable(columns, rows, tableState);
        }



        private void OnUseBtnClick(string key)
        {
            callback?.Invoke(key);
            callback = null;
            GetWindow<LocalizationEditorWindow>().Close();
        }




        private bool IsPassingSearch(string wordToCheck)
        {
            if (string.IsNullOrEmpty(wordToCheck)) return true;
            if (string.IsNullOrEmpty(searchString)) return true;

            return wordToCheck.ToLower().Contains(searchString.ToLower());
        }
    }

#endif
}
