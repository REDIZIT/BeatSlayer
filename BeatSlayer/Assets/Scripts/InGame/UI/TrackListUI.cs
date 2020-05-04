using CoversManagement;
using DatabaseManagement;
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
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    //public ListController listController { get { return GetComponent<ListController>(); } }
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
                content_approved;
        }
    }
    public Transform content_downloaded;
    public Transform content_allMusic;
    public Transform content_approved;


    public Text stateText;


    public ListType showedListType;
    public enum ListType
    {
        Downloaded, AllMusic, Approved
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
    public void Refresh()
    {
        StartCoroutine(ILoadAndRefresh());
    }
    // Recheck downloaded maps and refresh list
    public void ReloadDownloadedList()
    {
        showedListType = ListType.Downloaded;
        Database.container.downloadedGroups.Clear();
        Refresh();
    }



    IEnumerator ILoadAndRefresh()
    {
        stateText.text = "";

        // If maps aren't loaded
        if (GetData().Count == 0)
        {
            bool reachable = Application.internetReachability != NetworkReachability.NotReachable;
            if (!reachable && showedListType != ListType.Downloaded)
            {
                stateText.text = "No internet connection >﹏<";
            }
            else if (reachable || showedListType == ListType.Downloaded)
            {
                bool isApprovedLoaded = false;
                stateText.text = "Loading..";
                try
                {
                    LoadData(() => { isApprovedLoaded = true; });
                }
                catch(Exception err)
                {
                    isApprovedLoaded = false;
                    stateText.text = $"Sorry, I can't load maps ╯︿╰\n<color=#333>{err.Message}</color>";
                }
                
                while (!isApprovedLoaded)
                {
                    yield return new WaitForEndOfFrame();
                }
                if(GetData().Count == 0)
                {
                    stateText.text = "There no music yet";
                }
                else
                {
                    stateText.text = "";
                }
            }
        }

        List<GroupInfoExtended> sortedList = SearchUI.OnSearch(GetData()).ToList();

        DateTime d1 = DateTime.Now;

        RefreshList(sortedList);

        Debug.Log("Refresh time is " + (DateTime.Now - d1).TotalMilliseconds);
    }
    void RefreshList(List<GroupInfoExtended> ls)
    {
        // Clear cover downloading queue and content
        //CoversManager.ClearPackages(content.GetComponentsInChildren<RawImage>());
        CoversManager.ClearAll();
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }


        List<GroupInfoExtended> data = new List<GroupInfoExtended>();
        data.AddRange(ls);



        // Get item paging
        int pagingItemStart = pageController.GetStartItemIndex(pageController.GetCurrentPage(showedListType));
        int pagingItemCount = pageController.GetPageItemsCount(pageController.GetCurrentPage(showedListType), data.Count);


        // Creating TrackGroupClass for items from Database container
        List<GroupInfoExtended> groups = data.GetRange(pagingItemStart, pagingItemCount);


        // Creating items and cover requests
        //listController.displayedApprovedGroup = new List<TrackGroupPair>();
        List<CoverRequestPackage> coverPackages = new List<CoverRequestPackage>();

        for (int i = 0; i < groups.Count; i++)
        {
            TrackListItem item = Instantiate(trackItemPrefab, content).GetComponent<TrackListItem>();
            item.Setup(groups[i], menu);

            coverPackages.Add(new CoverRequestPackage(item.GetComponentInChildren<RawImage>(), groups[i].author + "-" + groups[i].name));

            //listController.displayedApprovedGroup.Add(new TrackGroupPair(group, item.gameObject));
        }



        float flexCount = groups.Count % 2 == 0 ? groups.Count / 2f : groups.Count / 2f + 1f;
        float contentSize = content.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + content.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);


        CoversManager.AddPackages(coverPackages);

        pageController.RefreshPageButtons(data.Count);
    }



    void LoadData(Action callback)
    {
        if(showedListType == ListType.Approved)
        {
            Database.LoadApproved(callback);
        }
        else if (showedListType == ListType.AllMusic)
        {
            Database.LoadAllGroups(callback);
        }
        else
        {
            Database.LoadDownloadedGroups(callback);
        }
    }
    public List<GroupInfoExtended> GetData()
    {
        List<GroupInfoExtended> ls = new List<GroupInfoExtended>();
        if (showedListType == ListType.Approved)
        {
            ls.AddRange(Database.container.approvedGroups);
        }
        else if (showedListType == ListType.AllMusic)
        {
            ls.AddRange(Database.container.allGroups);
        }
        else
        {
            ls.AddRange(Database.container.downloadedGroups);
        }

        return ls;
    }
}