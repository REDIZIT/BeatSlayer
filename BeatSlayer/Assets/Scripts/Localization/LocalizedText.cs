using Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleLocalization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string LocalizationKey;
        
        public void Start()
        {
            Localize();
            LocalizationManager.LocalizationChanged += Localize;
        }

        public void OnDestroy()
        {
            LocalizationManager.LocalizationChanged -= Localize;
        }

        private void Localize()
        {
            GetComponent<Text>().text = LocalizationManager.Localize(LocalizationKey);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LocalizedText targetComponent = (LocalizedText)target;

            targetComponent.LocalizationKey = EditorGUILayout.TextField("Localization key", targetComponent.LocalizationKey);

            if (!LocalizationManager.HasLocalization(targetComponent.LocalizationKey))
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("There is no such key!", style);
            }

            if (GUILayout.Button("Select key"))
            {
                LocalizationEditorWindow.SelectKey((string selectedKey) =>
                {
                    targetComponent.LocalizationKey = selectedKey;
                });
            }
        }
    }

#endif

}