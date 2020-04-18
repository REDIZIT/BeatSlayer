using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Group UI Item. Used in main lists like Author, Downloaded and Approved.
/// In maps sections used MapListItem
/// </summary>
/// Doc time: 15.04.2020
public class TrackListItem : MonoBehaviour
{
    MenuScript_v2 menu;
    public GroupInfoExtended groupInfo;

    public Text nameText, authorText, mapsCountText;
    public RawImage coverImage;

    public GameObject isPassedImage;

    public bool isLocalItem;
    public bool isCustomMusic;

    public Color32 defaultColor, downloadedColor, newColor;


    //TimeSpan t1;
    public void Setup(GroupInfoExtended groupInfo, MenuScript_v2 menu, bool getSpriteFromServer = true, bool isCustomMusic = false)
    {
        //this.group = group;
        this.groupInfo = groupInfo;
        authorText.text = this.groupInfo.author;
        nameText.text = this.groupInfo.name;
        this.menu = menu;
        this.isCustomMusic = isCustomMusic;
        if (isCustomMusic)
        {
            //mapsCountText.text = groupInfo.filepath;
        }
        else mapsCountText.text = Assets.SimpleLocalization.LocalizationManager.Localize("MapsCount") + ": " + groupInfo.mapsCount;

        //t1 = DateTime.Now.TimeOfDay;
        if (getSpriteFromServer)
        {
            //menu.transform.GetComponent<DownloadHelper>().DownloadSpriteWithCallback(group, SetCover);
            menu.transform.GetComponent<DownloadHelper>().trackListItems.Add(this);
        }

        isLocalItem = !getSpriteFromServer;

        string folderPath = Application.persistentDataPath + "/maps/" + groupInfo.author + "-" + groupInfo.name;
        //GetComponent<Image>().color =
        //    groupInfo.novelty ? newColor 
        //    : Directory.Exists(folderPath) && Directory.GetDirectories(folderPath).Length > 0 ? downloadedColor 
        //    : defaultColor;

        isPassedImage.SetActive(menu.accountManager.IsPassed(groupInfo.author, groupInfo.name));
    }

    public void OnClick()
    {
        menu.OnTrackItemClicked(this);
    }
}
