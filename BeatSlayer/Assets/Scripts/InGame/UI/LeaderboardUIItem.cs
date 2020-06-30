using System;
using InGame.Leaderboard;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// Used in player leaderboard and controlled by <see cref="PlayerLeaderboardUI"/>
/// </summary>
public class LeaderboardUIItem : MonoBehaviour
{
    public LeaderboardItem item;

    public Text placeText, nickText, accuracyText, missedText, playedTimesText, RPText, totalRPText;
    public Text scoreText;
    public UICornerCut cornerCut;

    [Header("Colors")]
    public Color32 defaultBodyColor;
    public Color32 defaultBorderColor;
    public Color32 defaultTextColor;

    public Color32 selectedBodyColor, selectedBorderColor;
    public Color32 selectedTextColor;



    public void Refresh(LeaderboardItem item, int place, bool isCurrentPlayer)
    {
        //Debug.Log(JsonConvert.SerializeObject(item, Formatting.Indented));

        this.item = item;
        placeText.text = "#" + place;
        nickText.text = item.Nick;

        float roundedAccuray = Mathf.FloorToInt(item.Accuracy * 1000f) / 10f;
        accuracyText.text = roundedAccuray + "%";
        ColorizeText(accuracyText, isCurrentPlayer);

        //missedText.text = item.MissedCount.ToString();
        //missedText.color = isCurrentPlayer ? selectedTextColor : defaultTextColor;

        float RP = Mathf.FloorToInt((float)item.RP * 10f) / 10f;
        RPText.text = RP.ToString();

        scoreText.text = Mathf.FloorToInt((float)item.Score).ToString();

        playedTimesText.text = item.PlayCount + "";
        ColorizeText(playedTimesText, isCurrentPlayer);

        cornerCut.color = isCurrentPlayer ? selectedBodyColor : defaultBodyColor;
        cornerCut.ColorDown = isCurrentPlayer ? selectedBorderColor : defaultBorderColor;
    }

    private void ColorizeText(Text text, bool isCurrentPlayer)
    {
        text.color = isCurrentPlayer ? selectedTextColor : defaultTextColor;
    }
}