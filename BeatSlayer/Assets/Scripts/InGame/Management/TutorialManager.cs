using Newtonsoft.Json;
using ProjectManagement;
using System.Collections;
using UnityEngine;
using Web;

public class TutorialManager : MonoBehaviour
{
    public BeatmapUI ui;
    public GameObject overlay;


    public void ShowOverlay()
    {
        overlay.SetActive(true);
    }
    public void ShowTutorialMap()
    {
        string json = WebAPI.GetTutorialGroup();
        GroupInfoExtended group = JsonConvert.DeserializeObject<GroupInfoExtended>(json);

        group.groupType = GroupInfo.GroupType.Author;

        ui.Open(group);
    }

    public void OnYesBtnClick()
    {
        overlay.SetActive(false);
        ShowTutorialMap();
    }

    public void OnNoBtnClick()
    {
        overlay.SetActive(false);
    }
}
