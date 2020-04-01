using Assets.SimpleLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ListController : MonoBehaviour
{
    public MenuScript_v2 menu { get { return GetComponent<MenuScript_v2>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public DatabaseScript database { get { return GetComponent<DatabaseScript>(); } }
    public DownloadHelper downloadHelper { get { return GetComponent<DownloadHelper>(); } }

    public GameObject authorScrollView, downloadScrollView;
    public Transform authorMusicList, ownMusicList, downloadMusicList;
    public GameObject trackItemPrefab;

    public Text customMusicText;

    public InputField searchField;
    public Dropdown sortByDropdown, newPostionDropdown;
    
    [HideInInspector] public UserTrackClass[] ownMusicArray;

    public void RefreshAuthorList()
    {
        //StartCoroutine(RefreshAuthorListInBackground());
        RefreshAuthorAsync();
    }
    public void RefreshDownloadList()
    {
        //StartCoroutine(RefreshDownloadedListInBackground());
        RefreshDownloadedAsync();
    }

    public delegate void CallbackDelegate(List<string> files);

    public void RefreshCustomList()
    {
        StartCoroutine(WaitingRefreshCustomList());
    }

   
    public void OnRefreshDownloadClick()
    {
        authorScrollView.SetActive(false);
        downloadScrollView.SetActive(true);

        RefreshDownloadList();
    }
    public void OnRefreshServerClick()
    {
        authorScrollView.SetActive(true);
        downloadScrollView.SetActive(false);

        //////RefreshAuthorList();
    }



    #region Refreshing

    //[HideInInspector] public Dictionary<TrackGroupClass, GameObject> displayedAuthorGroup;
    [HideInInspector] public List<TrackGroupPair> displayedAuthorGroup;
    [HideInInspector] public List<TrackGroupPair> displayedCustomGroup;
    [HideInInspector] public List<TrackGroupPair> displayedDownloadedGroup;
    //[HideInInspector] public List<TrackGroupClass> displayedAuthorGroup = new List<TrackGroupClass>();

    public async void RefreshAuthorAsync()
    {
        foreach (Transform child in authorMusicList)
        {
            Destroy(child.gameObject);
        }

        int sortType = prefsManager.prefs.sortTracks;
        List<TrackGroupClass> groups = database.data.tracks;



        downloadHelper.trackListItems.Clear();

        bool doSaveInDisplayedList = displayedAuthorGroup == null;
        if (doSaveInDisplayedList) displayedAuthorGroup = new List<TrackGroupPair>();


        bool isPortrait = Screen.height > Screen.width;
        for (int i = 0; i < groups.Count; i++)
        {
            TrackGroupClass group = groups[i];

            TrackListItem item = Instantiate(trackItemPrefab, authorMusicList).GetComponent<TrackListItem>();
            item.Setup(group, menu);

            downloadHelper.smartLoadingQueue.Add(item);
            //downloadHelper.LoadPreviewSmart(item);

            if (doSaveInDisplayedList) displayedAuthorGroup.Add(new TrackGroupPair(group, item.gameObject));
        }



        float flexCount = groups.Count % 2 == 0 ? groups.Count / 2f : groups.Count / 2f + 1f;
        float contentSize = authorMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + authorMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        authorMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);


        downloadHelper.LoadSmartQueue();


        OnSearch();
    }

    public async void RefreshDownloadedAsync()
    {
        //TimeSpan startTime = DateTime.Now.TimeOfDay;

        foreach (Transform child in downloadMusicList)
        {
            Destroy(child.gameObject);
        }

        int leaderboardScore = 0;


        int sortType = prefsManager.prefs.sortTracks;
        Task<List<TrackGroupClass>> task = database.GetDownloadedMusic();
        await task;
        List <TrackGroupClass> groups = task.Result;


        downloadMusicList.parent.GetChild(0).gameObject.SetActive(groups.Count == 0);

        float contentSize = 0;
        bool isPortrait = Screen.height > Screen.width;
        displayedDownloadedGroup = new List<TrackGroupPair>();

        for (int i = 0; i < groups.Count; i++)
        {
            TrackGroupClass group = groups[i];

            TrackListItem item = Instantiate(trackItemPrefab, downloadMusicList).GetComponent<TrackListItem>();
            item.Setup(group, menu, false);

            string trackFolder = Application.persistentDataPath + "/maps/" + group.author + "-" + group.name;
            string nick = new DirectoryInfo(Directory.GetDirectories(trackFolder)[0]).Name;

            string coverPath = TheGreat.GetCoverPath(trackFolder + "/" + nick, group.author + "-" + group.name);
            if (coverPath != "") item.coverImage.texture = TheGreat.LoadTexure(coverPath);
            else item.coverImage.texture = downloadHelper.defaultIcon;


            displayedDownloadedGroup.Add(new TrackGroupPair(group, item.gameObject));
        }

        float flexCount = groups.Count % 2 == 0 ? groups.Count / 2f : groups.Count / 2f + 1f;
        contentSize = downloadMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + downloadMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);

        downloadMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    }

    List<string> customMusicFiles;
    List<string> customMusicFolders = new List<string>()
    {
        "/storage/emulated/0",
        @"C:\Users\REDIZ\AppData\LocalLow\REDIZIT\Beat Slayer\maps\"
        //"/storage/0",
        //"/storage/1",
        //"/sdcard"
    };
    public void RefreshCustomListInBackground()
    {
        //TimeSpan startTime = DateTime.Now.TimeOfDay;

        string[] drives = Environment.GetLogicalDrives();
        customMusicFolders.AddRange(drives);

        //Debug.Log("[RefreshCustomListInBackground]");

        string[] filters = new string[2] { "*.mp3", "*.ogg" };

        TimeSpan t1 = DateTime.Now.TimeOfDay;
        List<string> files = new List<string>();
        for (int i = 0; i < customMusicFolders.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (!Directory.Exists(customMusicFolders[i])) continue;
                try
                {
                    files.AddRange(Directory.GetFiles(customMusicFolders[i], filters[j], SearchOption.AllDirectories));
                }
                catch (Exception err)
                {
                    Debug.LogWarning("[GETFILES] " + err.Message);
                }
            }
            
        }

        string msg = "";
        foreach (string filepath in files)
        {
            msg += filepath + " : " + Path.GetFullPath(filepath);
        }

        Debug.Log("[LOAD] File time is " + (DateTime.Now.TimeOfDay - t1).TotalMilliseconds);
        Debug.Log("[LOAD] Files count " + files.Count);
        

        customMusicFiles = new List<string>();
        customMusicFiles = files;
    }
    IEnumerator WaitingRefreshCustomList()
    {
        customMusicFiles = null;
        customMusicText.gameObject.SetActive(true);
        customMusicText.text = "Loading music..";

        Thread th = new Thread(RefreshCustomListInBackground);
        th.Start();

        while (customMusicFiles == null)
        {
            yield return new WaitForEndOfFrame();
        }

        OnCustomMusicFound(customMusicFiles);
    }
    public void OnCustomMusicFound(List<string> files)
    {
        if (files.Count == 0)
        {
            customMusicText.text = @"Music not found (>_<)" + "\nThese folders have been checked:";
            foreach (var path in customMusicFolders)
            {
                customMusicText.text += "\n" + path;
            }
        }
        else
        {
            customMusicText.text = "";
            customMusicText.gameObject.SetActive(false);
        }

        foreach (Transform child in ownMusicList)
        {
            Destroy(child.gameObject);
        }

        float contentSize = 0;
        bool isPortrait = Screen.height > Screen.width;
        displayedCustomGroup = new List<TrackGroupPair>();

        foreach (var file in files)
        {
            string[] split = Path.GetFileNameWithoutExtension(file).Split('-');

            string author = "Unknown";
            string name = Path.GetFileNameWithoutExtension(file);

            if (split.Length > 1)
            {
                author = split[0];
                name = split[1];
            }

            TrackGroupClass group = new TrackGroupClass()
            {
                author = author,
                name = name,
                mapsCount = 1,
                filepath = file
            };

            TrackListItem item = Instantiate(trackItemPrefab, ownMusicList).GetComponent<TrackListItem>();
            item.Setup(group, menu, false, true);

            //item.coverImage.sprite = database.defaultTrackSprite; ///////////// !!!!!!!!!!!!!!!!!!
            item.coverImage.texture = downloadHelper.defaultIcon;

            displayedCustomGroup.Add(new TrackGroupPair(group, item.gameObject));
        }


        float flexCount = files.Count % 2 == 0 ? files.Count / 2f : files.Count / 2f + 1f;
        contentSize = ownMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + ownMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);

        ownMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    }


    public void UpdateIsPassedStates()
    {
        AccountManager acc = GetComponent<AuthManager>().manager;
        if(displayedAuthorGroup != null)
        {
            foreach (var item in displayedAuthorGroup)
            {
                item.go.GetComponent<TrackListItem>().isPassedImage.SetActive(acc.IsPassed(item.group.author, item.group.name));
            }
        }
        
        foreach (var item in displayedDownloadedGroup)
        {
            item.go.GetComponent<TrackListItem>().isPassedImage.SetActive(acc.IsPassed(item.group.author, item.group.name));
        }
    }

    #endregion


    public UserTrackClass[] GetOwnMusic()
    {
        string dir = "";
        if (Application.isEditor) dir = Application.persistentDataPath;
        else /*dir = "/storage/emulated/0/";*/ dir = "/sdcard/";

        UserTrackList ls = new UserTrackList() { list = new List<UserTrackClass>() };
        bool binExists = false;
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/ownmusic.bin"))
        {
            using (var fileStream = File.Open(Application.persistentDataPath + "/ownmusic.bin", FileMode.Open))
            {
                ls = (UserTrackList)binaryFormatter.Deserialize(fileStream);
                binExists = true;
            }
        }




        string[] ext = { ".mp3", ".ogg" };
        string[] myFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
            .Where(s => ext.Contains(Path.GetExtension(s))/* && Path.GetDirectoryName(s).Contains(Application.persistentDataPath)*/).ToArray();
        List<UserTrackClass> clsToRemove = new List<UserTrackClass>();
        clsToRemove.AddRange(ls.list.Where(c => !File.Exists(c.path)));
        //foreach (UserTrackClass cl in ls.list)
        //{
        //    if (!File.Exists(cl.path)) clsToRemove.Add(cl);
        //}
        foreach (UserTrackClass cl in clsToRemove)
        {
            ls.list.Remove(cl);
        }
        foreach (string file in myFiles)
        {
            string folderPath = Path.GetDirectoryName(file);
            if (!folderPath.Contains("."))
            {
                UserTrackClass cl = null;
                if (binExists)
                {
                    cl = ls.list.Find(c => c.path == file);
                }

                if (cl == null)
                {
                    ls.list.Add(new UserTrackClass(file, Path.GetFileNameWithoutExtension(file)) { isNew = true });
                }
                else
                {
                    cl.isNew = false;
                }
            }
        }


        using (var fileStream = File.Create(Application.persistentDataPath + "/ownmusic.bin"))
        {
            binaryFormatter.Serialize(fileStream, ls);
        }
        return ls.list.ToArray();
    }


    public void OnSearch()
    {
        string search = searchField.text;

        var searched = displayedAuthorGroup.
            Where(c => (c.group.author + "-" + c.group.name).ToLower().Contains(search.ToLower())).
            OrderBy(c => (c.group.author + "-" + c.group.name).Contains(search));


        // Likes, plays, date
        if (sortByDropdown.value == 0) searched = searched.OrderByDescending(c => c.group.likes);
        else if (sortByDropdown.value == 1) searched = searched.OrderByDescending(c => c.group.plays);

        if(newPostionDropdown.value == 0) searched = searched.OrderByDescending(c => c.group.novelty);
        if(newPostionDropdown.value == 2) searched = searched.OrderBy(c => c.group.novelty);
        //if(newPostionDropdown.value == 1) searched = searched.OrderBy(c => c.group.novelty);



        int siblingIndex = 0;
        foreach (var item in displayedAuthorGroup)
        {
            item.go.SetActive(false);
        }
        foreach (var item in searched)
        {
            if (displayedAuthorGroup.Contains(item))
            {
                item.go.SetActive(true);
                item.go.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }

        float flexCount = siblingIndex % 2 == 0 ? siblingIndex / 2f : siblingIndex / 2f + 1f;
        float contentSize = authorMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + authorMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        authorMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    }

    public void OnSearchCustom(InputField field)
    {
        string search = field.text;

        var searched = displayedCustomGroup.
            Where(c => (c.group.author + "-" + c.group.name).ToLower().Contains(search.ToLower())).
            OrderBy(c => (c.group.author + "-" + c.group.name).Contains(search));


        // Likes, plays, date
        if (sortByDropdown.value == 0) searched = searched.OrderByDescending(c => c.group.likes);
        else if (sortByDropdown.value == 1) searched = searched.OrderByDescending(c => c.group.plays);

        if (newPostionDropdown.value == 0) searched = searched.OrderByDescending(c => c.group.novelty);
        if (newPostionDropdown.value == 2) searched = searched.OrderBy(c => c.group.novelty);
        //if(newPostionDropdown.value == 1) searched = searched.OrderBy(c => c.group.novelty);



        int siblingIndex = 0;
        foreach (var item in displayedCustomGroup)
        {
            item.go.SetActive(false);
        }
        foreach (var item in searched)
        {
            if (displayedCustomGroup.Contains(item))
            {
                item.go.SetActive(true);
                item.go.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }

        float flexCount = siblingIndex % 2 == 0 ? siblingIndex / 2f : siblingIndex / 2f + 1f;
        float contentSize = ownMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + ownMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);
        ownMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    }
}

public class TrackGroupPair
{
    public TrackGroupClass group;
    public GameObject go;

    public TrackGroupPair(TrackGroupClass group, GameObject go)
    {
        this.group = group;
        this.go = go;
    }
}