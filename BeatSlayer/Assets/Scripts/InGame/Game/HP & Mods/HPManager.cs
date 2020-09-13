using GameNet;
using InGame.Game.Scoring.Mods;
using InGame.Game.Spawn;
using InGame.Multiplayer.Lobby;
using UnityEngine;

namespace InGame.Game.HP
{
    public class HPManager : MonoBehaviour
    {
        public GameManager gm;
        public BeatManager bm;
        public ScoringManager sm;
        public HPBar bar;
        public HPLocker locker;


        public CanvasGroup deadOverlay;



        public bool isAlive = true;
        public float HP
        {
            get { return hp; }
            set { hp = Mathf.Clamp(value, 0, 100); }
        }
        private float hp;



        [Header("HP balance values")]
        [Tooltip("Count of HP to give player when block sliced")]
        public float rewardHP;
        [Tooltip("Count of HP to reduce when block missed or bomb exploded")]
        public float punishHP;

        public bool IsNoFail { get; private set; }
        public bool IsEasy { get; private set; }
        public bool IsHard { get; private set; }
        public bool IsInstantDeath { get; private set; }
        private bool IsMultiplayer { get; set; }



        private void Awake()
        {
            isAlive = true;
            HP = 100;

            bar.manager = this;
            bar.playerNick = Payload.Account == null ? "Player" : Payload.Account.Nick;
            locker.manager = this;

            gm.OnCubeMiss += OnMiss;
            gm.OnCubeSlice += OnSlice;
            gm.OnLineSlice += OnSlice;
            gm.OnBombSlice += OnMiss;
            gm.OnBombMiss += OnSlice;

            IsMultiplayer = LobbyManager.lobby != null;
        }
        private void Start()
        {
            IsNoFail = (sm.Replay.Mods & ModEnum.NoFail) == ModEnum.NoFail;
            IsEasy = sm.Replay.Mods.HasFlag(ModEnum.Easy);
            IsHard = sm.Replay.Mods.HasFlag(ModEnum.Hard);
            IsInstantDeath = sm.Replay.Mods.HasFlag(ModEnum.OneTry);


            rewardHP *= IsEasy ? 1.25f : IsHard ? 0.75f : 1;
            punishHP *= IsEasy ? 0.75f : IsHard ? 1.25f : 1;
        }
        private void Update()
        {
            if (isAlive) return;

            //     -x + offset
            // y = ——————————— + 1
            //       fadeHP
            deadOverlay.alpha = (-HP + 20) / 20f + 1;

            if (deadOverlay.alpha <= 0)
            {
                // Relive

                isAlive = true;
                deadOverlay.gameObject.SetActive(false);

                NetCore.ServerActions.Multiplayer.AliveChanged(LobbyManager.lobby.Id, Payload.Account.Nick, true);
            }
        }


        public void OnSlice()
        {
            if (!isAlive && !IsMultiplayer) return;

            HP += rewardHP;
        }
        public void OnMiss()
        {
            if (!isAlive && !IsMultiplayer) return;

            if (IsInstantDeath) HP = 0;
            else HP -= punishHP;

            if (HP <= 0)
            {
                KillPlayer();
            }
        }
        public void KillPlayer()
        {
            if (IsNoFail) return;
            if (!isAlive) return;

            if (IsMultiplayer)
            {
                isAlive = false;

                deadOverlay.gameObject.SetActive(true);

                NetCore.ServerActions.Multiplayer.AliveChanged(LobbyManager.lobby.Id, Payload.Account.Nick, false);
            }
            else
            {
                isAlive = false;

                bm.OnGameOver();
            }
        }
    }
}
