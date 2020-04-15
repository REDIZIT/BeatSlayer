using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeSlider : MonoBehaviour
{
    public Slider leftSlider, rightSlider;
    public RectTransform handleTransform;

    public RectTransform leftHandleRect, rightHandleRect;

    public void OnLeftSliderChange()
    {
        float newValue = leftSlider.value >= rightSlider.value ? rightSlider.value - 1 : leftSlider.value;

        leftSlider.value = newValue;

        RefreshHandle();
    }

    public void OnRightSliderChange()
    {
        float newValue = rightSlider.value <= leftSlider.value ? leftSlider.value + 1 : rightSlider.value;

        rightSlider.value = newValue;

        RefreshHandle();
    }

    void RefreshHandle()
    {
        float x = leftHandleRect.anchoredPosition.x;
        float width = rightHandleRect.anchoredPosition.x - leftHandleRect.anchoredPosition.x;

        Debug.Log(width);

        //handleTransform.sizeDelta = new Vector2(width, 30);
        handleTransform.anchoredPosition = new Vector2(x, 0);
    }
}
