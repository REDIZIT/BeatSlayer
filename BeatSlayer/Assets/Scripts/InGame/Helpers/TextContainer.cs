using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[CustomEditor(typeof(TextContainer))]
public class TextContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextContainer script = (TextContainer)target;
        script.UpdateThis();
    }

    private void OnDestroy() { }

}
#endif

public class TextContainer : MonoBehaviour
{
    [Tooltip("Text component which will extend this object")]
    public List<Text> texts;
    
    private RectTransform Rect
    {
        get { return GetComponent<RectTransform>(); }
    }
    List<string> prevTextsValue;

    
    public bool extendHorizont, extendVertical;

    public Vector2 padding;

    private Vector2 defaultRect;


    private void Awake()
    {
        defaultRect = Rect.sizeDelta;
        UpdateThis();
        prevTextsValue = texts.Select(c => c.text).ToList();
    }

    private void Update()
    {
        if (!enabled) return;

        bool update = false;
        for (int i = 0; i < texts.Count; i++)
        {
            if (prevTextsValue[i] != texts[i].text)
            {
                prevTextsValue[i] = texts[i].text;
                update = true;
            }
        }
        
        if (update) UpdateThis();
    }

    private void OnDisable()
    {

    }

    public void UpdateThis()
    {
        float maxWidth = 0;
        float maxHeight = 0;

        foreach (var text in texts)
        {
            float width = extendHorizont ? text.preferredWidth + padding.x * 2 : Rect.sizeDelta.x;
            float height = extendVertical ? text.preferredHeight + padding.y * 2 : Rect.sizeDelta.y;

            if (width > maxWidth) maxWidth = width;
            if (height > maxHeight) maxHeight = height;
        }

        Rect.sizeDelta = new Vector2(maxWidth, maxHeight);
        //text.Rebuild(CanvasUpdate.MaxUpdateValue);
    }

}