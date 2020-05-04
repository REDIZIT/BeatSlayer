using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BossHelper : MonoBehaviour
{
    string bossUrl = "http://beatslayer-server/boss/boss.php";

    public Transform bossMini;
    public Text bossHeader, bossDescription;
    public Slider bossSlider;

    public Transform leadertable;
    MenuScript_v2 menu
    {
        get
        {
            return GetComponent<MenuScript_v2>();
        }
    }


    string bossname;
    int maxhp, hp;


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        bossMini.gameObject.SetActive(false);
        StartCoroutine(GetEventInfo());
    }

    public void OpenBossFightPan()
    {
        UpdateLeaders();
        UpdateBossinfo();
    }

    public IEnumerator GetEventInfo()
    {
        string url = bossUrl + "?action=getevent";
        WWW www = new WWW(url);

        yield return www;
        if (www.text != "")
        {
            string[] split = www.text.Replace("<br/>", "\n").Split('\n');
            bool isActive = bool.Parse(split[0]);
            bossname = split[1];
            maxhp = int.Parse(split[2]);
            hp = int.Parse(split[3]);

            bossMini.gameObject.SetActive(true);
            bossMini.GetComponent<Animator>().Play("BossShow");

            bossMini.GetChild(1).GetComponent<Text>().text = "Boss fight (" + bossname + ")";
            bossMini.GetChild(2).GetComponent<Slider>().maxValue = maxhp;
            bossMini.GetChild(2).GetComponent<Slider>().value = hp;
            bossMini.GetChild(2).GetChild(2).GetComponent<Text>().text = "HP  " + hp + " / " + maxhp;

            bossMini.GetChild(4).gameObject.SetActive(!isActive);
        }
    }

    void UpdateLeaders()
    {/*
        string url = bossUrl + "?action=get";
        WWW www = new WWW(url);

        while (!www.isDone) { }

        Debug.LogWarning(www.text);
        List<string> players = new List<string>();
        players.AddRange(www.text.Split('\n'));
        players.RemoveAt(players.Count - 1);

        players = players.OrderByDescending(c => int.Parse(c.Split(';')[2])).ToList();

        for (int i = 0; i < 3; i++)
        {
            Transform item = leadertable.GetChild(i);
            item.GetChild(1).GetComponent<Text>().text = players[i].Split(';')[0] + (players[i].Split(';')[1] == "g123" ? " <color=#555>(You)</color>" : "");
            item.GetChild(2).GetComponent<Text>().text = "Damage: " + players[i].Split(';')[2];
        }

        string mypos = players.Find(c => c.Split(';')[1] == menu.gpsId);
        int place = players.FindIndex(c => c == mypos) + 1;
        if(place > 3)
        {
            Transform item = leadertable.GetChild(3);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = place.ToString();
            item.GetChild(1).GetComponent<Text>().text = mypos.Split(';')[0] + " <color=#888>(You)</color>";
            item.GetChild(2).GetComponent<Text>().text = "Damage: " + mypos.Split(';')[2];
        }
        else
        {
            Transform item = leadertable.GetChild(3);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = "4";
            item.GetChild(1).GetComponent<Text>().text = players[3].Split(';')[0];
            item.GetChild(2).GetComponent<Text>().text = "Damage: " + players[3].Split(';')[2];
        }*/
    }
    void UpdateBossinfo()
    {
        string url = bossUrl + "?action=eventdetails&lang=" + (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Ukrainian ? "rus" : "eng");
        WWW www = new WWW(url);

        while (!www.isDone) { }

        bossHeader.text = bossname;
        bossDescription.text = www.text;
        bossSlider.maxValue = maxhp;
        bossSlider.value = hp;
        bossSlider.transform.GetChild(2).GetComponent<Text>().text = "HP  " + hp + " / " + maxhp;
    }
}
