using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstAprilScript : MonoBehaviour
{
    public Text[] titleTexts;
    public GameObject bannedPan, bannedLocker;
    public Button logOutBtn;
    public Text bannedInfoText;

    public GameObject[] toActivate;

    public AccountManager manager;
    public AuthManager authManager;

    public Sprite defaultAvatar;
     
    public void Troll()
    {
        if(DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
        {
            foreach (var item in titleTexts)
            {
                item.text = "<color=#FF2200>B</color>leat <color=#FF2200>S</color>asayer";
            }

            bannedPan.SetActive(true);
            bannedInfoText.text = $"Ray ID: 552aebbdf8d1b9f8\nAccount id: 217\nAccount nick: {manager.accountNick.text}\nReason: Cheat\nDate: 31.03.2020 23:19";

            manager.accountNick.text += "<color=#FF2200> [Banned]</color>";
            manager.avatar.sprite = defaultAvatar;

            bannedLocker.SetActive(true);
            logOutBtn.interactable = false;


            foreach (var item in toActivate)
            {
                item.SetActive(true);
            }
        }
    }
}
