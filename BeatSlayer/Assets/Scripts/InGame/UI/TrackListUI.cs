using CoversManagement;
using DatabaseManagement;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TrackListUI : MonoBehaviour
{
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public ListController listController { get { return GetComponent<ListController>(); } }
    public MenuScript_v2 menu { get { return GetComponent<MenuScript_v2>(); } }

    public PageController pageController { get { return GetComponent<PageController>(); } }


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
        StartCoroutine(IRefreshApprovedList());
    }
    public void RefreshAllMusicList(int page = -1)
    {
        showedListType = ListType.AllMusic;
        pageController.SetCurrentPage(showedListType, page == -1 ? pageController.GetCurrentPage(showedListType) : page);
        StartCoroutine(IRefreshApprovedList());
    }




    IEnumerator IRefreshApprovedList()
    {
        // Clear cover downloading queue and content
        CoversManager.ClearPackages(content.GetComponentsInChildren<RawImage>());
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }


        // If approved maps not loaded
        if (GetData().Count == 0)
        {
            bool isApprovedLoaded = false;
            stateText.text = "Loading..";
            LoadData(() => { isApprovedLoaded = true; });
            while (!isApprovedLoaded)
            {
                yield return new WaitForEndOfFrame();
            }
            stateText.text = "";
        }

        // Get item paging
        int pagingItemStart = pageController.GetStartItemIndex(pageController.GetCurrentPage(showedListType));
        int pagingItemCount = pageController.GetPageItemsCount(pageController.GetCurrentPage(showedListType), GetData().Count);


        // Creating TrackGroupClass for items from Database container
        List<TrackGroupClass> groups = new List<TrackGroupClass>();
        foreach (var item in GetData().GetRange(pagingItemStart, pagingItemCount))
        {
            groups.Add(new TrackGroupClass(item));
        }



        // Creating items and cover requests
        listController.displayedApprovedGroup = new List<TrackGroupPair>();
        List<CoverRequestPackage> coverPackages = new List<CoverRequestPackage>();

        for (int i = 0; i < groups.Count; i++)
        {
            TrackGroupClass group = groups[i];

            TrackListItem item = Instantiate(trackItemPrefab, content).GetComponent<TrackListItem>();
            item.Setup(group, menu);

            Debug.Log(group.author + "-" + group.name);

            coverPackages.Add(new CoverRequestPackage(item.GetComponentInChildren<RawImage>(), group.author + "-" + group.name));

            listController.displayedApprovedGroup.Add(new TrackGroupPair(group, item.gameObject));
        }



        float flexCount = groups.Count % 2 == 0 ? groups.Count / 2f : groups.Count / 2f + 1f;
        float contentSize = content.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + content.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);


        CoversManager.AddPackages(coverPackages);

        pageController.RefreshPageButtons(GetData().Count);
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
            //Database.LoadAllGroups(callback);
        }
    }
    List<GroupInfoExtended> GetData()
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

        ls = ls.OrderByDescending(c => c.allLikes).ToList();

        return ls;
    }
}