using GameNet;
using InGame.Menu.Maps;
using InGame.SceneManagement;
using Michsky.UI.ModernUIPack;
using ProjectManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public BeatmapUI ui;
    public GameObject overlay;
    public DatabaseScript database;

    [SerializeField] private MapsDownloadQueuer queuer;

    [SerializeField] private SwitchManager easyMapsToggle, hardMapsToggle;
    [SerializeField] private GameObject body, progressBody;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private GameObject completeCheckmark;
    [SerializeField] private Button startTutorialMapBtn;

    private bool isWaitingForStart;
    private KeyValuePair<string, string> tutorialKeyPair;
    private MapDownloadTask tutorialMapTask;




    private void Update()
    {
        if (!isWaitingForStart) return;

        if (tutorialMapTask == null)
        {
            tutorialMapTask = MapsDownloadQueuerBackground.queue.FirstOrDefault(c => c.Trackname == tutorialKeyPair.Key && c.Mapper == tutorialKeyPair.Value);
        }

        if (tutorialMapTask == null) return;

        body.SetActive(false);
        progressBody.SetActive(true);

        progressBar.gameObject.SetActive(tutorialMapTask.TaskState == MapDownloadTask.State.Downloading);
        progressBar.CurrentPercent = tutorialMapTask.ProgressPercentage;

        bool isDone = tutorialMapTask.TaskState == MapDownloadTask.State.Completed;
        
        if(isDone)
        {
            completeCheckmark.SetActive(true);
            startTutorialMapBtn.interactable = true;
            isWaitingForStart = false;
        }
    }




    public void ShowOverlay()
    {
        overlay.SetActive(true);
    }


    public async void OnContinueBtnClick()
    {
        Dictionary<string, string> maps = new Dictionary<string, string>();


        // Adding default tutorial map to queue
        var tutorialMap = await NetCore.ServerActions.Tutorial.GetTutorialMap();
        maps[tutorialMap.Key] = tutorialMap.Value;
        tutorialKeyPair = tutorialMap;



        // Adding easy maps
        if (easyMapsToggle.isOn)
        {
            foreach (var map in await NetCore.ServerActions.Tutorial.GetEasyMaps())
            {
                maps[map.Key] = map.Value;
            }
        }
        // Adding hard
        if (hardMapsToggle.isOn)
        {
            foreach (var map in await NetCore.ServerActions.Tutorial.GetHardMaps())
            {
                maps[map.Key] = map.Value;
            }
        }


        foreach (var map in maps)
        {
            queuer.AddTask(map.Key, map.Value);
        }



        isWaitingForStart = true;
    }
    public void OnNoBtnClick()
    {
        overlay.SetActive(false);
    }


    public void OnStartMapBtnClick()
    {
        var mapInfo = database.GetMapInfo(tutorialMapTask.Trackname, tutorialMapTask.Mapper);

        SceneloadParameters param = SceneloadParameters.AuthorMusicPreset(mapInfo, mapInfo.difficulties[0], new List<InGame.Menu.Mods.ModSO>());
        SceneController.instance.LoadScene(param);
    }
}
