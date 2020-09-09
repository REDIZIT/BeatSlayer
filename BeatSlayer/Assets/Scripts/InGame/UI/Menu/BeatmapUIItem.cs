using GameNet;
using InGame.Helpers;
using InGame.Models;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

public class BeatmapUIItem : MonoBehaviour
{
    public BeatmapUI ui;
    public FullMapData mapInfo;
    public DifficultyInfo difficulty;
    
    public RawImage coverImage;
    public Text nameText, authorText, nickText, difficultyName;
    public Text downloadsText, playCountText, likesText, dislikesText;

    public Transform starsContent;

    public GameObject downloadIndicator, approvedImage, passedImage;

    public async void Setup(FullMapData info, bool isOnlyOneElement)
    {
        mapInfo = info;
        
        nickText.text = mapInfo.MapperNick;

        if (info.MapType == GroupType.Author)
        {
            likesText.text = mapInfo.Likes.ToString();
            dislikesText.text = mapInfo.Dislikes.ToString();
            downloadsText.text = mapInfo.Downloads.ToString();
            playCountText.text = mapInfo.PlayCount.ToString();
        }
        
        downloadIndicator.SetActive(ProjectManager.IsMapDownloaded(mapInfo.Author, mapInfo.Name, mapInfo.MapperNick));
        approvedImage.SetActive(mapInfo.IsApproved);

        bool isPassed = false;
        if(Payload.Account != null)
        {
            isPassed = await NetCore.ServerActions.Account.IsPassed(Payload.Account.Nick, mapInfo.Author, mapInfo.Name);
        }

        if (passedImage.gameObject == null) return;
        passedImage.SetActive(isPassed);

        GetComponent<Toggle>().isOn = isOnlyOneElement;
        if(isOnlyOneElement)
        {
            //ui.OnBeatmapItemClicked(this);
        }
    }

    public void Setup(DifficultyInfo difficulty, int i)
    {
        this.difficulty = difficulty;

        difficultyName.text = difficulty.name;
        RefreshStars(difficulty.stars);

        playCountText.text = difficulty.playCount.ToString();
        likesText.text = difficulty.likes.ToString();
        dislikesText.text = difficulty.dislikes.ToString();
        //playCountText.text = difficulty.playCount.ToString();

        GetComponent<Toggle>().isOn = i == 0;
    }


    public void Refresh()
    {
        downloadIndicator.SetActive(ProjectManager.IsMapDownloaded(mapInfo.Author, mapInfo.Name, mapInfo.MapperNick));
    }
    void RefreshStars(int stars)
    {
        HelperUI.FillContent(starsContent, 10, (star, i) =>
        {
            Color color = i < stars ? Color.white : Color.grey * 0.4f;
            star.GetComponent<Image>().color = color;
        });

        float x = difficultyName.preferredWidth + 20;
        starsContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0);
    }
}