using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Animations;
using Newtonsoft.Json;
using Pixelplacement;
using ProjectManagement;
using System;
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

        [Header("States")]
        public State mainStateMachine;
        public State lobbyStateMachine;
        public PlayBtnAnim pageSwitcher;
        public GameObject tracksOverlay, mainUI;

        [Header("Lobby page")]
        public Text lobbyNameText;
        public InputField nameInputField;
        public GameObject readyBtn, notReadyBtn, startBtn;

        [Header("Map info")]
        public Text authorText;
        public Text nameText, mapperText;

        [Header("Players list")]
        public List<LobbySlotPresenter> slots;

        public Transform slotStackParent;
        public GameObject slotPrefab;

        [Header("Lobbies list")]
        public Transform lobbyStackParent;
        public GameObject lobbyItemPrefab;

        private Lobby currentLobby;

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
                NetCore.Subs.OnLobbyMapChange += OnMapChanged;
                NetCore.Subs.OnRemotePlayerReadyStateChange += OnRemotePlayerReadyStateChange;
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
            authorText.text = currentLobby.SelectedMap.Group.Author;
            nameText.text = currentLobby.SelectedMap.Group.Name;
            mapperText.text = currentLobby.SelectedMap.Nick;
        }

        








        public void CreateLobby()
        {
            Task.Run(async () =>
            {
                Lobby createdLobby = await NetCore.ServerActions.Lobby.Create(Payload.Account.Nick);
                Debug.Log("Created successfuly. " + JsonConvert.SerializeObject(createdLobby));

                Lobby lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, createdLobby.Id);
                Debug.Log("Joined. " + JsonConvert.SerializeObject(lobby));

                UnityMainThreadDispatcher.Instance().Enqueue(() => RefreshLobby(lobby));
            });
        }
        public void JoinLobby(Lobby lobbyToJoin)
        {
            Task.Run(async () =>
            {
                Lobby lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, lobbyToJoin.Id);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshLobby(lobby);
                });
            });
        }
        public void LeaveLobby()
        {
            Task.Run(async () =>
            {
                try
                {
                    await NetCore.ServerActions.Lobby.Leave(Payload.Account.Nick, currentLobby.Id);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        currentLobby = null;
                        pageSwitcher.OpenMultiplayerPage();
                    });
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
        }
        public void OnMapSelected(MapInfo map)
        {
            tracksOverlay.SetActive(false);
            mainUI.SetActive(true);
            lobbyStateMachine.ChangeState(lobbyStateMachine.gameObject);

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

                //await NetCore.ServerActions.Lobby.ChangeMap(currentLobby.Id, mapData);
                NetCore.ServerActions.Lobby.ChangeMap(currentLobby.Id, mapData);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    RefreshMapInfo();
                });
            });            
        }




        public void OnReadyButtonClick()
        {
            readyBtn.SetActive(false);
            notReadyBtn.SetActive(true);
            Task.Run(async () => await NetCore.ServerActions.Lobby.ChangeReadyState(currentLobby.Id, Payload.Account.Nick, LobbyPlayer.ReadyState.Ready));
        }
        public void OnNotReadyButtonClick()
        {
            readyBtn.SetActive(true);
            notReadyBtn.SetActive(false);
            Task.Run(async () => await NetCore.ServerActions.Lobby.ChangeReadyState(currentLobby.Id, Payload.Account.Nick, LobbyPlayer.ReadyState.NotReady));
        }
        public void OnRemotePlayerReadyStateChange(string nick, LobbyPlayer.ReadyState state)
        {
            slots.First(c => c.player.Player.Nick == nick).ChangeState(state);
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

            FillSlots(lobby);

            RefreshMapInfo();

            pageSwitcher.OpenLobbyPage();
        }
        private void OnPlayerJoin(LobbyPlayer player)
        {
            currentLobby.Players.Add(player);

            FillSlots(currentLobby);
        }
        private void OnPlayerLeave(LobbyPlayer player)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                currentLobby.Players.RemoveAll(c => c.Player.Nick == player.Player.Nick);
                FillSlots(currentLobby);
            });
        }
        private void OnMapChanged(MapData map)
        {
            currentLobby.SelectedMap = map;

            RefreshMapInfo();
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
    }
}
