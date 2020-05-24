using System.Collections;
using System.Collections.Generic;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ChatGroupItemUI : MonoBehaviour
{
    public ChatUI ui;
    public ChatGroupData data;

    public RawImage image;
    public Toggle toggle;
    public Text label;

    public void Refresh(ChatGroupData data)
    {
        this.data = data;
        label.text = data.Name;

        toggle.isOn = ui.selectedGroupName == data.Name;
    }

    public void OnGroupBtnClick()
    {
        if (!toggle.isOn) return;
        ui.JoinGroup(data);
    }
}
