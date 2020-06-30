#if UNITY_EDITOR

using UnityEditor;
using System.IO;
using UnityEngine;

/*
 * Unity Forum link:
 * https://forum.unity.com/threads/c-script-template-how-to-make-custom-changes.273191/
*/




/// <summary>
/// Changes script template.
/// </summary>
/// Path to template C:\Program Files\Unity\Hub\Editor\2019.4.0f1\Editor\Data\Resources\ScriptTemplates\81-C# Script-NewBehaviourScript.cs.txt
/// Backup of default C:\Program Files\Unity\Hub\Editor\2019.4.0f1\Editor\Data\Resources\ScriptTemplates\81-C# Script-NewBehaviourScript.cs-backup.txt
public class ScriptTemplatePreprocessor : UnityEditor.AssetModificationProcessor
{
    /// <summary>
    /// Count of folders to remove (Remove Assets/Script folders for example)
    /// </summary>
    public static int FirstPartsSkipCount = 2;

    public static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta", "");

        // Skip folders and not .cs files
        if (Path.GetExtension(path) != ".cs") return;


        string clearPath = path.Replace(@"//", "/").Replace(@"\\", "/").Replace(@"\", "/");
        string[] parts = clearPath.Split('/');

        string scriptNamespace = string.Join(".", parts, FirstPartsSkipCount, parts.Length - FirstPartsSkipCount - 1);

        string fileContent = File.ReadAllText(path);

        fileContent = fileContent.Replace("#NAMESPACE#", scriptNamespace);

        File.WriteAllText(path, fileContent);
        AssetDatabase.Refresh();
    }
}

#endif