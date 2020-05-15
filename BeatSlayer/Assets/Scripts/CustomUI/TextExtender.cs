using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[CustomEditor(typeof(TextExtender))]
public class TextExtenderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextExtender script = (TextExtender)target;
        script.UpdateThis();
    }

    private void OnDestroy()
    {
        
    }
}
#endif



public class TextExtender : MonoBehaviour
{
    [Tooltip("Text component which will extend this object")]
    public Text text;
    string prevText;

    [Space]
    public Vector2 margin;


    private void Awake()
    {
        UpdateThis();
        prevText = text.text;
    }

    private void Update()
    {
        if (!enabled) return;

        if (text.text != prevText)
        {
            prevText = text.text;
            UpdateThis();
        }
    }

    private void OnDisable()
    {

    }

    public void UpdateThis()
    {
        if (!enabled) return;

        RectTransform rect = GetComponent<RectTransform>();

        float width = text.preferredWidth + margin.x * 2;
        rect.sizeDelta = new Vector2(width, 0);
        
        //text.Rebuild(CanvasUpdate.MaxUpdateValue);

        float height = text.preferredHeight + margin.y * 2;
        rect.sizeDelta = new Vector2(width, height);
    }

}