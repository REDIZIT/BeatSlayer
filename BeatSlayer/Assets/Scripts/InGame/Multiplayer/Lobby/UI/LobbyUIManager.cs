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
using Newtonsoft.Json;
using Pixelplacement;
using ProjectManagement;
using System;
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
        public PlayBtnAnim pageSwitcher;
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

        private Lobby currentLobby;
        private LobbyPlayer myLobbyPlayer;

        public const int MAX_LOBBY_PLAYERS = 10;






        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }
        private void Start()
        {
            slots = new List<LobbySlotPresenter>();
            for (int i = 0; i < MAX_LOBBY_PLAYERS; i++)
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
                NetCore.Subs.OnLobbyMapChange += OnMapRemoteChange;

                NetCore.Subs.OnRemotePlayerReadyStateChange += OnRemotePlayerReadyStateChange;
                NetCore.Subs.OnRemotePlayerModsChange += OnRemotePlayerModsChange;
                NetCore.Subs.OnRemotePlayerStartDownloading += OnRemotePlayerStartDownloading;
                NetCore.Subs.OnRemotePlayerDownloadProgress += OnRemotePlayerDownloadProgress;
                NetCore.Subs.OnRemotePlayerDownloaded += OnRemotePlayerDownloaded;
            });
        }




        public void RefreshLobbiesList()
        {
            foreach (Transform item in lobbyStackParent) Destroy(item.gameObject);

            Task.Run(async () =>
            {
                List<Lobby> lobbies = await NetCore.ServerActions.Lobby.GetLobbies();

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
        public void RefreshMapInfo()
        {
            if (currentLobby.SelectedMap == null)
            {
                mapInfoTextsContainer.SetActive(false);
                noMapSetText.SetActive(true);
                return;
            }

            authorText.text = currentLobby.SelectedMap.Group.Author;
            nameText.text = currentLobby.SelectedMap.Group.Name;
            mapperText.text = "by " + currentLobby.SelectedMap.Nick;
            difficultyText.text = currentLobby.SelectedDifficulty.Name + $"({currentLobby.SelectedDifficulty.Stars})";

            // TODO: make difficulty stars presenter

            CoversManager.AddPackage(new CoverRequestPackage(mapImage, currentLobby.SelectedMap.Trackname, currentLobby.SelectedMap.Nick));
        }










        public void CreateLobby()
        {
            Task.Run(async () =>
            {
                Lobby createdLobby = await NetCore.ServerActions.Lobby.Create(Payload.Account.Nick);

                Lobby lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, createdLobby.Id);
                myLobbyPlayer = lobby.Players.First(c => c.Player.Nick == Payload.Account.Nick);

                UnityMainThreadDispatcher.Instance().Enqueue(() => RefreshLobby(lobby));
            });
        }
        public void JoinLobby(Lobby lobbyToJoin)
        {
            Task.Run(async () =>
            {
                Lobby lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, lobbyToJoin.Id);
                myLobbyPlayer = lobby.Players.First(c => c.Player.Nick == Payload.Account.Nick);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshLobby(lobby);
                    DownloadMapIfNeeded();
                });
            });
        }
        public void LeaveLobby()
        {
            Task.Run(async () =>
            {
                try
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        currentLobby = null;
                        pageSwitcher.OpenMultiplayerPage();
                        LobbyActionsLocker.instance.Close();
                    });
                    await NetCore.ServerActions.Lobby.Leave(Payload.Account.Nick, currentLobby.Id);
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            });
        }
        public void GiveHostRights(string nick)
        {
            NetCore.ServerActions.Lobby.ChangeHost(currentLobby.Id, nick);
        }
        public void Kick(string nick)
        {
            NetCore.ServerActions.Lobby.Kick(currentLobby.Id, nick);
        }







        public void SelectMapButtonClick()
        {
            listUI.RefreshDownloadedList(0);
            mainStateMachine.ChangeState(0);
            pageSwitcher.OnAuthorBtnClick();
            beatmapUI.isSelectingLobbyMap = true;

            NetCore.ServerActions.Lobby.HostStartChangingMap(currentLobby.Id);
        }
        public void OnMapPicked(MapInfo map, DifficultyInfo diff)
        {
            tracksOverlay.SetActive(false);
            mainUI.SetActive(true);
            lobbyStateMachine.ChangeState(lobbyStateMachine.gameObject);

            mapInfoTextsContainer.SetActive(true);
            hostChangingMessage.SetActive(false);
            hostChangingIcon.SetActive(false);
            noMapSetText.SetActive(false);

            Task.Run(() =>
            {
                MapData mapData = new MapData()
                {
                    Group = new GroupData()
                    {
                        Author = map.author,
                        Name = map.name
                    },
                    Nick = map.nick
                };
                currentLobby.SelectedMap = mapData;
                currentLobby.SelectedDifficulty = new DifficultyData(diff);

                NetCore.ServerActions.Lobby.ChangeMap(currentLobby.Id, currentLobby.SelectedMap, currentLobby.SelectedDifficulty);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshMapInfo();
                });
            });
        }





        public void OnReadyButtonClick()
        {
            myLobbyPlayer.State = LobbyPlayer.ReadyState.Ready;

            Task.Run(async () => await NetCore.ServerActions.Lobby.ChangeReadyState(currentLobby.Id, Payload.Account.Nick, LobbyPlayer.ReadyState.Ready));
        }
        public void OnNotReadyButtonClick()
        {
            myLobbyPlayer.State = LobbyPlayer.ReadyState.NotReady;

            Task.Run(async () => await NetCore.ServerActions.Lobby.ChangeReadyState(currentLobby.Id, Payload.Account.Nick, LobbyPlayer.ReadyState.NotReady));
        }

        public void OnRemotePlayerReadyStateChange(string nick, LobbyPlayer.ReadyState state)
        {
            slots.First(c => c.player.Player.Nick == nick).ChangeState(state);

            UnityMainThreadDispatcher.Instance().Enqueue(() => { try { RefreshReadyButtons(); } catch (Exception err) { Debug.LogError(err); } });
        }



        public void OnRemotePlayerModsChange(string nick, ModEnum mods)
        {
            slots.First(c => c.player.Player.Nick == nick).ChangeMods(mods);
        }
        public void OnRemotePlayerStartDownloading(string nick)
        {
            slots.First(c => c.player.Player.Nick == nick).OnStartDownloading();
        }
        public void OnRemotePlayerDownloadProgress(string nick, int percent)
        {
            slots.First(c => c.player.Player.Nick == nick).OnDownloadProgress(percent);
        }
        public void OnRemotePlayerDownloaded(string nick)
        {
            slots.First(c => c.player.Player.Nick == nick).OnDownloadComplete();
        }



        public void OnModChangeButtonClick()
        {
            StartCoroutine(IEOnModChangeButtonClick());
        }





       

        public bool AmIHost()
        {
            return currentLobby.Players.Find(c => c.Player.Nick == Payload.Account.Nick).IsHost;
        }






        private void RefreshLobby(Lobby lobby)
        {
            currentLobby = lobby;

            nameText.text = lobby.Name;
            nameInputField.text = lobby.Name;

            changeMapButton.SetActive(myLobbyPlayer.IsHost);

            FillSlots(lobby);

            RefreshMapInfo();
            RefreshReadyButtons();
            RefreshMods();

            pageSwitcher.OpenLobbyPage();
        }
        private void RefreshReadyButtons()
        {
            if (AmIHost())
            {
                if (myLobbyPlayer.State != LobbyPlayer.ReadyState.Ready)
                {
                    readyBtn.SetActive(true);
                    locationBtn.SetActive(true);

                    notReadyBtn.SetActive(false);
                    forceStartBtn.SetActive(false);
                    startBtn.SetActive(false);
                }
                else if (myLobbyPlayer.State == LobbyPlayer.ReadyState.Ready)
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
                readyBtn.SetActive(myLobbyPlayer.State != LobbyPlayer.ReadyState.Ready);
                locationBtn.SetActive(myLobbyPlayer.State != LobbyPlayer.ReadyState.Ready);

                notReadyBtn.SetActive(myLobbyPlayer.State == LobbyPlayer.ReadyState.Ready);

                // False due to me isn't host
                forceStartBtn.SetActive(false);
                startBtn.SetActive(false);
            }
        }



        private void OnPlayerJoin(LobbyPlayer player)
        {
            currentLobby.Players.Add(player);
            FillSlots(currentLobby);
            RefreshReadyButtons();
        }
        private void OnPlayerLeave(LobbyPlayer player)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                currentLobby.Players.RemoveAll(c => c.Player.Nick == player.Player.Nick);
                FillSlots(currentLobby);
                RefreshReadyButtons();
            });
        }
        private void OnHostStartChangingMap()
        {
            hostChangingMessage.SetActive(true);
            hostChangingIcon.SetActive(true);
            mapInfoTextsContainer.SetActive(false);
        }
        private void OnMapRemoteChange(MapData map, DifficultyData diff)
        {
            hostChangingMessage.SetActive(false);
            hostChangingIcon.SetActive(false);
            noMapSetText.SetActive(false);
            mapInfoTextsContainer.SetActive(true);

            currentLobby.SelectedMap = map;
            currentLobby.SelectedDifficulty = diff;

            RefreshMapInfo();

            // Reset ready status
            OnNotReadyButtonClick();

            DownloadMapIfNeeded();
        }
        private void DownloadMapIfNeeded()
        {
            if (!ProjectManager.IsMapDownloaded(currentLobby.SelectedMap.Group.Author, currentLobby.SelectedMap.Group.Name, currentLobby.SelectedMap.Nick))
            {
                progressLocker.SetActive(true);
                progressCircle.CurrentPercent = 0;

                var task = downloader.AddTask(currentLobby.SelectedMap.Trackname, currentLobby.SelectedMap.Nick);
                task.OnProgress += OnMapDownloadProgress;
                task.OnDownloaded += OnMapDownloaded;

                myLobbyPlayer.State = LobbyPlayer.ReadyState.Downloading;
                NetCore.ServerActions.Lobby.OnStartDownloading(currentLobby.Id, Payload.Account.Nick);
            }
            else
            {
                progressLocker.SetActive(false);
            }
        }
        private void OnMapDownloaded()
        {
            progressLocker.SetActive(false);
            myLobbyPlayer.State = LobbyPlayer.ReadyState.NotReady;
            NetCore.ServerActions.Lobby.OnDownloaded(currentLobby.Id, Payload.Account.Nick);
        }
        private void OnMapDownloadProgress(int percent)
        {
            progressCircle.CurrentPercent = percent;
            if (percent % 5 == 0)
            {
                NetCore.ServerActions.Lobby.OnDownloadProgress(currentLobby.Id, Payload.Account.Nick, percent);
            }
        }

        private void OnHostChange(LobbyPlayer player)
        {
            foreach (LobbyPlayer lobbyPlayer in currentLobby.Players)
            {
                lobbyPlayer.IsHost = lobbyPlayer.Player.Nick == player.Player.Nick;
            }
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                RefreshLobby(currentLobby);
            });
        }
        private void OnLobbyPlayerKick(LobbyPlayer player)
        {
            // You're were too bad... you have been kicked ;(
            if (player.Player.Nick == Payload.Account.Nick)
            {
                //LeaveLobby();
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    currentLobby = null;
                    pageSwitcher.OpenMultiplayerPage();
                });
            }
            else
            {
                currentLobby.Players.RemoveAll(c => c.Player.Nick == player.Player.Nick);
                FillSlots(currentLobby);
            }
        }







        private IEnumerator IEOnModChangeButtonClick()
        {
            yield return modsUI.IEOpen();

            RefreshMods();

            Task.Run(() =>
            {
                NetCore.ServerActions.Lobby.ChangeMods(currentLobby.Id, myLobbyPlayer.Player.Nick, modsUI.selectedModEnum);
            });
        }
        private void ClearAllSlots()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }
        }
        private void FillSlots(Lobby lobby)
        {
            ClearAllSlots();
            foreach (LobbyPlayer player in lobby.Players)
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
    }
}
