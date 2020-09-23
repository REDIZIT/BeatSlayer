using Localization;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.SimpleLocalization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedTextMesh : MonoBehaviour
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
            GetComponent<TextMeshProUGUI>().text = LocalizationManager.Localize(LocalizationKey);
        }
    }

    [CustomEditor(typeof(LocalizedTextMesh))]
    public class LocalizedTextMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LocalizedTextMesh targetComponent = (LocalizedTextMesh)target;

            targetComponent.LocalizationKey = EditorGUILayout.TextField("Localization key", targetComponent.LocalizationKey);

            if (!LocalizationManager.HasLocalization(targetComponent.LocalizationKey))
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("There is no such key!", style);
            }

            if (GUILayout.Button("Select key"))
            {
                //EditorWindow.GetWindow(typeof(LocalizationEditorWindow)).Show();
                LocalizationEditorWindow.SelectKey((string selectedKey) =>
                {
                    targetComponent.LocalizationKey = selectedKey;
                });
            }
        }
    }
}