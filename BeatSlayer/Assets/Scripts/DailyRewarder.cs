using Assets.SimpleLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewarder : MonoBehaviour
{
    public GameObject yesterdayItem, todayItem, tomorrowItem;
    public GameObject yesterdayPoint, tomorrowPoint;

    //PrefsManager prefsManager
    //{
    //    get { return Camera.main.GetComponent<PrefsManager>(); }
    //}
    AdvancedSaveManager prefsManager
    {
        get { return Camera.main.GetComponent<AdvancedSaveManager>(); }
    }

    public int[] rewards;

    public int day;

    /*public void Calculate()
    {
        DateTime prevGame = prefsManager.prefs.prevPlay;
        if (prevGame.Day != DateTime.Now.Day)
        {
            transform.gameObject.SetActive(true);
            day = prefsManager.prefs.daysPlayed + 1;
            prefsManager.prefs.prevPlay = DateTime.Now;
        }

        if (day > 7) day = 1;
        prefsManager.prefs.daysPlayed = day;
        prefsManager.Save();

        if (day == 1)
        {
            yesterdayItem.gameObject.SetActive(false);
            yesterdayPoint.gameObject.SetActive(false);
        }
        else if (day == 7)
        {
            tomorrowItem.gameObject.SetActive(false);
            tomorrowPoint.gameObject.SetActive(false);
        }

        if(day > 1)
        {
            yesterdayItem.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = rewards[day - 2] + " " + LocalizationManager.Localize("Coins");
        }
        

        todayItem.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = day + "";
        todayItem.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = rewards[day - 1] + " " + LocalizationManager.Localize("Coins");

        tomorrowItem.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = day + 1 + "";
    }

    public void Claim()
    {
        todayItem.GetComponent<Animator>().Play("DailyReward_Today");

        int coins = prefsManager.prefs.coins;
        prefsManager.prefs.coins = coins + rewards[day - 1];

        Camera.main.GetComponent<MenuScript_v2>().coinsTexts[0].text = prefsManager.prefs.coins + "";
        Camera.main.GetComponent<MenuScript_v2>().coinsTexts[1].text = prefsManager.prefs.coins + "";

        prefsManager.Save();
    }*/
}