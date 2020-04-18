using LeaderboardManagement;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject leaderboardLayout;
    public Transform leaderboardContent;
    public Text stateText;
    
    public void Show()
    {
        leaderboardLayout.SetActive(true);
        stateText.text = "Loading..";
        try
        {
            LeaderboardManager.GetLeaderboard(OnLeaderboardLoaded);
        }
        catch (System.Exception err)
        {
            stateText.text = "Sorry, something went wrong";
            Debug.LogError("GetLeaderboard error\n" + err);
        }
    }
    void OnLeaderboardLoaded(List<LeaderboardItem> items)
    {
        GameObject prefab = ClearLeaderboard();
        stateText.text = "";

        foreach (LeaderboardItem item in items)
        {
            CreateItem(item, prefab);
        }

        prefab.SetActive(false);

        float height = -6 + (64 + 6) * items.Count;
        leaderboardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(leaderboardContent.GetComponent<RectTransform>().sizeDelta.x, height);
    }

    GameObject ClearLeaderboard()
    {
        foreach (Transform child in leaderboardContent) if (child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = leaderboardContent.GetChild(0).gameObject;
        prefab.SetActive(true);
        return prefab;
    }
    void CreateItem(LeaderboardItem item, GameObject prefab)
    {
        GameObject go = Instantiate(prefab, leaderboardContent);
        LeaderboardUIItem ui = go.GetComponent<LeaderboardUIItem>();
        ui.leaderboardItem = item;
        ui.RefreshLeaderboardItem();
    }
}