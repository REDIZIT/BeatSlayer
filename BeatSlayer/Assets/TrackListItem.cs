using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TrackListItem : MonoBehaviour
{
    MenuScript_v2 menu;
    public TrackGroupClass group;

    public Text nameText, authorText, mapsCountText;
    public RawImage coverImage;

    public GameObject isPassedImage;

    public bool isLocalItem;
    public bool isCustomMusic;

    public Color32 defaultColor, downloadedColor, newColor;


    //TimeSpan t1;
    public void Setup(TrackGroupClass group, MenuScript_v2 menu, bool getSpriteFromServer = true, bool isCustomMusic = false)
    {
        this.group = group;
        authorText.text = group.author;
        nameText.text = group.name;
        this.menu = menu;
        this.isCustomMusic = isCustomMusic;
        if (isCustomMusic)
        {
            mapsCountText.text = group.filepath;
        }
        else mapsCountText.text = Assets.SimpleLocalization.LocalizationManager.Localize("MapsCount") + ": " + group.mapsCount;

        //t1 = DateTime.Now.TimeOfDay;
        if (getSpriteFromServer)
        {
            //menu.transform.GetComponent<DownloadHelper>().DownloadSpriteWithCallback(group, SetCover);
            menu.transform.GetComponent<DownloadHelper>().trackListItems.Add(this);
        }

        isLocalItem = !getSpriteFromServer;

        string folderPath = Application.persistentDataPath + "/maps/" + group.author + "-" + group.name;
        GetComponent<Image>().color =
            group.novelty ? newColor 
            : Directory.Exists(folderPath) && Directory.GetDirectories(folderPath).Length > 0 ? downloadedColor 
            : defaultColor;

        isPassedImage.SetActive(menu.accountManager.IsPassed(group.author, group.name));
    }

    public void OnClick()
    {
        menu.OnTrackItemClicked(this);
    }
}
