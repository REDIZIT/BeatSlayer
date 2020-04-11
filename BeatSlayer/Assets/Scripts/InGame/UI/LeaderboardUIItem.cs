using LeaderboardManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUIItem : MonoBehaviour
{
    public LeaderboardRecord record;

    public Text placeText, nickText, accuracyText, playedTimesText, RPText, totalRPText;


    public void Refresh()
    {
        placeText.text = "#" + record.place;
        nickText.text = record.nick;

        float roundedAccuray = Mathf.FloorToInt(record.accuracy * 100f) / 100f;
        accuracyText.text = roundedAccuray + "%";

        playedTimesText.text = record.playedTimes.ToString();

        int RP = Mathf.FloorToInt(record.RP);
        RPText.text = RP.ToString();

        int totalRP = Mathf.FloorToInt(record.totalRP);
        totalRPText.text = totalRP.ToString();
    }
}