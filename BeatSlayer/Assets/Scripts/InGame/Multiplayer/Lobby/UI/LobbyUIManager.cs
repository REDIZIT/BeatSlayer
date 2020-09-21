using Assets.SimpleLocalization;
using CoversManagement;
using GameNet;
using InGame.Animations;
using InGame.Game.Mods;
using InGame.Game.Scoring.Mods;
using InGame.Helpers;
using InGame.Menu.Maps;
using InGame.Menu.Mods;
using InGame.Models;
using InGame.Multiplayer.Lobby.Chat;
using InGame.ScriptableObjects;
using Michsky.UI.ModernUIPack;
using Pixelplacement;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager instance;

        [Header("Components")]
        public LobbyChatUIManager chatUI;
        public BeatmapUI beatmapUI;
        public TrackListUI listUI;
        public ModsUI modsUI;
        public SODB sodb;
        public MapsDownloadQueuer downloader;
        public StartLobbyGameButtonPresenter startBtn;

        [Header("States")]
        public State mainStateMachine;
        public State lobbyStateMachine;
        public PlayBtnAnim pager;
        public GameObject tracksOverlay, mainUI;

        [Header("Lobby page")]
        public Text lobbyNameText;
        public InputField nameInputField, passwordInputField;
        public GameObject nonHostNameContainer, hostNameContainer;
        public GameObject lockImage;

        public GameObject readyBtn, notReadyBtn, locationBtn;
        public GameObject changeMapButton;


        [Header("Timeline")]
        public GameObject timeline;
        public Slider timelineSlider;
        public Text timelineText;

        [Header("Mods")]
        public Transform modsContainer;
        public GameObject modItemPrefab;

        [Header("Map info")]
        public RawImage mapImage;
        public Texture2D defaultMapImage;
        public Text authorText;
        public Text nameText, difficultyText, mapperText;
        public ProgressBar progressCircle;
        public GameObject progressLocker;
        public GameObject hostChangingMessage, mapInfoTextsContainer, noMapSetText;
        public GameObject hostChangingIcon;

        [Header("Players list")]
        public List<LobbySlotPresenter> slots;

        public Transform slotStackParent;
        public GameObject slotPrefab;

        [Header("Lobbies list")]
        public Transform lobbyStackParent;
        public GameObject lobbyItemPrefab;
        public Button refreshLobbiesBtn, createLobbyBtn;
        public Text refreshLabel;



        private void Awake()
        {
            if (instance == null) instance = this;
        }
        private void Start()
        {
            slots = new List<LobbySlotPresenter>();
            for (int i = 0; i < LobbyManager.MAX_LOBBY_PLAYERS; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotStackParent);
                slots.Add(slotObj.GetComponent<LobbySlotPresenter>());
            }

            // Open lobby page if game finished
            if (LobbyManager.lobby != null)
            {
                LobbyManager.ChangeReadyState(LobbyPlayer.ReadyState.NotReady);

                pager.OpenLobbyPage();
                RefreshLobby(); // Refresh as not actual lobby

                Task.Run(async () =>
                {
                    // Get actual lobby
                    Lobby lobby = await NetCore.ServerActions.Lobby.GetLobby(LobbyManager.lobby.Id);
                    LobbyManager.lobby.UpdateValues(lobby);
                    // Refresh as actual lobby
                    UnityMainThreadDispatcher.Instance().Enqueue(RefreshLobby);
                });
            }

            if (Payload.Account == null)
            {
                refreshLabel.text = LocalizationManager.Localize("NotLoggedIn");
                createLobbyBtn.interactable = false;
                refreshLobbiesBtn.interactable = false;
            }



            NetCore.Configure(() =>
            {
                NetCore.Subs.OnLobbyPlayerJoin += OnPlayerJoin;
                NetCore.Subs.OnLobbyPlayerLeave += OnPlayerLeave;
                NetCore.Subs.OnLobbyHostChange += OnHostChange;
                NetCore.Subs.OnLobbyPlayerKick += OnLobbyPlayerKick;
                NetCore.Subs.OnHostStartChangingMap += OnHostStartChangingMap;
                NetCore.Subs.OnHostCancelChangingMap += OnRemoteHostCancelChanging;
                NetCore.Subs.OnLobbyMapChange += OnMapRemoteChange;
                NetCore.Subs.OnLobbyRename += OnLobbyRename;
                NetCore.Subs.OnLobbyChangePassword += OnLobbyChangePassword;
                NetCore.Subs.OnLobbyPlayStatusChanged += OnLobbyPlayStatusChanged;

                NetCore.Subs.OnRemotePlayerReadyStateChange += OnRemotePlayerReadyStateChange;
                NetCore.Subs.OnRemotePlayerModsChange += OnRemotePlayerModsChange;
                NetCore.Subs.OnRemotePlayerStartDownloading += OnRemotePlayerStartDownloading;
                NetCore.Subs.OnRemotePlayerDownloadProgress += OnRemotePlayerDownloadProgress;
                NetCore.Subs.OnRemotePlayerDownloaded += OnRemotePlayerDownloaded;

                NetCore.Subs.OnMultiplayerGameStart += LobbyManager.StartMap;
            });
        }
        //private void Update()
        //{
        //    if (LobbyManager.lobby == null) return;

        //    if (LobbyManager.lobby.IsPlaying)
        //    {
        //        // Increase timeline seconds
        //        LobbyManager.lobby.CurrentSecond += Time.deltaTime;
        //        RefreshTimeline();
        //    }
        //}



        public void RefreshLobby()
        {
            if (LobbyManager.lobby == null) return;

            nonHostNameContainer.SetActive(!LobbyManager.lobbyPlayer.IsHost);
            hostNameContainer.SetActive(LobbyManager.lobbyPlayer.IsHost);

            lobbyNameText.text = LobbyManager.lobby.Name;
            nameInputField.text = LobbyManager.lobby.Name;
            passwordInputField.text = LobbyManager.lobby.Password;
            lockImage.SetActive(LobbyManager.lobby.HasPassword);



            RefreshPlayerSlots();
            RefreshMapInfo();
            RefreshReadyButtons();

            RemoveBlockedMods();
            RefreshMods();
            //RefreshTimeline();

            pager.OpenLobbyPage();
        }

        #region Refresh modules

        private void RefreshMapInfo()
        {
            changeMapButton.SetActive(LobbyManager.lobbyPlayer.IsHost);

            RefreshSelectedMapState();
            if (LobbyManager.lobby.SelectedMap == null)
            {
                mapImage.texture = defaultMapImage;
                return;
            }

            authorText.text = LobbyManager.lobby.SelectedMap.Author;
            nameText.text = LobbyManager.lobby.SelectedMap.Name;
            mapperText.text = "by " + LobbyManager.lobby.SelectedMap.MapperNick;
            difficultyText.text = LobbyManager.lobby.SelectedDifficulty.name + $"({LobbyManager.lobby.SelectedDifficulty.stars})";

            // TODO: make difficulty stars presenter

            CoversManager.AddPackage(new CoverRequestPackage(mapImage,
                LobbyManager.lobby.SelectedMap.Trackname, LobbyManager.lobby.SelectedMap.MapperNick));
        }
        private void RefreshPlayerSlots()
        {
            ClearAllSlots();
            foreach (LobbyPlayer player in LobbyManager.lobby.Players)
            {
                slots[player.SlotIndex].Refresh(player);
            }
        }
        private void RefreshMods()
        {
            HelperUI.ClearContentAll(modsContainer);

            foreach (ModSO modSO in sodb.mods)
            {
                if (modsUI.selectedMods.Any(c => c.modEnum.HasFlag(modSO.modEnum)))
                {
                    GameObject obj = Instantiate(modItemPrefab, modsContainer);
                    obj.GetComponent<ModsBarItem>().Refresh(modSO);
                }
            }
        }
        private void RefreshReadyButtons()
        {
            if (LobbyManager.lobbyPlayer.IsHost)
            {
                if (LobbyManager.lobbyPlayer.State != LobbyPlayer.ReadyState.Ready)
                {
                    readyBtn.SetActive(true);
                    locationBtn.SetActive(true);

                    notReadyBtn.SetActive(false);
                    startBtn.gameObject.SetActive(false);
                }
                else if (LobbyManager.lobbyPlayer.State == LobbyPlayer.ReadyState.Ready)
                {
                    notReadyBtn.SetActive(true);

                    readyBtn.SetActive(false);
                    locationBtn.SetActive(false);
                    startBtn.gameObject.SetActive(true);

                    if (LobbyManager.lobby.IsPlaying || slots.Where(c => c.player != null).Any(c => c.player.State == LobbyPlayer.ReadyState.Playing))
                    {
                        startBtn.RefreshAsPlaying();
                    }
                    else if (LobbyManager.lobby.SelectedMap == null)
                    {
                        startBtn.RefreshAsMapNotSet();
                    }
                    else if (slots.Where(c => c.player != null).All(c => c.player.State == LobbyPlayer.ReadyState.Ready))
                    {
                        startBtn.RefreshAsStart();
                    }
                    else
                    {
                        startBtn.RefreshAsForce(slots.Count(c => c.player != null && c.player.State != LobbyPlayer.ReadyState.Ready));
                    }
                }
            }
            else
            {
                readyBtn.SetActive(LobbyManager.lobbyPlayer.State != LobbyPlayer.ReadyState.Ready);
                locationBtn.SetActive(LobbyManager.lobbyPlayer.State != LobbyPlayer.ReadyState.Ready);

                notReadyBtn.SetActive(LobbyManager.lobbyPlayer.State == LobbyPlayer.ReadyState.Ready);

                // False due to me isn't host
                startBtn.gameObject.SetActive(false);
            }
        }
        private void RefreshSelectedMapState()
        {
            if (LobbyManager.lobby.IsHostChangingMap)
            {
                hostChangingMessage.SetActive(true);
                hostChangingIcon.SetActive(true);
                noMapSetText.SetActive(false);
                mapInfoTextsContainer.SetActive(false);
                return;
            }

            hostChangingMessage.SetActive(false);
            hostChangingIcon.SetActive(false);

            noMapSetText.SetActive(LobbyManager.lobby.SelectedMap == null);
            mapInfoTextsContainer.SetActive(LobbyManager.lobby.SelectedMap != null);
        }
        //private void RefreshTimeline()
        //{
        //    if (!LobbyManager.lobby.IsPlaying)
        //    {
        //        timeline.SetActive(false);
        //        return;
        //    }

        //    timeline.SetActive(true);
        //    timelineSlider.maxValue = LobbyManager.lobby.MapDuration;
        //    timelineSlider.value = LobbyManager.lobby.CurrentSecond;


        //    RefreshTimelineText();
        //}
        //private void RefreshTimelineText()
        //{
        //    string currentTime = TimeSpan.FromSeconds(LobbyManager.lobby.CurrentSecond).ToString("m:ss");
        //    string durationTime = TimeSpan.FromSeconds(LobbyManager.lobby.MapDuration).ToString("m:ss");

        //    timelineText.text = $"<b>{currentTime}</b> / {durationTime}";
        //}

        public void RefreshLobbiesList()
        {
            if (Payload.Account == null)
            {
                refreshLabel.text = LocalizationManager.Localize("NotLoggedIn");
                createLobbyBtn.interactable = false;
                refreshLobbiesBtn.interactable = false;
                return;
            }
            createLobbyBtn.interactable = true;
            refreshLobbiesBtn.interactable = true;


            foreach (Transform item in lobbyStackParent) Destroy(item.gameObject);
            refreshLobbiesBtn.interactable = false;
            refreshLabel.gameObject.SetActive(true);
            refreshLabel.text = LocalizationManager.Localize("Refreshing");

            Task.Run(async () =>
            {
                List<Lobby> lobbies = await LobbyManager.GetLobbies();

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    HelperUI.FillContent<LobbyPresenter, Lobby>(lobbyStackParent, lobbyItemPrefab, lobbies, (presenter, lobby) =>
                    {
                        presenter.Refresh(lobby);
                    });

                    refreshLobbiesBtn.interactable = true;
                    if(lobbies.Count == 0)
                    {
                        refreshLabel.text = LocalizationManager.Localize("NoLobbies");
                    }
                    else
                    {
                        refreshLabel.gameObject.SetActive(false);
                    }
                });
            });
        }


        #endregion









        #region Create/Join/Leave/Rename/Password lobby

        public void CreateLobby()
        {
            createLobbyBtn.interactable = false;
            Task.Run(async () =>
            {
                await LobbyManager.CreateAndJoinLobby();
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    createLobbyBtn.interactable = true;
                    RefreshLobby();
                });
            });
        }
        public void JoinLobby(Lobby lobbyToJoin)
        {
            modsUI.selectedMods.Clear();
            Task.Run(async () =>
            {
                await LobbyManager.JoinLobby(lobbyToJoin);
                if (LobbyManager.lobby == null) return;
               
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshLobby();
                    DownloadMapIfNeeded();
                    chatUI.ClearChat();
                });
            });
        }
        public void LeaveLobby()
        {
            Task.Run(async () =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    pager.OpenMultiplayerPage();
                    LobbyActionsLocker.instance.Close();
                });
                await LobbyManager.LeaveLobby();
            });
        }
        public void OnRenameLobbyInputFieldChange()
        {
            Task.Run(async () =>
            {
                await LobbyManager.RenameLobby(nameInputField.text);
            });
        }
        public void OnPasswordInputFieldChange()
        {
            Task.Run(async () =>
            {
                await LobbyManager.ChangePassword(passwordInputField.text);
            });
        }

        private void OnLobbyRename(string lobbyName)
        {
            LobbyManager.lobby.Name = lobbyName;
            RefreshLobby();
        }
        private void OnLobbyChangePassword(string password)
        {
            LobbyManager.lobby.Password = password;
            RefreshLobby();
        }
        private void OnLobbyPlayStatusChanged(bool isPlaying)
        {
            LobbyManager.lobby.IsPlaying = isPlaying;
            RefreshReadyButtons();
        }

        #endregion


        #region Map picking

        public void SelectMapButtonClick()
        {
            listUI.RefreshDownloadedList(0);
            mainStateMachine.ChangeState(0);
            pager.OnAuthorBtnClick();
            LobbyManager.isPickingMap = true;

            LobbyManager.StartMapPicking();
        }
        public void OnMapPicked(BasicMapData map, DifficultyInfo diff)
        {
            LobbyManager.isPickingMap = false;

            tracksOverlay.SetActive(false);
            mainUI.SetActive(true);
            lobbyStateMachine.ChangeState(lobbyStateMachine.gameObject);

            Task.Run(async () =>
            {
                await LobbyManager.ChangeMap(map, diff);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshMapInfo();
                });
            });
        }
        public void OnMapPickCancel()
        {
            LobbyManager.CancelMapPicking();
            OnRemoteHostCancelChanging();
        }

        #endregion


        #region Ready state button events

        public void OnReadyButtonClick()
        {
            LobbyManager.ChangeReadyState(LobbyPlayer.ReadyState.Ready);
        }
        public void OnNotReadyButtonClick()
        {
            LobbyManager.ChangeReadyState(LobbyPlayer.ReadyState.NotReady);
        }
        public void OnStartButtonClick()
        {
            NetCore.ServerActions.Multiplayer.StartGame(LobbyManager.lobby.Id);
            //LobbyManager.StartMap();
        }
        

        #endregion



        #region OnRemote (other players activity)

        private void OnRemotePlayerReadyStateChange(string nick, LobbyPlayer.ReadyState state)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (slots == null) return;
                slots.First(c => c.player.Player.Nick == nick).ChangeState(state);
                RefreshPlayerSlots();
                RefreshReadyButtons();
            });
        }
        private void OnRemotePlayerModsChange(string nick, ModEnum mods)
        {
            slots.First(c => c.player.Player.Nick == nick).ChangeMods(mods);
        }
        private void OnRemotePlayerStartDownloading(string nick)
        {
            slots.First(c => c.player.Player.Nick == nick).OnStartDownloading();
        }
        private void OnRemotePlayerDownloadProgress(string nick, int percent)
        {
            slots.First(c => c.player.Player.Nick == nick).OnDownloadProgress(percent);
        }
        private void OnRemotePlayerDownloaded(string nick)
        {
            slots.First(c => c.player.Player.Nick == nick).OnDownloadComplete();
        }
        private void OnMapRemoteChange(BasicMapData map, DifficultyInfo diff)
        {
            LobbyManager.lobby.SelectedMap = map;
            LobbyManager.lobby.SelectedDifficulty = diff;

            LobbyManager.lobby.IsHostChangingMap = false;

            RefreshMapInfo();

            // Reset ready status
            OnNotReadyButtonClick();

            DownloadMapIfNeeded();
        }

        #endregion

        #region Map downloading

        private void DownloadMapIfNeeded()
        {
            if (LobbyManager.lobby.SelectedMap == null) return;

            if (!ProjectManager.IsMapDownloaded(LobbyManager.lobby.SelectedMap.Author, LobbyManager.lobby.SelectedMap.Name, LobbyManager.lobby.SelectedMap.MapperNick))
            {
                progressLocker.SetActive(true);
                progressCircle.CurrentPercent = 0;

                var task = downloader.AddTask(LobbyManager.lobby.SelectedMap.Author + "-" + LobbyManager.lobby.SelectedMap.Name, LobbyManager.lobby.SelectedMap.MapperNick);
                task.OnProgress += OnMapDownloadProgress;
                task.OnDownloaded += OnMapDownloaded;

                LobbyManager.PingStartDownloading();
            }
            else
            {
                progressLocker.SetActive(false);
            }
        }
        private void OnMapDownloaded()
        {
            progressLocker.SetActive(false);
            LobbyManager.PingDownloadCompleted();
        }
        private void OnMapDownloadProgress(int percent)
        {
            progressCircle.CurrentPercent = percent;
            if (percent % 5 == 0)
            {
                LobbyManager.PingDownloadProgress(percent);
            }
        }

        #endregion

        #region Mods

        public void OnModChangeButtonClick()
        {
            StartCoroutine(IEOnModChangeButtonClick());
        }
        private IEnumerator IEOnModChangeButtonClick()
        {
            RemoveBlockedMods();
            pager.ShowModsInLobby();
            yield return modsUI.IEOpen(true);

            RefreshMods();
            LobbyManager.ChangeMods(modsUI.selectedMods, modsUI.selectedModEnum);
        }
        private void RemoveBlockedMods()
        {
            modsUI.selectedMods.RemoveAll(c => c.blockInMultiplayer);
        }

        #endregion

        #region Player Join/Leave/Kick

        private void OnPlayerJoin(LobbyPlayer player)
        {
            LobbyManager.lobby.Players.Add(player);
            RefreshPlayerSlots();
            RefreshReadyButtons();
        }
        private void OnPlayerLeave(LobbyPlayer player)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                LobbyManager.lobby.Players.RemoveAll(c => c.Player.Nick == player.Player.Nick);
                RefreshPlayerSlots();
                RefreshReadyButtons();
            });
        }
        private void OnLobbyPlayerKick(LobbyPlayer player)
        {
            // You're were too bad... you have been kicked ;(
            if (player.Player.Nick == LobbyManager.lobbyPlayer.Player.Nick)
            {
                LobbyManager.RemoteKickMe();
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    pager.OpenMultiplayerPage();
                });
            }
            else
            {
                LobbyManager.RemoteKick(player.Player.Nick);
                RefreshPlayerSlots();
            }
        }

        #endregion

        #region Host

        private void OnHostStartChangingMap()
        {
            LobbyManager.lobby.IsHostChangingMap = true;
            RefreshSelectedMapState();
        }
        private void OnHostChange(LobbyPlayer player)
        {
            LobbyManager.RemoteHostChanged(player.Player.Nick);

            UnityMainThreadDispatcher.Instance().Enqueue(RefreshLobby);
        }
        private void OnRemoteHostCancelChanging()
        {
            LobbyManager.lobby.IsHostChangingMap = false;
            RefreshSelectedMapState();
        }

        #endregion


        private void ClearAllSlots()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }
        }
    }
}
