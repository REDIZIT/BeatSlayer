using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    public TrackListUI ui { get { return GetComponent<TrackListUI>(); } }

    public int itemsPerPage = 10;

    public Transform pageButtonsContent;


    public GameObject approvedScrollView, authorScrollView, downloadedScrollView;


    public int GetCurrentPage(TrackListUI.ListType type)
    {
        if (type == TrackListUI.ListType.AllMusic) return currentpage_allMusic;
        if (type == TrackListUI.ListType.Approved) return currentpage_approved;
        else return currentpage_downloaded;
    }
    public void SetCurrentPage(TrackListUI.ListType type, int page)
    {
        if (type == TrackListUI.ListType.AllMusic) currentpage_allMusic = page;
        if (type == TrackListUI.ListType.Approved) currentpage_approved = page;
        else currentpage_downloaded = page;
    }
    int currentpage_downloaded;
    int currentpage_approved;
    int currentpage_allMusic;



    Action<int> refreshListCallback;


    public void RefreshPageButtons(int itemsCount)
    {
        int pagesCount = Mathf.FloorToInt(itemsCount / (float)itemsPerPage);

        int currentPage = GetCurrentPage(ui.showedListType);
        int nextPage = currentPage + 1 <= pagesCount ? currentPage + 1 : -1;
        int prevPage = currentPage - 1 >= 0 ? currentPage - 1 : -1;
        int startPage = currentPage - 2 >= 0 ? 0 : -1;
        int endPage = currentPage <= pagesCount - 2 ? pagesCount : -1;

        RefreshButton(2, currentPage, true);
        RefreshButton(3, nextPage, false);
        RefreshButton(1, prevPage, false);
        RefreshButton(0, startPage, false);
        RefreshButton(4, endPage, false);
    }
    void RefreshButton(int index, int page, bool isCurrent)
    {
        pageButtonsContent.GetChild(index).gameObject.SetActive(true);
        if (page == -1)
        {
            pageButtonsContent.GetChild(index).gameObject.SetActive(false);
            return;
        }

        pageButtonsContent.GetChild(index).GetComponentInChildren<Text>().text = (page + 1).ToString();

        pageButtonsContent.GetChild(index).GetComponentInChildren<Text>().color = isCurrent ? Color.black : Color.white;
        pageButtonsContent.GetChild(index).GetComponentInChildren<Image>().color = isCurrent ? new Color32(0, 145, 255, 255) : new Color32(0, 0, 0, 0);
    }
    public void OnPageBtnClicked(Transform btn)
    {
        int page = int.Parse(btn.GetComponentInChildren<Text>().text) - 1;
        refreshListCallback(page);
    }


    public int GetStartItemIndex(int currentPage)
    {
        return currentPage * itemsPerPage;
    }
    public int GetPageItemsCount(int currentPage, int allItemsCount)
    {
        int minRange = GetStartItemIndex(currentPage);
        int maxRange = allItemsCount > minRange + itemsPerPage ? minRange + itemsPerPage : allItemsCount;
        return maxRange - minRange;
    }




    
    public void ShowScrollViewApproved()
    {
        refreshListCallback = ui.RefreshApprovedList;
        refreshListCallback(-1);

        approvedScrollView.SetActive(true);
        authorScrollView.SetActive(false);
        downloadedScrollView.SetActive(false);
    }
    public void ShowScrollViewAuthor()
    {
        refreshListCallback = ui.RefreshAllMusicList;
        refreshListCallback(-1);

        approvedScrollView.SetActive(false);
        authorScrollView.SetActive(true);
        downloadedScrollView.SetActive(false);
    }
    public void ShowScrollViewDownloaded()
    {
        refreshListCallback = ui.RefreshDownloadedList;
        refreshListCallback(-1);

        approvedScrollView.SetActive(false);
        authorScrollView.SetActive(false);
        downloadedScrollView.SetActive(true);
    }
}