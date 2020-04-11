using MusicFilesManagement;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class MusicFilesUI : MonoBehaviour
{
    public MenuScript_v2 menu { get { return GetComponent<MenuScript_v2>(); } }
    public DownloadHelper downloadHelper { get { return GetComponent<DownloadHelper>(); } }

    public Transform foldersContent;

    public Transform ownMusicList;
    public GameObject trackItemPrefab;
    public Text stateText;


    public void Refresh()
    {
        stateText.gameObject.SetActive(true);
        stateText.text = "Loading..";
        MusicFilesManager.LoadData(ShowMusicFilesList);
    }

    /// <summary>
    /// Search files and refresh list
    /// </summary>
    public void FullRefresh()
    {
        stateText.gameObject.SetActive(true);
        stateText.text = "Loading..";
        MusicFilesManager.Search(ShowMusicFilesList);
    }

    public void ShowMusicFilesList()
    {
        stateText.text = "Showing..";
        foreach (Transform child in ownMusicList) Destroy(child.gameObject);


        for (int i = 0; i < MusicFilesManager.data.files.Count; i++)
        {
            string filepath = MusicFilesManager.data.files[i];

            if (!File.Exists(filepath)) continue;

            string trackname = Path.GetFileName(filepath);
            string author = trackname.Contains("-") ? trackname.Split('-')[0] : "";
            string name = trackname.Contains("-") ? trackname.Split('-')[1] : trackname.Split('-')[0];

            TrackGroupClass group = new TrackGroupClass()
            {
                author = author,
                name = name,
                mapsCount = 1,
                filepath = filepath
            };

            TrackListItem item = Instantiate(trackItemPrefab, ownMusicList).GetComponent<TrackListItem>();
            item.Setup(group, menu, false, true);

            item.coverImage.texture = downloadHelper.defaultIcon;
        }

        stateText.gameObject.SetActive(false);

        float filesCount = MusicFilesManager.data.files.Count;
        float flexCount = filesCount % 2 == 0 ? filesCount / 2f : filesCount / 2f + 1f;
        float contentSize = ownMusicList.GetComponent<GridLayoutGroup>().cellSize.y * flexCount + ownMusicList.GetComponent<GridLayoutGroup>().spacing.y * (flexCount - 2);

        ownMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    }

    public void ShowFoldersOverlay()
    {
        foreach (Transform child in foldersContent) if(child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = foldersContent.GetChild(0).gameObject;
        prefab.SetActive(true);

        float height = -2;
        for (int i = 0; i < MusicFilesManager.data.folders.Count; i++)
        {
            string folderPath = MusicFilesManager.data.folders[i];
            Transform item = Instantiate(prefab, foldersContent).transform;

            item.name = i.ToString();
            item.GetChild(0).GetComponent<Text>().text = folderPath;

            height += 62 + 2;
        }

        prefab.SetActive(false);
        foldersContent.GetComponent<RectTransform>().sizeDelta = new Vector2(foldersContent.GetComponent<RectTransform>().sizeDelta.x, height);
    }

    public void OnDeleteFolderBtnClick(Transform item)
    {
        int i = int.Parse(item.name);
        MusicFilesManager.data.folders.RemoveAt(i);
        MusicFilesManager.SaveData();

        ShowFoldersOverlay();
    }
    public void OnAddBtnClick(InputField field)
    {
        string folderPath = field.text;
        field.GetComponent<Image>().color = new Color32(24,24,24, 255);


        if (!Directory.Exists(folderPath))
        {
            field.GetComponent<Image>().color = Color.red * 0.8f;
            return;
        }

        if (!MusicFilesManager.data.folders.Contains(folderPath))
        {
            MusicFilesManager.data.folders.Add(folderPath);
        }

        MusicFilesManager.SaveData();

        field.text = "";
        ShowFoldersOverlay();
    }
}