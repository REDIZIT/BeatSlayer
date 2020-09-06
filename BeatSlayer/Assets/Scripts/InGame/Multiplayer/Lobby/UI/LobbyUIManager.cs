using Assets.SimpleLocalization;
using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using CoversManagement;
using GameNet;
using InGame.Animations;
using InGame.Game.Mods;
using InGame.Game.Scoring.Mods;
using InGame.Helpers;
using InGame.Menu.Maps;
using InGame.Menu.Mods;
using InGame.ScriptableObjects;
using Michsky.UI.ModernUIPack;
using Pixelplacement;
using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using MapInfo = ProjectManagement.MapInfo;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager instance;

        [Header("Components")]
        public BeatmapUI beatmapUI;
        public TrackListUI listUI;
        public ModsUI modsUI;
        public SODB sodb;
        public MapsDownloadQueuer downloader;

        [Header("States")]
        public State mainStateMachine;
        public State lobbyStateMachine;
        public PlayBtnAnim pager;
        public GameObject tracksOverlay, mainUI;

        [Header("Lobby page")]
        public Text lobbyNameText;
        public InputField nameInputField;
        public GameObject readyBtn, forceStartBtn, notReadyBtn, startBtn, locationBtn;
        public Text notReadyPlayersCountText;
        public GameObject changeMapButton;

        [Header("Mods")]
        public Transform modsContainer;
        public GameObject modItemPrefab;

        [Header("Map info")]
        public RawImage mapImage;
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




        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }
        private void Start()
        {
            slots = new List<LobbySlotPresenter>();
            for (int i = 0; i < LobbyManager.MAX_LOBBY_PLAYERS; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotStackParent);
                slots.Add(slotObj.GetComponent<LobbySlotPresenter>());
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

                NetCore.Subs.OnRemotePlayerReadyStateChange += OnRemotePlayerReadyStateChange;
                NetCore.Subs.OnRemotePlayerModsChange += OnRemotePlayerModsChange;
                NetCore.Subs.OnRemotePlayerStartDownloading += OnRemotePlayerStartDownloading;
                NetCore.Subs.OnRemotePlayerDownloadProgress += OnRemotePlayerDownloadProgress;
                NetCore.Subs.OnRemotePlayerDownloaded += OnRemotePlayerDownloaded;
            });
        }




        public void RefreshLobby()
        {
            nameText.text = LobbyManager.lobby.Name;
            nameInputField.text = LobbyManager.lobby.Name;


            RefreshPlayerSlots();
            RefreshMapInfo();
            RefreshReadyButtons();
            RefreshMods();

            pager.OpenLobbyPage();
        }

        #region Refresh modules

        private void RefreshMapInfo()
        {
            changeMapButton.SetActive(LobbyManager.lobbyPlayer.IsHost);

            RefreshSelectedMapState();

            authorText.text = LobbyManager.lobby.SelectedMap.Group.Author;
            nameText.text = LobbyManager.lobby.SelectedMap.Group.Name;
            mapperText.text = "by " + LobbyManager.lobby.SelectedMap.Nick;
            difficultyText.text = LobbyManager.lobby.SelectedDifficulty.Name + $"({LobbyManager.lobby.SelectedDifficulty.Stars})";

            // TODO: make difficulty stars presenter

            CoversManager.AddPackage(new CoverRequestPackage(mapImage, LobbyManager.lobby.SelectedMap.Trackname, LobbyManager.lobby.SelectedMap.Nick));
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
                    forceStartBtn.SetActive(false);
                    startBtn.SetActive(false);
                }
                else if (LobbyManager.lobbyPlayer.State == LobbyPlayer.ReadyState.Ready)
                {
                    notReadyBtn.SetActive(true);

                    readyBtn.SetActive(false);
                    locationBtn.SetActive(false);

                    if (slots.Where(c => c.player != null).All(c => c.player.State == LobbyPlayer.ReadyState.Ready))
                    {
                        startBtn.SetActive(true);
                        forceStartBtn.SetActive(false);
                    }
                    else
                    {
                        forceStartBtn.SetActive(true);
                        startBtn.SetActive(false);

                        notReadyPlayersCountText.text = LocalizationManager.Localize("NotReadyPlayers", slots.Count(c => c.player != null && c.player.State != LobbyPlayer.ReadyState.Ready));
                    }
                }
            }
            else
            {
                readyBtn.SetActive(LobbyManager.lobbyPlayer.State != LobbyPlayer.ReadyState.Ready);
                locationBtn.SetActive(LobbyManager.lobbyPlayer.State != LobbyPlayer.ReadyState.Ready);

                notReadyBtn.SetActive(LobbyManager.lobbyPlayer.State == LobbyPlayer.ReadyState.Ready);

                // False due to me isn't host
                forceStartBtn.SetActive(false);
                startBtn.SetActive(false);
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

        #endregion



        public void RefreshLobbiesList()
        {
            foreach (Transform item in lobbyStackParent) Destroy(item.gameObject);

            Task.Run(async () =>
            {
                List<Lobby> lobbies = await LobbyManager.GetLobbies();

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    foreach (var lobby in lobbies)
                    {
                        GameObject obj = Instantiate(lobbyItemPrefab, lobbyStackParent);
                        LobbyPresenter presenter = obj.GetComponent<LobbyPresenter>();
                        presenter.Refresh(lobby);
                    }
                });
            });
        }





        #region Create/Join/Leave lobby

        public void CreateLobby()
        {
            Task.Run(async () =>
            {
                await LobbyManager.CreateAndJoinLobby();
                UnityMainThreadDispatcher.Instance().Enqueue(RefreshLobby);
            });
        }
        public void JoinLobby(Lobby lobbyToJoin)
        {
            modsUI.selectedMods.Clear();
            Task.Run(async () =>
            {
                await LobbyManager.JoinLobby(lobbyToJoin);
               
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshLobby();
                    DownloadMapIfNeeded();
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
        public void OnMapPicked(MapInfo map, DifficultyInfo diff)
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

        #endregion



        #region OnRemote (other players activity)

        private void OnRemotePlayerReadyStateChange(string nick, LobbyPlayer.ReadyState state)
        {
            slots.First(c => c.player.Player.Nick == nick).ChangeState(state);

            UnityMainThreadDispatcher.Instance().Enqueue(RefreshReadyButtons);
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
        private void OnMapRemoteChange(MapData map, DifficultyData diff)
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

            if (!ProjectManager.IsMapDownloaded(LobbyManager.lobby.SelectedMap.Group.Author, LobbyManager.lobby.SelectedMap.Group.Name, LobbyManager.lobby.SelectedMap.Nick))
            {
                progressLocker.SetActive(true);
                progressCircle.CurrentPercent = 0;

                var task = downloader.AddTask(LobbyManager.lobby.SelectedMap.Trackname, LobbyManager.lobby.SelectedMap.Nick);
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
            pager.ShowModsInLobby();
            yield return modsUI.IEOpen();

            RefreshMods();
            LobbyManager.ChangeMods(modsUI.selectedModEnum);
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
