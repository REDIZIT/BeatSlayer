using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomInputField : InputField
{
    public Action onEndEdit;
    public void OnEndEdit()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            onEndEdit();
        }
    }
}
