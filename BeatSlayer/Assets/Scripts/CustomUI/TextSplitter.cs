using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[CustomEditor(typeof (TextSplitter))]
public class TextSplittereEditor : Editor
{
    GameObject leftLine, rightLine;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextSplitter script = (TextSplitter)target;

        script.UpdateThis();

        leftLine = script.leftLine;
        rightLine = script.rightLine;

        if (!script.enabled)
        {
            if(leftLine != null) DestroyImmediate(leftLine);
            if(rightLine != null) DestroyImmediate(rightLine);
        }
    }


    // When remove component TextSplitterScript from object destroy lines
    private void OnDestroy()
    {
        if(target == null)
        {
            DestroyImmediate(leftLine);
            DestroyImmediate(rightLine);
        }
    }
}
#endif



[RequireComponent(typeof(Text))]
public class TextSplitter : MonoBehaviour
{
    public Text text
    {
        get
        {
            return GetComponent<Text>();
        }
    }

    [Header("Format")]
    public float paddingFromText = 70;
    public float paddingFromEdge = 100;
    public float width = 3;
    public float yOffset = -2;

    [Header("Color")]
    public bool copyFromText = true;
    public Color color = Color.white;

    [Header("Lines")]
    public GameObject leftLine;
    public GameObject rightLine;

    string prevText;


    private void Start()
    {
        UpdateThis();
        prevText = text.text;
    }

    private void FixedUpdate()
    {
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
        if (enabled)
        {
            if (leftLine == null)
            {
                leftLine = new GameObject("LeftLine");
            }
            UpdateLine(leftLine, true);

            if (rightLine == null)
            {
                rightLine = new GameObject("RightLine");
            }
            UpdateLine(rightLine, false);
        }
    }

    public void UpdateLine(GameObject line, bool isLeft)
    {
        line.transform.SetParent(transform);
        if (line.GetComponent<Image>() == null)
        {
            line.AddComponent<Image>();
        }
        line.GetComponent<Image>().color = copyFromText ? text.color : color;

        float paddingRight = text.preferredWidth / 2f + paddingFromText;
        float paddingLeft = paddingFromEdge;

        line.GetComponent<RectTransform>().anchorMin = new Vector2(isLeft ? 0 : 0.5f, 0.5f);
        line.GetComponent<RectTransform>().anchorMax = new Vector2(isLeft ? 0.5f : 1, 0.5f);

        Vector2 v1 = new Vector2(isLeft ? paddingLeft : paddingRight, 0);
        Vector2 v2 = new Vector2(isLeft ? -paddingRight : -paddingLeft, width);
        line.GetComponent<RectTransform>().offsetMin = v1;
        line.GetComponent<RectTransform>().offsetMax = v2;

        line.GetComponent<RectTransform>().anchoredPosition = new Vector2(line.GetComponent<RectTransform>().anchoredPosition.x, yOffset);


        line.transform.localScale = new Vector3(1, 1, 1);
    }
}