using System.Collections;
using System.Collections.Generic;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ChatGroupItemUI : MonoBehaviour
{
    public ChatUI ui;
    public ChatGroupData data;
    public bool changing;

    public RawImage image;
    public Toggle toggle;
    public Text label;

    public void Refresh(ChatGroupData data)
    {
        this.data = data;
        label.text = data.Name;

        changing = true;
        toggle.isOn = ui.selectedGroupName == data.Name;
        changing = false;
    }

    public void OnGroupBtnClick()
    {
        if (!toggle.isOn || changing) return;
        ui.JoinGroup(data);
    }
}
