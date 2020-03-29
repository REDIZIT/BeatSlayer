using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomUI : MonoBehaviour
{
    public enum Action { SetActive_True, SetActive_False, Size, SizeDeltaAndPos };
    public Action action;

    public Vector4 defaultSize, verticalSize;

    bool isVerticaled;

    private void Start()
    {
        Camera.main.GetComponent<MenuScript_v2>().resizableUI.Add(this);
        OnOrientationChange(Screen.height > Screen.width);
    }

    public void OnOrientationChange(bool isVertical)
    {
        if (action == Action.SetActive_True) gameObject.SetActive(isVertical);
        else if (action == Action.SetActive_False) gameObject.SetActive(!isVertical);
        else if (action == Action.Size)
        {
            SetLeft(isVertical ? verticalSize.x : defaultSize.x);
            SetRight(isVertical ? verticalSize.y : defaultSize.y);
        }
        else if (action == Action.SizeDeltaAndPos)
        {
            GetComponent<RectTransform>().sizeDelta = isVertical ? (Vector2)verticalSize : (Vector2)defaultSize;
            GetComponent<RectTransform>().anchoredPosition = isVertical ? new Vector2(verticalSize.z, verticalSize.w) : new Vector2(defaultSize.z, defaultSize.w);
        }
    }

    public void SetLeft(float left)
    {
        GetComponent<RectTransform>().offsetMin = new Vector2(left, GetComponent<RectTransform>().offsetMin.y);
    }

    public void SetRight(float right)
    {
        GetComponent<RectTransform>().offsetMax = new Vector2(-right, GetComponent<RectTransform>().offsetMax.y);
    }

    public void SetTop(float top)
    {
        GetComponent<RectTransform>().offsetMax = new Vector2(GetComponent<RectTransform>().offsetMax.x, -top);
    }

    public void SetBottom(float bottom)
    {
        GetComponent<RectTransform>().offsetMin = new Vector2(GetComponent<RectTransform>().offsetMin.x, bottom);
    }
}