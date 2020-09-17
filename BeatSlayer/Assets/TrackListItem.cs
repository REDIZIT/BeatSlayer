using CoversManagement;
using GameNet;
using InGame.Models;
using ProjectManagement;
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
    public MapsData groupInfo;

    public Text nameText, authorText, mapsCountText;
    public RawImage coverImage;

    public GameObject isPassedImage;

    public bool isLocalItem;
    public bool isCustomMusic;

    public Color32 defaultColor, downloadedColor, newColor;

    
    public async void Setup(MapsData groupInfo, MenuScript_v2 menu, bool getSpriteFromServer = true, bool isCustomMusic = false)
    {
        this.groupInfo = groupInfo;
        this.menu = menu;
        this.isCustomMusic = isCustomMusic;


        authorText.text = groupInfo.Author;
        nameText.text = groupInfo.Name;
        if (!isCustomMusic)
        {
            mapsCountText.text = Assets.SimpleLocalization.LocalizationManager.Localize("MapsCount") + ": " + groupInfo.MappersNicks.Count;
        }
        CoversManager.AddMapPackage(coverImage, groupInfo.Trackname);

        
        isLocalItem = !getSpriteFromServer;

        string folderPath = Application.persistentDataPath + "/maps/" + groupInfo.Trackname;
        GetComponent<Image>().color =
            groupInfo.IsNew ? newColor 
            : Directory.Exists(folderPath) && Directory.GetDirectories(folderPath).Length > 0 ? downloadedColor 
            : defaultColor;


        if(Payload.Account != null && groupInfo.MapType == GroupType.Author)
        {
            bool isPassed = await NetCore.ServerActions.Account.IsPassed(Payload.Account.Nick, groupInfo.Author, groupInfo.Name);
            if (isPassedImage != null) isPassedImage.SetActive(isPassed);
        }
    }

    public void OnClick()
    {
        menu.OnTrackItemClicked(this);
    }
}
