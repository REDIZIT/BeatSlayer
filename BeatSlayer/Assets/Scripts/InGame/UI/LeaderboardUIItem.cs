using LeaderboardManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class LeaderboardUIItem : MonoBehaviour
{
    //public LeaderboardRecord record;

    public Text placeText, nickText, accuracyText, missedText, playedTimesText, RPText, totalRPText;
    public Text scoreText;
    public Replay replay;
    public LeaderboardItem leaderboardItem;

    [Header("Colors")]
    public Color32 defaultBodyColor;
    public Color32 defaultBorderColor;
    public Color32 defaultTextColor;

    public Color32 selectedBodyColor, selectedBorderColor;
    public Color32 selectedTextColor;



    public void Refresh(int place, bool isCurrentPlayer)
    {
        placeText.text = "#" + place;
        nickText.text = replay.player;

        float roundedAccuray = Mathf.FloorToInt(replay.Accuracy * 1000f) / 10f;
        accuracyText.text = roundedAccuray + "%";
        accuracyText.color = isCurrentPlayer ? selectedTextColor : defaultTextColor;

        missedText.text = replay.missed.ToString();
        missedText.color = isCurrentPlayer ? selectedTextColor : defaultTextColor;

        float RP = Mathf.FloorToInt((float)replay.RP * 10f) / 10f;
        RPText.text = RP.ToString();

        scoreText.text = Mathf.FloorToInt(replay.score).ToString();

        GetComponent<UICornerCut>().color = isCurrentPlayer ? selectedBodyColor : defaultBodyColor;
        GetComponent<UICornerCut>().ColorDown = isCurrentPlayer ? selectedBorderColor : defaultBorderColor;
    }
    public void RefreshLeaderboardItem()
    {
        nickText.text = leaderboardItem.nick;
        placeText.text = "#" + leaderboardItem.place;

        accuracyText.text = Mathf.FloorToInt(leaderboardItem.Accuracy * 1000f) / 10f + "%";
        playedTimesText.text = leaderboardItem.playCount + "";
        RPText.text = Math.Floor(leaderboardItem.RP * 10) / 10f + "";
        scoreText.text = Math.Floor(leaderboardItem.score * 10) / 10f + "";
    }
}