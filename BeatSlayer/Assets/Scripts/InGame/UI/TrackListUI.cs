using CoversManagement;
using DatabaseManagement;
using InGame.Models;
using ProjectManagement;
using Searching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TrackListUI : MonoBehaviour
{
    public MenuScript_v2 menu { get { return GetComponent<MenuScript_v2>(); } }
    public PageController pageController { get { return GetComponent<PageController>(); } }
    public SearchUI SearchUI { get { return GetComponent<SearchUI>(); } }




    public GameObject trackItemPrefab;

    public static Texture2D defaultIcon;
    public Texture2D _defaultIcon;


    public Transform content
    {
        get
        {
            return showedListType == ListType.Downloaded ? content_downloaded :
                showedListType == ListType.AllMusic ? content_allMusic :
                showedListType == ListType.Own ? content_own :
                content_approved;
        }
    }
    public Transform content_downloaded;
    public Transform content_allMusic;
    public Transform content_approved;
    public Transform content_own;


    public Text StateText =>
        showedListType == ListType.Own ? stateOwnText : stateText;
    public Text stateText;
    public Text stateOwnText;


    public ListType showedListType;
    public enum ListType
    {
        Downloaded, AllMusic, Approved, Own
    }



    private void Awake()
    {
        defaultIcon = _defaultIcon;
    }
    public void RefreshApprovedList(int page = -1)
    {
        showedListType = ListType.Approved;
        pageController.SetCurrentPage(showedListType, page == -1 ? pageController.GetCurrentPage(showedListType) : page);
        StartCoroutine(ILoadAndRefresh());
    }
    public void RefreshAllMusicList(int page = -1)
    {
        showedListType = ListType.AllMusic;
        pageController.SetCurrentPage(showedListType, page == -1 ? pageController.GetCurrentPage(showedListType) : page);
        StartCoroutine(ILoadAndRefresh());
    }
    public void RefreshDownloadedList(int page = -1)
    {
        showedListType = ListType.Downloaded;
        pageController.SetCurrentPage(showedListType, page == -1 ? pageController.GetCurrentPage(showedListType) : page);
        StartCoroutine(ILoadAndRefresh());
    }
    public void RefreshOwnList(int page = -1)
    {
        showedListType = ListType.Own;
        pageController.SetCurrentPage(showedListType, page == -1 ? pageController.GetCurrentPage(showedListType) : page);
        StartCoroutine(ILoadAndRefresh());
    }
    public void Refresh()
    {
        StartCoroutine(ILoadAndRefresh());
    }
    // Recheck downloaded maps and refresh list
    public void ReloadDownloadedList()
    {
        showedListType = ListType.Downloaded;
        DatabaseManager.container.DownloadedGroups.Clear();
        Refresh();
    }



    IEnumerator ILoadAndRefresh()
    {
        StateText.text = "";

        // If maps aren't loaded
        if (GetData().Count == 0)
        {
            bool reachable = Application.internetReachability != NetworkReachability.NotReachable;
            if (!reachable && (showedListType != ListType.Downloaded && showedListType != ListType.Own))
            {
                StateText.text = "No internet connection >﹏<";
            }
            else
            {
                bool isApprovedLoaded = false;
                StateText.text = "Loading..";
                try
                {
                    LoadData(() => { isApprovedLoaded = true; });
                }
                catch(Exception err)
                {
                    isApprovedLoaded = false;
                    StateText.text = $"Sorry, I can't load maps ╯︿╰\n<color=#333>{err.Message}</color>";
                }
                
                while (!isApprovedLoaded)
                {
                    yield return new WaitForEndOfFrame();
                }
                if(GetData().Count == 0)
                {
                    StateText.text = "There no music yet";
                }
                else
                {
                    StateText.text = "";
                }
            }
        }

        List<MapsData> sortedList = SearchUI.OnSearch(GetData()).ToList();
        
        RefreshList(sortedList);
    }
    void RefreshList(List<MapsData> ls)
    {
        // Clear cover downloading queue and content
        //CoversManager.ClearPackages(content.GetComponentsInChildren<RawImage>());
        CoversManager.ClearAll();
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }


        List<MapsData> data = new List<MapsData>();
        data.AddRange(ls);



        // Get item paging
        int pagingItemStart = pageController.GetStartItemIndex(pageController.GetCurrentPage(showedListType));
        int pagingItemCount = pageController.GetPageItemsCount(pageController.GetCurrentPage(showedListType), data.Count);


        // Creating TrackGroupClass for items from Database container
        List<MapsData> groups = data.GetRange(pagingItemStart, pagingItemCount);


        // Creating items and cover requests
        //List<CoverRequestPackage> coverPackages = new List<CoverRequestPackage>();

        for (int i = 0; i < groups.Count; i++)
        {
            MapsDataPresenter item = Instantiate(trackItemPrefab, content).GetComponent<MapsDataPresenter>();
            item.Setup(groups[i], menu);

            //coverPackages.Add(new CoverRequestPackage(item.GetComponentInChildren<RawImage>(), groups[i].Trackname));
        }



        float flexCount = groups.Count % 2 == 0 ? groups.Count / 2f : groups.Count / 2f + 1f;
        float contentSize = content.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + content.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);


        // Start downloading covers images for showed groups
        //if(showedListType != ListType.Own) CoversManager.AddPackages(coverPackages);


        pageController.RefreshPageButtons(data.Count);
    }



    void LoadData(Action callback)
    {
        if(showedListType == ListType.Approved)
        {
            DatabaseManager.LoadApproved(callback);
        }
        else if (showedListType == ListType.AllMusic)
        {
            DatabaseManager.LoadAllGroups(callback);
        }
        else if (showedListType == ListType.Downloaded)
        {
            DatabaseManager.LoadDownloadedGroups(callback);
        }
        else
        {
            DatabaseManager.LoadOwnGroups(callback);
        }
    }
    public List<MapsData> GetData()
    {
        List<MapsData> ls = new List<MapsData>();
        if (showedListType == ListType.Approved)
        {
            ls.AddRange(DatabaseManager.container.approvedGroups);
        }
        else if (showedListType == ListType.AllMusic)
        {
            ls.AddRange(DatabaseManager.container.allGroups);
        }
        else if (showedListType == ListType.Downloaded)
        {
            ls.AddRange(DatabaseManager.container.DownloadedGroups);
        }
        else
        {
            ls.AddRange(DatabaseManager.container.OwnGroups);
        }

        return ls;
    }

    public MapsData FindGroupInDownloaded(string trackname)
    {
        return DatabaseManager.container.DownloadedGroups.FirstOrDefault(c => c.Trackname == trackname);
    }
}