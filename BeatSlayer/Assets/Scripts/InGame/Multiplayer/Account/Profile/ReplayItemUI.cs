using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using UnityEngine;
using UnityEngine.UI;

public class ReplayItemUI : MonoBehaviour
{
    public ReplayData data;
    public Text mapText, nickText, scoreText, missedText, accuracyText, rpText;

    public void Refresh(ReplayData data)
    {
        this.data = data;

        mapText.text = data.Map.Group.Author + "-" + data.Map.Group.Name;
        nickText.text = "by " + data.Map.Nick + " <color=#e80>" + data.DifficultyName + "</color>";
        scoreText.text = data.Score + "";
        missedText.text = data.Missed + "";
        rpText.text = data.RP + "";
        accuracyText.text = (Mathf.FloorToInt(data.Accuracy * 10000) / 100f) + "";
    }
}
