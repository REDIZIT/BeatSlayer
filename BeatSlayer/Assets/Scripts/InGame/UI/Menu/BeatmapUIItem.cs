﻿using System.Collections;
using System.Collections.Generic;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

public class BeatmapUIItem : MonoBehaviour
{
    public BeatmapUI ui;
    public MapInfo mapInfo;
    public DifficultyInfo difficulty;
    
    public RawImage coverImage;
    public Text nameText, authorText, nickText, difficultyName;
    public Text downloadsText, playCountText, likesText, dislikesText;

    public Transform starsContent;

    public GameObject downloadIndicator, approvedImage, passedImage;

    public void Setup(MapInfo info)
    {
        mapInfo = info;
        
        nickText.text = mapInfo.nick;

        if (info.group.groupType == GroupInfo.GroupType.Author)
        {
            likesText.text = mapInfo.Likes.ToString();
            dislikesText.text = mapInfo.Dislikes.ToString();
            downloadsText.text = mapInfo.Downloads.ToString();
            playCountText.text = mapInfo.PlayCount.ToString();
        }
        
        downloadIndicator.SetActive(ProjectManager.IsMapDownloaded(mapInfo.author, mapInfo.name, mapInfo.nick));
        approvedImage.SetActive(mapInfo.approved);
        passedImage.SetActive(AccountManager.IsPassed(mapInfo.author, mapInfo.name));
    }

    public void Setup(DifficultyInfo difficulty)
    {
        this.difficulty = difficulty;

        difficultyName.text = difficulty.name;
        RefreshStars(difficulty.stars);

        playCountText.text = difficulty.playCount.ToString();
        likesText.text = difficulty.likes.ToString();
        dislikesText.text = difficulty.dislikes.ToString();
        //playCountText.text = difficulty.playCount.ToString();
    }


    public void Refresh()
    {
        downloadIndicator.SetActive(ProjectManager.IsMapDownloaded(mapInfo.author, mapInfo.name, mapInfo.nick));
    }
    void RefreshStars(int stars)
    {
        ui.FillContent(starsContent, 10, (star, i) =>
        {
            Color color = i < stars ? Color.white : Color.grey * 0.4f;
            star.GetComponent<Image>().color = color;
        });

        float x = difficultyName.preferredWidth + 20;
        starsContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0);
    }
}