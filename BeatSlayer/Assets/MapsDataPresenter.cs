using CoversManagement;
using GameNet;
using InGame.Models;
using InGame.UI.Menu.MapsData;
using ProjectManagement;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Group UI Item. Used in main lists like Author, Downloaded and Approved.
/// In maps sections used MapListItem
/// </summary>
/// Doc time: 15.04.2020 (upd: 24.09.2020)
public class MapsDataPresenter : MonoBehaviour
{
    public LikesSlider likesSlider;

    public MapsData groupInfo;


    public Text nameText, authorText;
    public RawImage coverImage, backgroundCoverImage;
    public Text viewsText, downloadsText, gamesText;

    public GameObject isPassedImage;
    public GameObject approvedContainer, newContainer;


    public Color32 defaultColor, downloadedColor, newColor;


    public bool isLocalItem;

    private MenuScript_v2 menu;

    public async void Setup(MapsData groupInfo, MenuScript_v2 menu, bool getSpriteFromServer = true, bool isCustomMusic = false)
    {
        this.groupInfo = groupInfo;
        this.menu = menu;

        authorText.text = groupInfo.Author;
        nameText.text = groupInfo.Name;

        isLocalItem = !getSpriteFromServer;


        CoversManager.AddMapPackage(coverImage, groupInfo.Trackname, callback: (tex) => RefreshBackgroundImage());

        likesSlider.Refresh(groupInfo.Likes, groupInfo.Dislikes);

        downloadsText.text = groupInfo.Downloads.ToString();
        gamesText.text = groupInfo.PlayCount.ToString();
        viewsText.text = groupInfo.Launches.ToString();


        approvedContainer.SetActive(groupInfo.IsApproved && !groupInfo.IsNew);
        newContainer.SetActive(groupInfo.IsNew && !groupInfo.IsApproved);

        if (Payload.Account != null && groupInfo.MapType == GroupType.Author)
        {
            bool isPassed = await NetCore.ServerActions.Account.IsPassed(Payload.Account.Nick, groupInfo.Author, groupInfo.Name);
            if(isPassedImage != null) isPassedImage.SetActive(isPassed);
        }
    }

    public void OnClick()
    {
        menu.OnTrackItemClicked(this);
    }


    private void RefreshBackgroundImage()
    {
        backgroundCoverImage.texture = coverImage.texture;
        var rect = backgroundCoverImage.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
    }
}
